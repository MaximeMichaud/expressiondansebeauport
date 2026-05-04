# Group Join Requests — Design Spec

## Summary

Allow members to request to join a group without knowing the invite code. The request is sent as an interactive message to every professor in the group. A professor can accept (adds the member to the group) or reject the request directly from the conversation. The first professor to respond resolves the request for everyone.

## Decisions

- Request is sent to all `GroupMember` with `Role = Professor` in the target group — admins only receive it if they are also a professor-member of that group
- First professor to accept/reject wins — other professors see "Already handled by [Prof X]"
- Member receives an auto-reply in the conversation with the resolving professor ("accepted" or "rejected")
- One pending request per member per group at a time — duplicate blocked with 409
- After rejection the member can re-request (status resets to available)
- No custom message from the requester — fixed system text only
- The existing invite-code flow remains untouched alongside this new option

## Data Model

### New entity: `JoinRequest`

| Field | Type | Notes |
|---|---|---|
| Id | Guid | PK |
| GroupId | Guid | FK → Group |
| RequesterMemberId | Guid | FK → Member |
| Status | JoinRequestStatus | Pending / Accepted / Rejected |
| ResolvedByMemberId | Guid? | FK → Member (the professor who acted) |
| ResolvedAt | Instant? | |
| CreatedAt | Instant | |

### New enum: `JoinRequestStatus`

```
Pending = 0
Accepted = 1
Rejected = 2
```

### New enum: `MessageType`

```
Text = 0
JoinRequest = 1
```

### Modifications to `Message`

| Field | Type | Notes |
|---|---|---|
| MessageType | MessageType | Default `Text` — no impact on existing messages |
| JoinRequestId | Guid? | FK → JoinRequest, nullable |

### EF Core migration

- New table `JoinRequests`
- Two new columns on `Messages`: `MessageType` (int, default 0), `JoinRequestId` (Guid?, FK)
- Index on `JoinRequests` (GroupId, RequesterMemberId, Status) for fast pending-check

## API Endpoints

### `POST /social/groups/{groupId}/join-requests`

- **Auth:** Member, Professor, Administrator
- **Logic:**
  1. Verify member is not already in the group → 400
  2. Check no `JoinRequest` with Status=Pending exists for this member+group → 409
  3. Create `JoinRequest` (Status=Pending)
  4. Find all `GroupMember` where `Role = Professor` for this group
  5. For each professor: get-or-create a 1-to-1 conversation, send a `Message` with `MessageType=JoinRequest` and `JoinRequestId` set, content = "[Nom] souhaite rejoindre le groupe [Groupe]"
  6. Notify each professor via SignalR (`ReceiveMessage`)
- **Response:** 200 with `{ joinRequestId }`

### `PUT /social/join-requests/{id}/accept`

- **Auth:** Professor or Administrator (must be a professor-member of the group)
- **Logic:**
  1. Load JoinRequest — must be Pending → 400 if already resolved
  2. Verify caller is a professor in JoinRequest.Group → 403
  3. Set Status=Accepted, ResolvedByMemberId, ResolvedAt
  4. Add requester to group by creating a `GroupMember` with Role=Member (direct insert, no invite code needed)
  5. Send auto-reply message in the conversation between requester and this professor: "✅ [Prof] a accepté votre demande pour [Groupe]"
  6. Push SignalR event `JoinRequestResolved` to:
     - The requester (so they see the reply + group appears)
     - All other professors who received the request (so their cards update)
- **Response:** 200

### `PUT /social/join-requests/{id}/reject`

- **Auth:** Same as accept
- **Logic:**
  1. Load JoinRequest — must be Pending → 400
  2. Verify caller is professor in group → 403
  3. Set Status=Rejected, ResolvedByMemberId, ResolvedAt
  4. Send auto-reply: "❌ [Prof] a refusé votre demande pour [Groupe]"
  5. Push SignalR `JoinRequestResolved` to requester + other professors
- **Response:** 200

### `GET /social/groups/{groupId}/join-requests/mine`

- **Auth:** Member, Professor, Administrator
- **Logic:** Return the caller's Pending `JoinRequest` for this group, or null
- **Response:** 200 with `{ joinRequestId, status }` or `null`

## Frontend

### Modified: `SocialPortal.vue` — Join Modal

The current modal shows only an invite-code input. Replace with a two-option modal:

**State: default (no pending request)**
```
Rejoindre « [Group Name] »

[Button: Demander à rejoindre]
[Divider: — ou —]
[Link/Button: J'ai un code d'invitation]
```

Clicking "J'ai un code" reveals the existing invite-code input + Rejoindre button (same as current).

Clicking "Demander à rejoindre" → calls `POST /social/groups/{groupId}/join-requests` → shows success state → closes modal.

**State: pending request exists**
```
Rejoindre « [Group Name] »

[Disabled button: Demande envoyée ✓]
[Divider: — ou —]
[Link/Button: J'ai un code d'invitation]
```

On modal open, call `GET .../join-requests/mine` to determine which state to show.

### Modified: `SocialConversation.vue`

In the message rendering loop, add a branch:

```
if msg.messageType === 'JoinRequest' → render <JoinRequestCard>
else → render normal bubble (existing code)
```

### New component: `JoinRequestCard.vue`

Located in `src/Web/vue-app/src/components/social/`.

**Props:**
- `joinRequestId` (string)
- `groupName` (string)
- `requesterName` (string)
- `status` ('pending' | 'accepted' | 'rejected')
- `resolvedByName` (string | null)
- `isMine` (boolean — whether the current user is the requester)

**Rendering:**

When `status === 'pending'` and viewer is a professor of the group:
```
┌─────────────────────────────┐
│ 👤 [requesterName]          │
│ souhaite rejoindre [group]  │
│                             │
│ [Accepter]  [Refuser]       │
└─────────────────────────────┘
```

When `status === 'pending'` and viewer is the requester:
```
┌─────────────────────────────┐
│ Vous avez demandé à         │
│ rejoindre [group]           │
│ En attente...               │
└─────────────────────────────┘
```

When `status === 'accepted'`:
```
┌─────────────────────────────┐
│ ✅ Accepté par [resolvedBy] │
└─────────────────────────────┘
```

When `status === 'rejected'`:
```
┌─────────────────────────────┐
│ ❌ Refusé par [resolvedBy]  │
└─────────────────────────────┘
```

**Events:** `@accept(joinRequestId)`, `@reject(joinRequestId)`

### Modified: `socialService.ts`

New methods:
- `requestJoinGroup(groupId: string): Promise<{ joinRequestId: string }>`
- `acceptJoinRequest(joinRequestId: string): Promise<SucceededOrNotResponse>`
- `rejectJoinRequest(joinRequestId: string): Promise<SucceededOrNotResponse>`
- `getMyJoinRequest(groupId: string): Promise<{ joinRequestId: string, status: string } | null>`

### Modified: `ChatHub.cs` / SignalR

New event `JoinRequestResolved`:
- Payload: `{ joinRequestId, status, resolvedByName, groupId, groupName }`
- Sent to: requester + all professors of the group
- Frontend handler: updates the `JoinRequestCard` status reactively (no page refresh needed)

### Modified: Message API responses

`GetMessagesEndpoint` and `SendMessageEndpoint` responses need to include `messageType`, `joinRequestId`, and the join request details (status, groupName, requesterName, resolvedByName) when `messageType === JoinRequest`. This can be a nested object in the message payload.

## Backend Services

### New: `IJoinRequestService` / `JoinRequestService`

Methods:
- `CreateRequest(Guid groupId, Guid requesterMemberId)` — validates, creates entity, sends messages to professors
- `AcceptRequest(Guid joinRequestId, Guid professorMemberId)` — resolves, adds member to group, sends reply
- `RejectRequest(Guid joinRequestId, Guid professorMemberId)` — resolves, sends reply
- `GetPendingRequest(Guid groupId, Guid memberId)` — returns pending request or null

### New: `IJoinRequestRepository` / `JoinRequestRepository`

Standard CRUD + `FindPendingByGroupAndMember(Guid groupId, Guid memberId)`.

## Error Cases

| Scenario | Response |
|---|---|
| Member already in group | 400 "Already a member" |
| Pending request exists | 409 "Request already pending" |
| JoinRequest not found | 404 |
| JoinRequest already resolved | 400 "Already resolved" |
| Caller is not a professor in the group | 403 |
| Group has no professors | 400 "No professors in this group" |
