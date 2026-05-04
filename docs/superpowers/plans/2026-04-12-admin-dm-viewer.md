# Admin DM Viewer — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Let admins browse any member's DM conversations in a read-only view, via a new "Admin" tab on the existing Social Messages page.

**Architecture:** Two new admin-only backend endpoints reuse existing `ConversationService` methods with a target member ID instead of the authenticated user's ID. Frontend adds an admin tab to `SocialMessages.vue` with a member picker, and a readonly mode to `SocialConversation.vue` that hides all write controls.

**Tech Stack:** C# / FastEndpoints (backend), Vue 3 + TypeScript + Tailwind (frontend)

---

### Task 1: Backend — Admin Get Conversations Endpoint

**Files:**
- Create: `src/Web/Features/Social/Admin/GetAdminConversationsRequest.cs`
- Create: `src/Web/Features/Social/Admin/GetAdminConversationsEndpoint.cs`

- [ ] **Step 1: Create the request class**

Create `src/Web/Features/Social/Admin/GetAdminConversationsRequest.cs`:

```csharp
namespace Web.Features.Social.Admin;

public class GetAdminConversationsRequest
{
    public Guid MemberId { get; set; }
    public int Page { get; set; } = 1;
}
```

- [ ] **Step 2: Create the endpoint**

Create `src/Web/Features/Social/Admin/GetAdminConversationsEndpoint.cs`:

```csharp
using Application.Services.Messaging;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Admin;

public class GetAdminConversationsEndpoint : Endpoint<GetAdminConversationsRequest>
{
    private readonly IConversationService _conversationService;

    public GetAdminConversationsEndpoint(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("social/admin/members/{MemberId}/conversations");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetAdminConversationsRequest req, CancellationToken ct)
    {
        var conversationsResult = await _conversationService.GetConversations(req.MemberId, req.Page);
        var conversations = conversationsResult.Items;

        var results = conversations.Select(c =>
        {
            var lastMsg = c.Messages?.OrderByDescending(m => m.Created).FirstOrDefault();
            var participant = c.Participants?.FirstOrDefault(p => p.MemberId == req.MemberId);
            var unread = 0;
            if (lastMsg != null && lastMsg.SenderMemberId != req.MemberId)
            {
                if (participant?.LastReadAt == null || lastMsg.Created > participant.LastReadAt)
                    unread = 1;
            }

            return new
            {
                c.Id,
                ParticipantA = new
                {
                    Id = c.ParticipantAMemberId,
                    FullName = c.ParticipantA?.FullName ?? "Inconnu",
                    ProfileImageUrl = c.ParticipantA?.ProfileImageUrl,
                    AvatarColor = c.ParticipantA?.AvatarColor ?? "#1a1a1a"
                },
                ParticipantB = new
                {
                    Id = c.ParticipantBMemberId,
                    FullName = c.ParticipantB?.FullName ?? "Inconnu",
                    ProfileImageUrl = c.ParticipantB?.ProfileImageUrl,
                    AvatarColor = c.ParticipantB?.AvatarColor ?? "#1a1a1a"
                },
                LastMessage = lastMsg != null ? new
                {
                    Content = lastMsg.Content,
                    SenderName = lastMsg.SenderMember?.FullName ?? "",
                    Created = lastMsg.Created.ToDateTimeUtc().ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    MediaCount = lastMsg.Media?.Count ?? 0,
                    HasVideo = lastMsg.Media != null && lastMsg.Media.Any(m => m.ContentType != null && m.ContentType.StartsWith("video/")),
                    HasImage = lastMsg.Media != null && lastMsg.Media.Any(m => m.ContentType != null && m.ContentType.StartsWith("image/")),
                    HasLegacyMedia = !string.IsNullOrEmpty(lastMsg.MediaUrl)
                } : null,
                UnreadCount = unread
            };
        });

        await Send.OkAsync(new { Items = results, conversationsResult.HasMore }, ct);
    }
}
```

- [ ] **Step 3: Build and verify**

Run: `dotnet build src/Web/Web.csproj`
Expected: Build succeeds with no errors.

- [ ] **Step 4: Commit**

```bash
git add src/Web/Features/Social/Admin/
git commit -m "feat(social): add admin get-conversations endpoint"
```

---

### Task 2: Backend — Admin Get Messages Endpoint

**Files:**
- Create: `src/Web/Features/Social/Admin/GetAdminMessagesRequest.cs`
- Create: `src/Web/Features/Social/Admin/GetAdminMessagesEndpoint.cs`

- [ ] **Step 1: Create the request class**

Create `src/Web/Features/Social/Admin/GetAdminMessagesRequest.cs`:

```csharp
namespace Web.Features.Social.Admin;

public class GetAdminMessagesRequest
{
    public Guid ConversationId { get; set; }
    public int Page { get; set; } = 1;
}
```

- [ ] **Step 2: Create the endpoint**

Create `src/Web/Features/Social/Admin/GetAdminMessagesEndpoint.cs`.

This endpoint mirrors `GetMessagesEndpoint` (at `src/Web/Features/Social/Messages/GetMessages/GetMessagesEndpoint.cs`) but is admin-only and doesn't filter by authenticated user. It returns messages for any conversation with both participants' read status omitted (admin doesn't need IsRead).

```csharp
using Application.Services.Messaging;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Admin;

public class GetAdminMessagesEndpoint : Endpoint<GetAdminMessagesRequest>
{
    private readonly IConversationService _conversationService;

    public GetAdminMessagesEndpoint(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("social/admin/conversations/{ConversationId}/messages");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetAdminMessagesRequest req, CancellationToken ct)
    {
        var messagesResult = await _conversationService.GetMessages(req.ConversationId, req.Page);
        var messages = messagesResult.Items;

        var results = messages.Select(m =>
        {
            var mediaList = m.Media?
                .OrderBy(x => x.SortOrder)
                .Select(x => new
                {
                    x.Id,
                    MediaUrl = x.MediaUrl,
                    ThumbnailUrl = x.ThumbnailUrl,
                    OriginalUrl = x.OriginalUrl,
                    x.ContentType,
                    x.Size,
                    x.SortOrder
                })
                .Cast<object>()
                .ToList() ?? new List<object>();

            if (mediaList.Count == 0 && !string.IsNullOrEmpty(m.MediaUrl))
            {
                mediaList.Add(new
                {
                    Id = Guid.Empty,
                    MediaUrl = m.MediaUrl,
                    ThumbnailUrl = m.MediaThumbnailUrl,
                    OriginalUrl = m.MediaOriginalUrl,
                    ContentType = "image/webp",
                    Size = 0L,
                    SortOrder = 0
                });
            }

            return new
            {
                m.Id,
                m.ConversationId,
                SenderMemberId = m.SenderMemberId,
                SenderName = m.SenderMember?.FullName ?? "Inconnu",
                Content = m.Deleted.HasValue ? null : m.Content,
                Media = m.Deleted.HasValue ? new List<object>() : mediaList,
                Created = m.Created.ToDateTimeUtc().ToString("yyyy-MM-ddTHH:mm:ssZ"),
                IsDeleted = m.Deleted.HasValue,
                MessageType = m.MessageType.ToString(),
                JoinRequest = m.JoinRequest != null ? new
                {
                    m.JoinRequest.Id,
                    m.JoinRequest.GroupId,
                    GroupName = m.JoinRequest.Group?.Name,
                    RequesterMemberId = m.JoinRequest.RequesterMemberId,
                    RequesterName = m.JoinRequest.RequesterMember?.FullName ?? "Inconnu",
                    Status = m.JoinRequest.Status.ToString(),
                    ResolvedByName = m.JoinRequest.ResolvedByMember?.FullName
                } : null
            };
        });

        await Send.OkAsync(new { Items = results, messagesResult.HasMore }, ct);
    }
}
```

- [ ] **Step 3: Build and verify**

Run: `dotnet build src/Web/Web.csproj`
Expected: Build succeeds with no errors.

- [ ] **Step 4: Commit**

```bash
git add src/Web/Features/Social/Admin/GetAdminMessagesRequest.cs src/Web/Features/Social/Admin/GetAdminMessagesEndpoint.cs
git commit -m "feat(social): add admin get-messages endpoint"
```

---

### Task 3: Frontend — Add admin service methods

**Files:**
- Modify: `src/Web/vue-app/src/services/socialService.ts` (add 2 methods after line 234)

- [ ] **Step 1: Add getAdminConversations and getAdminMessages methods**

Add after the existing `getConversations` method (after line 235 in `socialService.ts`):

```typescript
  async getAdminConversations(memberId: string, page: number = 1): Promise<{ items: any[]; hasMore: boolean }> {
    const response = await this._httpClient.get(`${API}/social/admin/members/${memberId}/conversations?Page=${page}`)
    const data = toCamel(response.data)
    return { items: data.items, hasMore: data.hasMore }
  }

  async getAdminMessages(conversationId: string, page: number = 1): Promise<{ items: any[]; hasMore: boolean }> {
    const response = await this._httpClient.get(`${API}/social/admin/conversations/${conversationId}/messages?Page=${page}`)
    const data = toCamel(response.data)
    return { items: data.items, hasMore: data.hasMore }
  }
```

- [ ] **Step 2: Verify TypeScript compiles**

Run: `cd src/Web/vue-app && npx vue-tsc --noEmit`
Expected: No type errors.

- [ ] **Step 3: Commit**

```bash
git add src/Web/vue-app/src/services/socialService.ts
git commit -m "feat(social): add admin conversation/message service methods"
```

---

### Task 4: Frontend — Add admin conversation route

**Files:**
- Modify: `src/Web/vue-app/src/router/index.ts` (add route after line 102)

- [ ] **Step 1: Add the admin conversation route**

In `src/Web/vue-app/src/router/index.ts`, add a new route after the existing `socialConversation` route (after line 102):

```typescript
  {
    path: '/social/messages/admin/:conversationId',
    name: 'socialAdminConversation',
    component: () => import('@/views/social/SocialConversation.vue'),
    meta: { title: 'Conversation (Admin)', requiredRole: [Role.Admin], social: true },
    props: true
  },
```

**Important:** This route MUST be placed before the catch-all `socialConversation` route (`/social/messages/:conversationId`) so that `/social/messages/admin/xxx` doesn't get matched by the `:conversationId` param. Move the new route to be ABOVE the existing `socialConversation` route entry.

The final order should be:
1. `/social/messages` → `socialMessages`
2. `/social/messages/admin/:conversationId` → `socialAdminConversation` (NEW)
3. `/social/messages/:conversationId` → `socialConversation`

- [ ] **Step 2: Verify TypeScript compiles**

Run: `cd src/Web/vue-app && npx vue-tsc --noEmit`
Expected: No type errors.

- [ ] **Step 3: Commit**

```bash
git add src/Web/vue-app/src/router/index.ts
git commit -m "feat(social): add admin conversation route"
```

---

### Task 5: Frontend — Add admin tab to SocialMessages.vue

**Files:**
- Modify: `src/Web/vue-app/src/views/social/SocialMessages.vue`

This is the largest frontend change. We add:
- A tab bar (visible only to admins) with "Mes messages" and "Admin" tabs
- Admin tab content: member search → member's conversation list
- Navigation to the admin readonly route

- [ ] **Step 1: Add imports and admin state**

In `SocialMessages.vue`, add to the imports section (after line 108):

```typescript
import { useUserStore } from '@/stores/userStore'
import { Role } from '@/types/enums'
```

After line 115 (`const avatarRegistry = ...`), add:

```typescript
const userStore = useUserStore()
const isAdmin = computed(() => userStore.hasRole(Role.Admin))
const activeTab = ref<'mine' | 'admin'>('mine')

// Admin tab state
const adminSelectedMember = ref<any>(null)
const adminMemberSearch = ref('')
const adminSearchResults = ref<any[]>([])
const adminLoadingMembers = ref(false)
let adminSearchTimeout: ReturnType<typeof setTimeout> | null = null

const adminConvoListContainer = ref<HTMLElement | null>(null)
```

- [ ] **Step 2: Add admin infinite scroll and search functions**

After the `adminConvoListContainer` ref, add:

```typescript
const {
  items: adminRawConversations,
  loading: adminLoading,
  loadingMore: adminLoadingMore,
  load: loadAdminConversations,
  attachScroll: attachAdminScroll,
} = useInfiniteScroll<any>({
  fetchFn: (page) => socialService.getAdminConversations(adminSelectedMember.value.id, page),
  scrollContainer: adminConvoListContainer,
  direction: 'down',
  threshold: 300,
})

const adminConversations = computed(() => adminRawConversations.value.filter((c: any) => c.lastMessage))

function onAdminMemberSearch() {
  if (adminSearchTimeout) clearTimeout(adminSearchTimeout)
  adminSearchTimeout = setTimeout(() => searchAdminMembers(adminMemberSearch.value), 300)
}

async function searchAdminMembers(query: string) {
  adminLoadingMembers.value = true
  try {
    adminSearchResults.value = await socialService.searchMembers(query)
  } catch { adminSearchResults.value = [] }
  adminLoadingMembers.value = false
}

async function selectAdminMember(member: any) {
  adminSelectedMember.value = member
  await loadAdminConversations()
  nextTick(() => attachAdminScroll())
}

function clearAdminMember() {
  adminSelectedMember.value = null
  adminRawConversations.value = []
}
```

Also add a `watch` for when the admin tab is first opened (load all members):

```typescript
watch(activeTab, (val) => {
  if (val === 'admin' && adminSearchResults.value.length === 0) {
    searchAdminMembers('')
  }
})
```

- [ ] **Step 3: Update the template — add tab bar**

Replace the header section (lines 3-12) with:

```html
    <!-- Tab bar (admin only) -->
    <div v-if="isAdmin" class="flex border-b border-gray-200">
      <button
        @click="activeTab = 'mine'"
        :class="['flex-1 py-3 text-center text-sm font-semibold transition', activeTab === 'mine' ? 'border-b-2 border-[#1a1a1a] text-gray-900' : 'text-gray-400 hover:text-gray-600']"
      >
        Mes messages
      </button>
      <button
        @click="activeTab = 'admin'"
        :class="['flex-1 py-3 text-center text-sm font-semibold transition', activeTab === 'admin' ? 'border-b-2 border-[#1a1a1a] text-gray-900' : 'text-gray-400 hover:text-gray-600']"
      >
        Admin
      </button>
    </div>

    <!-- Header (non-admin sees this, admin sees it only on "mine" tab) -->
    <div v-if="!isAdmin || activeTab === 'mine'" class="flex items-center justify-between border-b border-gray-200 px-4 py-3">
      <h2 class="text-lg font-bold text-gray-900">Messages</h2>
      <button
        @click="showNewConvo = !showNewConvo"
        class="rounded-lg border border-[rgba(21,128,61,0.15)] bg-[rgba(21,128,61,0.06)] px-3 py-1.5 text-xs font-semibold text-[#15803d] transition hover:bg-[rgba(21,128,61,0.12)] cursor-pointer"
      >
        {{ showNewConvo ? 'Fermer' : '+ Nouvelle conversation' }}
      </button>
    </div>
```

- [ ] **Step 4: Update the template — wrap existing content in v-if for "mine" tab**

Wrap the existing new-conversation panel (lines 14-52) and conversation list (lines 54-97) inside a container that only shows when not on the admin tab:

Add `v-if="!isAdmin || activeTab === 'mine'"` to the new-conversation panel div (the one with `v-if="showNewConvo"`). Change it to: `v-if="showNewConvo && (!isAdmin || activeTab === 'mine')"`.

For the conversation list sections (loading, empty state, list), wrap them in a `<template v-if="!isAdmin || activeTab === 'mine'">` block. Or simpler: add the condition to each of the three existing sections:
- Loading div: add `&& (!isAdmin || activeTab === 'mine')` to the `v-if="loading"` → `v-if="loading && (!isAdmin || activeTab === 'mine')"`
- Empty state div: add `&& (!isAdmin || activeTab === 'mine')` to its condition
- List div: add `&& (!isAdmin || activeTab === 'mine')` to its condition

- [ ] **Step 5: Update the template — add admin tab content**

Add after the conversation list section (after the closing `</div>` on line 97), before the template closing `</div>`:

```html
    <!-- Admin tab content -->
    <template v-if="isAdmin && activeTab === 'admin'">
      <!-- Member search -->
      <div class="border-b border-gray-200 bg-gray-50 p-4">
        <div v-if="adminSelectedMember" class="flex items-center gap-3">
          <button @click="clearAdminMember" class="text-gray-400 hover:text-gray-600 transition">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M15 19l-7-7 7-7"/></svg>
          </button>
          <div class="flex h-8 w-8 flex-shrink-0 items-center justify-center rounded-full text-xs font-bold text-white" :style="{ background: adminSelectedMember.avatarColor || getAvatarColor(adminSelectedMember.fullName) }">
            {{ getInitials(adminSelectedMember.fullName) }}
          </div>
          <span class="text-sm font-semibold text-gray-900">{{ adminSelectedMember.fullName }}</span>
        </div>
        <template v-else>
          <h3 class="mb-3 text-sm font-semibold text-gray-700">Choisir un membre</h3>
          <input
            v-model="adminMemberSearch"
            type="text"
            class="mb-3 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm placeholder-gray-400 focus:border-[#1a1a1a] focus:outline-none focus:ring-1 focus:ring-[#1a1a1a]"
            placeholder="Rechercher un membre..."
            @input="onAdminMemberSearch"
          />
          <div v-if="adminLoadingMembers" class="flex justify-center py-4">
            <div class="h-5 w-5 animate-spin rounded-full border-2 border-[#1a1a1a] border-t-transparent"></div>
          </div>
          <div v-else-if="adminSearchResults.length" class="max-h-60 space-y-1 overflow-y-auto">
            <button
              v-for="member in adminSearchResults"
              :key="member.id"
              @click="selectAdminMember(member)"
              class="flex w-full items-center gap-3 rounded-lg px-3 py-2 text-left transition hover:bg-white"
            >
              <div class="flex h-8 w-8 flex-shrink-0 items-center justify-center rounded-full text-xs font-bold text-white" :style="{ background: member.avatarColor || getAvatarColor(member.fullName) }">
                {{ getInitials(member.fullName) }}
              </div>
              <p class="text-sm font-medium text-gray-900 truncate">{{ member.fullName }}</p>
            </button>
          </div>
          <p v-else class="py-4 text-center text-xs text-gray-500">
            {{ adminMemberSearch ? 'Aucun membre trouvé.' : 'Aucun membre disponible.' }}
          </p>
        </template>
      </div>

      <!-- Admin conversation list -->
      <div v-if="adminSelectedMember && adminLoading" class="flex flex-1 items-center justify-center">
        <div class="h-6 w-6 animate-spin rounded-full border-2 border-[#1a1a1a] border-t-transparent"></div>
      </div>
      <div v-else-if="adminSelectedMember && adminConversations.length === 0" class="flex flex-col items-center justify-center gap-3 py-20 text-gray-400">
        <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15a2 2 0 01-2 2H7l-4 4V5a2 2 0 012-2h14a2 2 0 012 2z"/></svg>
        <span class="text-sm">Aucune conversation pour ce membre.</span>
      </div>
      <div v-else-if="adminSelectedMember" ref="adminConvoListContainer" class="flex-1 overflow-y-auto">
        <router-link
          v-for="conv in adminConversations"
          :key="conv.id"
          :to="{ name: 'socialAdminConversation', params: { conversationId: conv.id }, query: { names: conv.participantA.fullName + ' ↔ ' + conv.participantB.fullName } }"
          class="flex items-center gap-3 border-b px-4 py-3 transition hover:bg-gray-50"
          style="border-color: var(--soc-divider, #f0f0f0);"
        >
          <div class="min-w-0 flex-1">
            <span class="text-sm font-medium text-gray-700">
              {{ conv.participantA.fullName }} &harr; {{ conv.participantB.fullName }}
            </span>
            <p class="truncate text-xs text-gray-500">
              <span v-if="conv.lastMessage">{{ conv.lastMessage.senderName }}: {{ conv.lastMessage.content || 'Fichier' }}</span>
            </p>
          </div>
          <div class="flex flex-col items-end gap-1 flex-shrink-0">
            <span class="text-[10px] text-gray-400">{{ formatTime(conv.lastMessage?.created) }}</span>
          </div>
        </router-link>
        <div v-if="adminLoadingMore" class="flex justify-center py-3">
          <div class="h-4 w-4 animate-spin rounded-full border-2 border-[#1a1a1a] border-t-transparent"></div>
        </div>
      </div>
    </template>
```

- [ ] **Step 6: Verify it compiles**

Run: `cd src/Web/vue-app && npx vue-tsc --noEmit`
Expected: No type errors.

- [ ] **Step 7: Commit**

```bash
git add src/Web/vue-app/src/views/social/SocialMessages.vue
git commit -m "feat(social): add admin tab to messages page with member picker"
```

---

### Task 6: Frontend — Add readonly mode to SocialConversation.vue

**Files:**
- Modify: `src/Web/vue-app/src/views/social/SocialConversation.vue`

- [ ] **Step 1: Add readonly detection and conditional data loading**

In the script section, after the `route` const (line 283), add:

```typescript
const isAdminView = computed(() => route.name === 'socialAdminConversation')
```

- [ ] **Step 2: Update the header — back button and participant names**

Replace the header section (lines 12-27) with:

```html
    <!-- Header -->
    <div class="soc-convo__header">
      <button @click="$router.push(isAdminView ? { name: 'socialMessages', query: { tab: 'admin' } } : { name: 'socialMessages' })" class="soc-convo__back">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M15 19l-7-7 7-7"/></svg>
      </button>
      <template v-if="isAdminView">
        <h2 class="soc-convo__name">{{ adminParticipantNames }}</h2>
      </template>
      <template v-else>
        <router-link v-if="otherMemberId" :to="{ name: 'socialMemberProfile', params: { id: otherMemberId } }" class="soc-convo__profile-link">
          <div class="soc-convo__avatar" :style="{ background: otherMemberColor || getAvatarColor(otherMemberName) }">
            <img v-if="effectiveOtherMemberPfp" :src="effectiveOtherMemberPfp" :alt="otherMemberName" class="soc-convo__avatar-img" />
            <span v-else class="soc-convo__avatar-initials">{{ getInitials(otherMemberName) }}</span>
          </div>
          <h2 class="soc-convo__name">{{ otherMemberName }}</h2>
        </router-link>
        <template v-else>
          <span class="soc-convo__name">{{ otherMemberName }}</span>
        </template>
      </template>
    </div>
```

- [ ] **Step 3: Hide write controls in readonly mode**

In the template:

1. **Hide the drag-drop overlay** (line 9): Change `v-if="attachment.isDraggingOver.value"` to `v-if="!isAdminView && attachment.isDraggingOver.value"`

2. **Hide the delete trigger button** (lines 118-124): Wrap the existing `v-if="msg.isMine && !msg.pending"` with an admin check: `v-if="!isAdminView && msg.isMine && !msg.pending"`

3. **Disable context menu** (line 69): Change `@contextmenu.prevent="msg.isMine && !msg.pending ? openDeleteMenu(msg) : null"` to `@contextmenu.prevent="!isAdminView && msg.isMine && !msg.pending ? openDeleteMenu(msg) : null"`

4. **Hide the preview strip** (line 137): Change `v-if="attachment.previews.value.length"` to `v-if="!isAdminView && attachment.previews.value.length"`

5. **Hide the error inline** (line 168): Change `v-if="attachment.error.value"` to `v-if="!isAdminView && attachment.error.value"`

6. **Hide the input bar** (line 173): Add `v-if="!isAdminView"` to the `<div class="soc-convo__input-bar">` element.

7. **Hide the delete modal** (line 212): Change `v-if="deleteTarget"` to `v-if="!isAdminView && deleteTarget"`

8. **Disable drag events on root** (lines 4-7): Wrap the drag event handlers. Change:
   ```
   @dragenter="attachment.handleDragEnter"
   @dragleave="attachment.handleDragLeave"
   @dragover="attachment.handleDragOver"
   @drop="attachment.handleDrop"
   ```
   to:
   ```
   @dragenter="!isAdminView && attachment.handleDragEnter($event)"
   @dragleave="!isAdminView && attachment.handleDragLeave($event)"
   @dragover="!isAdminView && attachment.handleDragOver($event)"
   @drop="!isAdminView && attachment.handleDrop($event)"
   ```

- [ ] **Step 4: Add admin-specific data loading**

Add refs for admin participant names after `otherMemberColor` (after line 315):

```typescript
const adminParticipantNames = ref('')
```

Modify `loadConversationInfo()` (line 401) to handle admin view. Replace the function with:

```typescript
async function loadConversationInfo() {
  if (isAdminView.value) {
    // Admin view: participant names are passed via query param from the admin list
    adminParticipantNames.value = (route.query.names as string) || 'Conversation'
    return
  }

  const m = memberStore.member
  if (m?.id) {
    currentMemberId.value = m.id
  } else {
    try {
      const profile = await socialService.getMyProfile()
      currentMemberId.value = profile.id || profile.Id
      memberStore.setMember(profile)
    } catch { /* */ }
  }

  try {
    const convResult = await socialService.getConversations()
    const convo = convResult.items.find((c: any) => c.id === conversationId.value)
    if (convo?.otherMember) {
      otherMemberName.value = convo.otherMember.fullName || 'Conversation'
      otherMemberId.value = convo.otherMember.id || ''
      otherMemberPfp.value = convo.otherMember.profileImageUrl || ''
      otherMemberColor.value = convo.otherMember.avatarColor || ''
      avatarRegistry.setAvatar(convo.otherMember.id, convo.otherMember.profileImageUrl ?? null)
    }
  } catch { /* */ }
}
```

- [ ] **Step 5: Update message fetching for admin view**

In the `useInfiniteScroll` fetchFn (lines 330-346), update the fetch call to use the admin endpoint when in admin mode:

Replace line 331:
```typescript
    const raw = await socialService.getMessages(conversationId.value, page)
```
with:
```typescript
    const raw = isAdminView.value
      ? await socialService.getAdminMessages(conversationId.value, page)
      : await socialService.getMessages(conversationId.value, page)
```

For admin view, `isMine` should always be `false` (admin is not a participant). Update the mapping at line 338:
```typescript
        isMine: isAdminView.value ? false : m.senderMemberId === currentMemberId.value,
```

- [ ] **Step 6: Skip read-marking and polling in admin view**

In `loadMessages()` (line 426), wrap the `markAsRead` and `getUnreadCount` calls in an `!isAdminView.value` check:

Replace lines 434-438:
```typescript
    await socialService.markAsRead(conversationId.value)
    try {
      const result = await socialService.getUnreadCount()
      memberStore.setUnreadCount(result.count)
    } catch { /* */ }
```
with:
```typescript
    if (!isAdminView.value) {
      await socialService.markAsRead(conversationId.value)
      try {
        const result = await socialService.getUnreadCount()
        memberStore.setUnreadCount(result.count)
      } catch { /* */ }
    }
```

In `pollMessages()` (line 530), wrap the `markAsRead` and `getUnreadCount` in the same check:

Replace lines 539-543:
```typescript
      await socialService.markAsRead(conversationId.value)
      try {
        const r = await socialService.getUnreadCount()
        memberStore.setUnreadCount(r.count)
      } catch { /* */ }
```
with:
```typescript
      if (!isAdminView.value) {
        await socialService.markAsRead(conversationId.value)
        try {
          const r = await socialService.getUnreadCount()
          memberStore.setUnreadCount(r.count)
        } catch { /* */ }
      }
```

- [ ] **Step 7: Verify it compiles**

Run: `cd src/Web/vue-app && npx vue-tsc --noEmit`
Expected: No type errors.

- [ ] **Step 8: Commit**

```bash
git add src/Web/vue-app/src/views/social/SocialConversation.vue
git commit -m "feat(social): add readonly admin mode to conversation view"
```

---

### Task 7: Frontend — Handle admin tab restoration on back navigation

**Files:**
- Modify: `src/Web/vue-app/src/views/social/SocialMessages.vue`

When admin navigates back from a conversation, restore the admin tab.

- [ ] **Step 1: Read query param on mount to restore tab**

In `SocialMessages.vue`, add `useRoute` import and read the `tab` query param. Add after the router import (line 103):

```typescript
import { useRouter, useRoute } from 'vue-router'
```

Remove the existing `import { useRouter } from 'vue-router'` line.

In the script section, after `const router = useRouter()` (line 111), add:

```typescript
const route = useRoute()
```

Then in `onMounted`, add at the top (before `loadConversations()`):

```typescript
  if (route.query.tab === 'admin' && isAdmin.value) {
    activeTab.value = 'admin'
  }
```

- [ ] **Step 2: Verify it compiles**

Run: `cd src/Web/vue-app && npx vue-tsc --noEmit`
Expected: No type errors.

- [ ] **Step 3: Commit**

```bash
git add src/Web/vue-app/src/views/social/SocialMessages.vue
git commit -m "feat(social): restore admin tab on back navigation"
```

---

### Task 8: Manual Testing

- [ ] **Step 1: Start the dev server**

Run: `cd src/Web/vue-app && npm run dev`

- [ ] **Step 2: Test as non-admin user**

1. Log in as a regular member
2. Navigate to `/social/messages`
3. Verify: No tabs visible, page looks exactly the same as before
4. Verify: Can still send/receive messages normally

- [ ] **Step 3: Test as admin user**

1. Log in as an admin
2. Navigate to `/social/messages`
3. Verify: Two tabs visible — "Mes messages" and "Admin"
4. Verify: "Mes messages" tab shows existing conversations (default)
5. Click "Admin" tab
6. Verify: Member search appears with all members listed
7. Search for a specific member name — verify filtering works
8. Click a member — verify their conversations appear
9. Verify: Each conversation shows both participant names (A ↔ B)
10. Click a conversation — verify it navigates to `/social/messages/admin/{id}`
11. Verify: Messages display correctly with sender names
12. Verify: No input bar at the bottom
13. Verify: No delete buttons on messages
14. Verify: Right-click on messages does not show delete option
15. Verify: Cannot drag-and-drop files
16. Click back — verify return to messages page with admin tab still active
17. Click the back arrow on the member — verify return to member search

- [ ] **Step 4: Test real-time updates**

1. As admin, open a conversation in admin view
2. From another browser/account, send a message in that conversation
3. Verify: The new message appears in the admin view within ~1 second (polling)

- [ ] **Step 5: Commit any fixes found during testing**
