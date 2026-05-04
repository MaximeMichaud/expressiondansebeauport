# Admin DM Viewer — Design Spec

## Overview

Admins can view all members' direct message conversations in a read-only mode. This is accessed via a new "Admin" tab on the existing Social Messages page (`SocialMessages.vue`). The admin selects a member, sees that member's conversations, clicks into one, and reads the full thread — no sending, deleting, or other actions.

## User Flow

1. Admin opens `/social/messages` — sees two tabs: "Mes messages" (default, their own DMs) and "Admin" (visible only to admins)
2. Admin clicks "Admin" tab — sees a member search input
3. Admin searches and selects a member — sees that member's conversation list (other participant name, last message preview, timestamps)
4. Admin clicks a conversation — navigates to `/social/messages/admin/{conversationId}` showing the full thread in read-only mode (no input bar, no delete options, no context menus)
5. Real-time: new messages appear via existing SignalR hub

## Backend

### New Endpoints (admin-only)

Both endpoints restricted to `Roles.ADMINISTRATOR` only.

#### 1. `GET /social/admin/members/{memberId}/conversations?page=1`

Returns paginated conversations for a given member. Reuses existing `IConversationRepository.GetForMember()` and `IConversationService.GetConversations()` logic but for any member, not just the authenticated user.

**Request:** `memberId` (route param), `page` (query param)

**Response:** Same shape as `GET /social/conversations` but with both participants shown:
```json
{
  "items": [
    {
      "id": "conversation-guid",
      "participantA": { "id": "...", "fullName": "...", "profileImageUrl": "..." },
      "participantB": { "id": "...", "fullName": "...", "profileImageUrl": "..." },
      "lastMessage": { "content": "...", "senderName": "...", "created": "..." },
      "unreadCount": 0
    }
  ],
  "hasMore": false
}
```

Note: `unreadCount` is from the selected member's perspective (how many messages they haven't read).

**Location:** `src/Web/Features/Social/Admin/GetAdminConversationsEndpoint.cs`

#### 2. `GET /social/admin/conversations/{conversationId}/messages?page=1`

Returns paginated messages for any conversation. Reuses existing `IMessageRepository.GetByConversation()`.

**Request:** `conversationId` (route param), `page` (query param)

**Response:** Same shape as existing `GET /social/conversations/{id}/messages`.

**Location:** `src/Web/Features/Social/Admin/GetAdminMessagesEndpoint.cs`

### Service Layer

No new service methods needed. The existing `ConversationService.GetConversations(memberId, page)` and `ConversationService.GetMessages(conversationId, page)` already accept arbitrary IDs — the current endpoints just pass the authenticated user's member ID. The new admin endpoints pass the target member's ID instead.

### SignalR

No hub changes. The admin conversation view will poll or listen for messages on the conversation like any other client. The existing `ChatHub` broadcasts `ReceiveMessage` to connected users — the admin frontend will filter for messages matching the currently viewed conversation.

## Frontend

### SocialMessages.vue Changes

Add a tab system at the top of the page (visible only to admins):

```
[Mes messages]  [Admin]
```

- Non-admins see no tabs, just the existing page as-is
- "Mes messages" tab: current behavior, unchanged
- "Admin" tab content:
  - Member search input (reuses existing `socialService.searchMembers()`)
  - After selecting a member: show that member's conversations via new `socialService.getAdminConversations(memberId, page)`
  - Conversation items show both participant names (not "otherMember" since admin is outside the conversation)
  - Clicking a conversation navigates to the admin read-only view

### New Route

Add route: `/social/messages/admin/:conversationId`
- Name: `socialAdminConversation`
- Component: `SocialConversation.vue` (reused with `readonly` prop)
- Meta: `requiredRole: [Role.Admin]`

### SocialConversation.vue Changes

Add a `readonly` route-based mode:

- Detect via route name (`socialAdminConversation`) or a route meta/query param
- When readonly:
  - Hide the entire input bar (`soc-convo__input-bar`)
  - Hide delete triggers (`soc-convo__delete-trigger`)
  - Disable context menu delete (`@contextmenu.prevent` handler returns early)
  - Hide the delete confirmation modal
  - Show both participant names in the header (instead of just "otherMember")
- Messages still load via `socialService.getAdminMessages(conversationId, page)`
- SignalR still delivers real-time updates
- Back button navigates to `/social/messages` with the admin tab active (preserve selected member via query param or route state)

### New Service Methods

Add to `socialService.ts`:

```typescript
async getAdminConversations(memberId: string, page: number) // calls GET /social/admin/members/{memberId}/conversations
async getAdminMessages(conversationId: string, page: number) // calls GET /social/admin/conversations/{conversationId}/messages
```

### Role Check

Use existing `useUserStore().hasRole(Role.Admin)` to conditionally show the Admin tab.

## What We Reuse

| Component | Reuse Strategy |
|-----------|---------------|
| `ConversationService` | Call existing methods with target member ID |
| `IConversationRepository` | Existing `GetForMember()` and message queries |
| `SocialConversation.vue` | Add readonly mode, same component |
| `SocialMessages.vue` | Add tab + admin panel to existing page |
| `socialService.searchMembers()` | Already exists for member picker |
| `ChatHub` (SignalR) | No changes, real-time works as-is |

## What We Don't Build

- No message deletion by admin
- No message sending by admin
- No moderation actions (ban, flag)
- No audit logging of admin views
- No new database tables or migrations
