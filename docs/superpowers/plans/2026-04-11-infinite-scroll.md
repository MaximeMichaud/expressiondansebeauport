# Infinite Scroll Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add infinite scroll to all social views so content loads progressively on scroll instead of all at once.

**Architecture:** Backend services return a `PaginatedResult<T>` with `items` + `hasMore` flag (using the fetch-N+1 trick). Frontend uses a shared `useInfiniteScroll` composable that observes scroll position and loads the next page automatically. DM messages scroll up for history (WhatsApp style with scroll position preservation).

**Tech Stack:** C# / FastEndpoints, Vue 3 Composition API, TypeScript

---

### Task 1: Backend — Add `PaginatedResult<T>` record

**Files:**
- Create: `src/Application/Common/PaginatedResult.cs`

- [ ] **Step 1: Create the PaginatedResult record**

```csharp
namespace Application.Common;

public record PaginatedResult<T>(List<T> Items, bool HasMore);
```

- [ ] **Step 2: Commit**

```bash
git add src/Application/Common/PaginatedResult.cs
git commit -m "feat: add PaginatedResult<T> record"
```

---

### Task 2: Backend — Update PostService to return PaginatedResult

**Files:**
- Modify: `src/Application/Services/Posts/IPostService.cs`
- Modify: `src/Application/Services/Posts/PostService.cs`

- [ ] **Step 1: Update IPostService interface**

In `IPostService.cs`, change the three return types:

```csharp
// Old:
Task<List<Post>> GetGroupFeed(Guid groupId, int page);
Task<List<Post>> GetAnnouncements(int page);
Task<List<Comment>> GetComments(Guid postId, int page);

// New:
Task<PaginatedResult<Post>> GetGroupFeed(Guid groupId, int page);
Task<PaginatedResult<Post>> GetAnnouncements(int page);
Task<PaginatedResult<Comment>> GetComments(Guid postId, int page);
```

Add the import at the top:

```csharp
using Application.Common;
```

- [ ] **Step 2: Update PostService implementations**

In `PostService.cs`, update each method to fetch `pageSize + 1` and compute `hasMore`:

```csharp
public async Task<PaginatedResult<Post>> GetGroupFeed(Guid groupId, int page)
{
    const int pageSize = 20;
    var skip = (page - 1) * pageSize;
    var items = await _postRepository.GetByGroupId(groupId, skip, pageSize + 1);
    var hasMore = items.Count > pageSize;
    return new PaginatedResult<Post>(items.Take(pageSize).ToList(), hasMore);
}

public async Task<PaginatedResult<Post>> GetAnnouncements(int page)
{
    const int pageSize = 10;
    var skip = (page - 1) * pageSize;
    var items = await _postRepository.GetAnnouncements(skip, pageSize + 1);
    var hasMore = items.Count > pageSize;
    return new PaginatedResult<Post>(items.Take(pageSize).ToList(), hasMore);
}

public async Task<PaginatedResult<Comment>> GetComments(Guid postId, int page)
{
    const int pageSize = 10;
    var skip = (page - 1) * pageSize;
    var items = await _commentRepository.GetByPostId(postId, skip, pageSize + 1);
    var hasMore = items.Count > pageSize;
    return new PaginatedResult<Comment>(items.Take(pageSize).ToList(), hasMore);
}
```

Add the import:

```csharp
using Application.Common;
```

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build src/Application/`
Expected: Build fails (endpoints still expect `List<T>`) — this is fine, we'll fix them in the next tasks.

- [ ] **Step 4: Commit**

```bash
git add src/Application/Services/Posts/IPostService.cs src/Application/Services/Posts/PostService.cs
git commit -m "feat: PostService returns PaginatedResult with hasMore"
```

---

### Task 3: Backend — Update ConversationService to return PaginatedResult

**Files:**
- Modify: `src/Application/Services/Messaging/IConversationService.cs`
- Modify: `src/Application/Services/Messaging/ConversationService.cs`

- [ ] **Step 1: Update IConversationService interface**

```csharp
// Old:
Task<List<Conversation>> GetConversations(Guid memberId, int page);
Task<List<Message>> GetMessages(Guid conversationId, int page);

// New:
Task<PaginatedResult<Conversation>> GetConversations(Guid memberId, int page);
Task<PaginatedResult<Message>> GetMessages(Guid conversationId, int page);
```

Add import: `using Application.Common;`

- [ ] **Step 2: Update ConversationService implementations**

```csharp
public async Task<PaginatedResult<Conversation>> GetConversations(Guid memberId, int page)
{
    const int pageSize = 20;
    var skip = (page - 1) * pageSize;
    var items = await _conversationRepository.GetForMember(memberId, skip, pageSize + 1);
    var hasMore = items.Count > pageSize;
    return new PaginatedResult<Conversation>(items.Take(pageSize).ToList(), hasMore);
}

public async Task<PaginatedResult<Message>> GetMessages(Guid conversationId, int page)
{
    const int pageSize = 30;
    var skip = (page - 1) * pageSize;
    var items = await _messageRepository.GetByConversation(conversationId, skip, pageSize + 1);
    var hasMore = items.Count > pageSize;
    return new PaginatedResult<Message>(items.Take(pageSize).ToList(), hasMore);
}
```

Add import: `using Application.Common;`

- [ ] **Step 3: Commit**

```bash
git add src/Application/Services/Messaging/IConversationService.cs src/Application/Services/Messaging/ConversationService.cs
git commit -m "feat: ConversationService returns PaginatedResult with hasMore"
```

---

### Task 4: Backend — Update all 5 endpoints to wrap responses

**Files:**
- Modify: `src/Web/Features/Social/Posts/GetFeed/GetFeedEndpoint.cs`
- Modify: `src/Web/Features/Social/Announcements/GetAnnouncements/GetAnnouncementsEndpoint.cs`
- Modify: `src/Web/Features/Social/Posts/Comments/GetCommentsEndpoint.cs`
- Modify: `src/Web/Features/Social/Messages/GetConversations/GetConversationsEndpoint.cs`
- Modify: `src/Web/Features/Social/Messages/GetMessages/GetMessagesEndpoint.cs`

- [ ] **Step 1: Update GetFeedEndpoint.cs**

Change the end of `HandleAsync` from:

```csharp
await Send.OkAsync(result, ct);
```

to:

```csharp
await Send.OkAsync(new { Items = result, posts.HasMore }, ct);
```

Where `posts` is the variable from the service call. Rename the service call result:

```csharp
// Old:
var posts = await _postService.GetGroupFeed(req.GroupId, req.Page);
// ...mapping uses posts...

// New:
var feedResult = await _postService.GetGroupFeed(req.GroupId, req.Page);
var posts = feedResult.Items;
// ...rest of mapping stays the same...
// At the end:
await Send.OkAsync(new { Items = result, feedResult.HasMore }, ct);
```

- [ ] **Step 2: Update GetAnnouncementsEndpoint.cs**

```csharp
// Old:
var announcements = await _postService.GetAnnouncements(req.Page);

// New:
var announcementsResult = await _postService.GetAnnouncements(req.Page);
var announcements = announcementsResult.Items;

// At the end, change:
await Send.OkAsync(result, ct);
// To:
await Send.OkAsync(new { Items = result, announcementsResult.HasMore }, ct);
```

- [ ] **Step 3: Update GetCommentsEndpoint.cs**

```csharp
// Old:
var comments = await _postService.GetComments(req.PostId, req.Page);

// New:
var commentsResult = await _postService.GetComments(req.PostId, req.Page);
var comments = commentsResult.Items;

// At the end:
await Send.OkAsync(new { Items = result, commentsResult.HasMore }, ct);
```

- [ ] **Step 4: Update GetConversationsEndpoint.cs**

```csharp
// Old:
var conversations = await _conversationService.GetConversations(member.Id, req.Page);

// New:
var conversationsResult = await _conversationService.GetConversations(member.Id, req.Page);
var conversations = conversationsResult.Items;

// At the end:
await Send.OkAsync(new { Items = results, conversationsResult.HasMore }, ct);
```

Note: the mapped variable is called `results` (plural, with an s) in this endpoint.

- [ ] **Step 5: Update GetMessagesEndpoint.cs**

```csharp
// Old:
var messages = await _conversationService.GetMessages(req.ConversationId, req.Page);

// New:
var messagesResult = await _conversationService.GetMessages(req.ConversationId, req.Page);
var messages = messagesResult.Items;

// At the end:
await Send.OkAsync(new { Items = results, messagesResult.HasMore }, ct);
```

Note: the mapped variable is called `results` (plural) in this endpoint too.

- [ ] **Step 6: Verify full backend build**

Run: `dotnet build`
Expected: PASS — all projects compile successfully.

- [ ] **Step 7: Commit**

```bash
git add src/Web/Features/Social/
git commit -m "feat: wrap all social list endpoints with { items, hasMore }"
```

---

### Task 5: Frontend — Update socialService.ts return types

**Files:**
- Modify: `src/Web/vue-app/src/services/socialService.ts`

- [ ] **Step 1: Update the 5 paginated methods to return `{ items, hasMore }`**

Each method currently returns the raw `toCamel(response.data)` which was a flat array. Now the API returns `{ items: [...], hasMore: bool }`. Update each method:

```typescript
// getGroupFeed — line 72
async getGroupFeed(groupId: string, page: number = 1): Promise<{ items: Post[]; hasMore: boolean }> {
  const response = await this._httpClient.get(`${API}/social/groups/${groupId}/posts?Page=${page}`)
  const data = toCamel(response.data)
  return { items: data.items, hasMore: data.hasMore }
}

// getAnnouncements — line 207
async getAnnouncements(page: number = 1): Promise<{ items: Post[]; hasMore: boolean }> {
  const response = await this._httpClient.get(`${API}/social/announcements?Page=${page}`)
  const data = toCamel(response.data)
  return { items: data.items, hasMore: data.hasMore }
}

// getComments — line 127
async getComments(postId: string, page: number = 1): Promise<{ items: Comment[]; hasMore: boolean }> {
  const response = await this._httpClient.get(`${API}/social/posts/${postId}/comments?Page=${page}`)
  const data = toCamel(response.data)
  return { items: data.items, hasMore: data.hasMore }
}

// getConversations — line 213
async getConversations(page: number = 1): Promise<{ items: Conversation[]; hasMore: boolean }> {
  const response = await this._httpClient.get(`${API}/social/conversations?Page=${page}`)
  const data = toCamel(response.data)
  return { items: data.items, hasMore: data.hasMore }
}

// getMessages — line 218
async getMessages(conversationId: string, page: number = 1): Promise<{ items: Message[]; hasMore: boolean }> {
  const response = await this._httpClient.get(`${API}/social/conversations/${conversationId}/messages?Page=${page}`)
  const data = toCamel(response.data)
  return { items: data.items, hasMore: data.hasMore }
}
```

- [ ] **Step 2: Commit**

```bash
git add src/Web/vue-app/src/services/socialService.ts
git commit -m "feat: socialService returns { items, hasMore } for paginated endpoints"
```

---

### Task 6: Frontend — Create useInfiniteScroll composable

**Files:**
- Create: `src/Web/vue-app/src/composables/useInfiniteScroll.ts`

- [ ] **Step 1: Write the composable**

```typescript
import { ref, onMounted, onUnmounted, type Ref, nextTick } from 'vue'

interface UseInfiniteScrollOptions<T> {
  fetchFn: (page: number) => Promise<{ items: T[]; hasMore: boolean }>
  scrollContainer: Ref<HTMLElement | null>
  direction?: 'down' | 'up'
  threshold?: number
}

export function useInfiniteScroll<T>(options: UseInfiniteScrollOptions<T>) {
  const {
    fetchFn,
    scrollContainer,
    direction = 'down',
    threshold = 200,
  } = options

  const items = ref<T[]>([]) as Ref<T[]>
  const loading = ref(false)
  const loadingMore = ref(false)
  const hasMore = ref(true)
  const page = ref(1)

  let scrollHandler: (() => void) | null = null

  async function load() {
    loading.value = true
    page.value = 1
    hasMore.value = true
    try {
      const result = await fetchFn(1)
      items.value = result.items
      hasMore.value = result.hasMore
    } catch { /* */ }
    loading.value = false
  }

  async function loadMore() {
    if (loadingMore.value || !hasMore.value) return
    loadingMore.value = true
    const nextPage = page.value + 1
    try {
      const result = await fetchFn(nextPage)
      if (direction === 'up') {
        // Save scroll position before prepending
        const el = scrollContainer.value
        const prevScrollHeight = el?.scrollHeight ?? 0

        items.value = [...result.items, ...items.value]

        // Restore scroll position after DOM update
        await nextTick()
        if (el) {
          const newScrollHeight = el.scrollHeight
          el.scrollTop = newScrollHeight - prevScrollHeight
        }
      } else {
        items.value = [...items.value, ...result.items]
      }
      hasMore.value = result.hasMore
      page.value = nextPage
    } catch { /* */ }
    loadingMore.value = false
  }

  function onScroll() {
    const el = scrollContainer.value
    if (!el || loadingMore.value || !hasMore.value) return

    if (direction === 'down') {
      const distanceFromBottom = el.scrollHeight - el.scrollTop - el.clientHeight
      if (distanceFromBottom < threshold) {
        loadMore()
      }
    } else {
      // direction === 'up': trigger when near the top
      if (el.scrollTop < threshold) {
        loadMore()
      }
    }
  }

  function attachScroll() {
    const el = scrollContainer.value
    if (!el || scrollHandler) return
    scrollHandler = onScroll
    el.addEventListener('scroll', scrollHandler, { passive: true })
  }

  function detachScroll() {
    const el = scrollContainer.value
    if (el && scrollHandler) {
      el.removeEventListener('scroll', scrollHandler)
      scrollHandler = null
    }
  }

  // Refresh page 1 for polling — updates existing items in place and prepends new ones
  async function refreshFirst() {
    try {
      const result = await fetchFn(1)
      if (items.value.length === 0) {
        items.value = result.items
        hasMore.value = result.hasMore
        return result.items
      }

      // Find new items not already in the list (by id)
      const existingIds = new Set((items.value as any[]).map((i: any) => i.id))
      const newItems = result.items.filter((i: any) => !existingIds.has(i.id))

      if (direction === 'up') {
        // For messages: new items go at the end (most recent)
        items.value = [...items.value, ...newItems]
      } else {
        // For feeds: new items go at the beginning
        items.value = [...newItems, ...items.value]
      }

      // Update existing items in place (likes, read status, etc.)
      const freshMap = new Map(result.items.map((i: any) => [i.id, i]))
      items.value = items.value.map((item: any) => {
        const fresh = freshMap.get(item.id)
        return fresh ? { ...item, ...fresh } : item
      }) as T[]

      return result.items
    } catch {
      return []
    }
  }

  async function reset() {
    detachScroll()
    await load()
    await nextTick()
    attachScroll()
  }

  onMounted(() => {
    // Defer attachment to let the container render
    nextTick(() => attachScroll())
  })

  onUnmounted(() => {
    detachScroll()
  })

  return {
    items,
    loading,
    loadingMore,
    hasMore,
    page,
    load,
    loadMore,
    refreshFirst,
    reset,
    attachScroll,
    detachScroll,
  }
}
```

- [ ] **Step 2: Commit**

```bash
git add src/Web/vue-app/src/composables/useInfiniteScroll.ts
git commit -m "feat: add useInfiniteScroll composable"
```

---

### Task 7: Frontend — Integrate infinite scroll into SocialGroup.vue (feed)

**Files:**
- Modify: `src/Web/vue-app/src/views/social/SocialGroup.vue`

- [ ] **Step 1: Add import and setup the composable**

Add the import near the other composable imports (line ~346):

```typescript
import { useInfiniteScroll } from '@/composables/useInfiniteScroll'
```

Add a template ref for the scroll container. In the `<template>`, change the outer wrapper at line 2:

```html
<div ref="feedContainer" class="flex min-h-[calc(100vh-120px)] flex-col overflow-y-auto">
```

In the `<script setup>`, add the ref and composable setup. Replace the existing `posts`, `loadingPosts` refs and the `loadPosts`/`refreshPostsSilent` functions:

```typescript
const feedContainer = ref<HTMLElement | null>(null)

const {
  items: posts,
  loading: loadingPosts,
  loadingMore: loadingMorePosts,
  hasMore: hasMorePosts,
  load: loadPosts,
  refreshFirst: refreshPostsFirst,
  reset: resetPosts,
  attachScroll: attachFeedScroll,
} = useInfiniteScroll<Post>({
  fetchFn: (page) => socialService.getGroupFeed(groupId.value, page),
  scrollContainer: feedContainer,
  direction: 'down',
  threshold: 300,
})
```

Remove the old `posts` ref (line 365), `loadingPosts` ref (line 366), `loadPosts` function (lines 490-497), and `refreshPostsSilent` function (lines 499-504).

- [ ] **Step 2: Add loading-more spinner in template**

After the posts loop (after the closing `</div>` of `v-else` at line ~284), before the feed section closing `</div>`, add:

```html
<!-- Load more spinner -->
<div v-if="loadingMorePosts" class="flex justify-center py-4">
  <div class="h-5 w-5 animate-spin rounded-full border-2 border-[#1a1a1a] border-t-transparent"></div>
</div>
```

- [ ] **Step 3: Update polling to use refreshFirst**

Replace the polling interval in `onMounted` (lines 600-614):

```typescript
pollInterval = setInterval(async () => {
  if (expandedComments.value || submittingComment.value) return
  try {
    const freshPage1 = await refreshPostsFirst()
    avatarRegistry.populateFromList(freshPage1 as any[], 'authorMemberId', 'authorProfileImageUrl')
  } catch { /* */ }
}, 2000)
```

- [ ] **Step 4: Update onMounted to use the composable's load**

Replace `onMounted` (line 597-615):

```typescript
onMounted(async () => {
  await loadGroup()
  await loadPosts()
  avatarRegistry.populateFromList(posts.value as any[], 'authorMemberId', 'authorProfileImageUrl')
  nextTick(() => attachFeedScroll())
  pollInterval = setInterval(async () => {
    if (expandedComments.value || submittingComment.value) return
    try {
      const freshPage1 = await refreshPostsFirst()
      avatarRegistry.populateFromList(freshPage1 as any[], 'authorMemberId', 'authorProfileImageUrl')
    } catch { /* */ }
  }, 2000)
})
```

- [ ] **Step 5: Update submitPost and confirmDelete to use resetPosts**

In `submitPost` (line ~541), change `await loadPosts()` to `await resetPosts()`.
In `confirmDelete` (line ~436), change `await loadPosts()` to `await resetPosts()`.
In `CreatePollModal` @created handler (line ~309), change `loadPosts` to `resetPosts`.

- [ ] **Step 6: Update getComments calls**

The `toggleComments` function (line 557) and `submitComment` (line 577) call `socialService.getComments(post.id)` which now returns `{ items, hasMore }`. Update both:

```typescript
// In toggleComments:
const result = await socialService.getComments(post.id)
postComments.value = result.items

// In submitComment:
const result = await socialService.getComments(post.id)
postComments.value = result.items
```

- [ ] **Step 7: Verify no TypeScript errors**

Run: `cd src/Web/vue-app && npx vue-tsc --noEmit`

- [ ] **Step 8: Commit**

```bash
git add src/Web/vue-app/src/views/social/SocialGroup.vue
git commit -m "feat: infinite scroll on group feed"
```

---

### Task 8: Frontend — Integrate infinite scroll into SocialImportant.vue (announcements)

**Files:**
- Modify: `src/Web/vue-app/src/views/social/SocialImportant.vue`

- [ ] **Step 1: Add import and set up composable**

Add import:

```typescript
import { useInfiniteScroll } from '@/composables/useInfiniteScroll'
```

Add a ref to the template root. Change line 2 `<div class="p-4">` to:

```html
<div ref="announcementsContainer" class="p-4 overflow-y-auto max-h-[calc(100vh-120px)]">
```

Replace the `announcements` ref (line 239), `loading` ref (line 240), and `loadAnnouncements` function (lines 331-337):

```typescript
const announcementsContainer = ref<HTMLElement | null>(null)

const {
  items: announcements,
  loading,
  loadingMore,
  hasMore: hasMoreAnnouncements,
  load: loadAnnouncements,
  refreshFirst: refreshAnnouncementsFirst,
  reset: resetAnnouncements,
  attachScroll: attachAnnouncementsScroll,
} = useInfiniteScroll<Post>({
  fetchFn: (page) => socialService.getAnnouncements(page),
  scrollContainer: announcementsContainer,
  direction: 'down',
  threshold: 300,
})
```

- [ ] **Step 2: Add loading-more spinner**

After the `</div>` closing the `v-else class="space-y-3"` list (line ~193), add:

```html
<div v-if="loadingMore" class="flex justify-center py-4">
  <div class="h-5 w-5 animate-spin rounded-full border-2 border-[#1a1a1a] border-t-transparent"></div>
</div>
```

- [ ] **Step 3: Update polling**

Replace the polling interval (lines 397-400):

```typescript
pollInterval = setInterval(async () => {
  if (Date.now() - likeDebounce < 3000) return
  try { await refreshAnnouncementsFirst() } catch { /* */ }
}, 30000)
```

- [ ] **Step 4: Update onMounted**

```typescript
onMounted(() => {
  loadAnnouncements()
  nextTick(() => attachAnnouncementsScroll())
  pollInterval = setInterval(async () => {
    if (Date.now() - likeDebounce < 3000) return
    try { await refreshAnnouncementsFirst() } catch { /* */ }
  }, 30000)
})
```

- [ ] **Step 5: Update createAnnouncement and confirmDelete**

Change `await loadAnnouncements()` to `await resetAnnouncements()` in both `createAnnouncement` (line ~369) and `confirmDelete` (line ~386).

- [ ] **Step 6: Update getComments calls**

In `toggleComments` (line 300) and `submitComment` (line 314), update:

```typescript
// toggleComments:
const result = await socialService.getComments(post.id)
postComments.value = result.items

// submitComment:
const result = await socialService.getComments(post.id)
postComments.value = result.items
```

- [ ] **Step 7: Commit**

```bash
git add src/Web/vue-app/src/views/social/SocialImportant.vue
git commit -m "feat: infinite scroll on announcements list"
```

---

### Task 9: Frontend — Integrate infinite scroll into SocialConversation.vue (messages, scroll up)

**Files:**
- Modify: `src/Web/vue-app/src/views/social/SocialConversation.vue`

- [ ] **Step 1: Add import**

```typescript
import { useInfiniteScroll } from '@/composables/useInfiniteScroll'
```

- [ ] **Step 2: Set up the composable**

The `messagesContainer` ref already exists (line 329). Add the composable after the refs block:

```typescript
const {
  items: scrollMessages,
  loading: loadingScroll,
  loadingMore: loadingMoreMessages,
  hasMore: hasMoreMessages,
  load: loadScrollMessages,
  loadMore: loadOlderMessages,
  refreshFirst: refreshMessagesFirst,
  reset: resetScrollMessages,
  attachScroll: attachMessagesScroll,
} = useInfiniteScroll<ChatMessage>({
  fetchFn: async (page) => {
    const raw = await socialService.getMessages(conversationId.value, page)
    return {
      items: raw.items.map((m: any) => ({
        id: m.id,
        content: m.content,
        senderMemberId: m.senderMemberId,
        created: m.created,
        isMine: m.senderMemberId === currentMemberId.value,
        isRead: m.isRead ?? false,
        isDeleted: m.isDeleted ?? false,
        media: Array.isArray(m.media) ? m.media : undefined,
        messageType: m.messageType,
        joinRequest: m.joinRequest,
      })),
      hasMore: raw.hasMore,
    }
  },
  scrollContainer: messagesContainer,
  direction: 'up',
  threshold: 200,
})
```

- [ ] **Step 3: Update allMessages computed**

Replace the `allMessages` computed (lines 332-336). The composable stores messages with page 1 first (most recent), and older pages prepended. Since the API returns newest-first and we display oldest-first, we need to reverse the composable items:

```typescript
const allMessages = computed(() => {
  const sorted = [...scrollMessages.value].reverse()
  return [...sorted, ...pendingMessages.value]
})
```

- [ ] **Step 4: Update loadMessages function**

Replace `loadMessages` (lines 392-420):

```typescript
async function loadMessages(smooth = false) {
  const isFirstLoad = scrollMessages.value.length === 0
  if (isFirstLoad) loading.value = true
  try {
    await loadScrollMessages()
    pendingMessages.value = pendingMessages.value.filter(
      pm => !scrollMessages.value.some(sm => sm.content === pm.content)
    )
    await socialService.markAsRead(conversationId.value)
    try {
      const count = await socialService.getUnreadCount()
      memberStore.setUnreadCount(count)
    } catch { /* */ }
  } catch { /* */ }
  loading.value = false
  scrollToBottom(smooth)
}
```

- [ ] **Step 5: Update pollMessages function**

Replace `pollMessages` (lines 508-537):

```typescript
async function pollMessages() {
  const prevCount = scrollMessages.value.length
  try {
    const freshPage1 = await refreshMessagesFirst()
    pendingMessages.value = pendingMessages.value.filter(
      pm => !scrollMessages.value.some(sm => sm.content === pm.content)
    )
    if (scrollMessages.value.length > prevCount) {
      scrollToBottom(true)
      await socialService.markAsRead(conversationId.value)
      try {
        const count = await socialService.getUnreadCount()
        memberStore.setUnreadCount(count)
      } catch { /* */ }
    }
  } catch { /* */ }
}
```

- [ ] **Step 6: Remove old serverMessages ref**

Remove `const serverMessages = ref<ChatMessage[]>([])` (line 283). All references to `serverMessages` are now replaced by `scrollMessages` from the composable.

- [ ] **Step 7: Add loading-more spinner in template**

Inside the `soc-convo__messages` div (line 30), right after the `<div v-if="loading">` block and before `<template v-else>`, add:

```html
<div v-if="loadingMoreMessages" class="flex justify-center py-3">
  <div class="h-4 w-4 animate-spin rounded-full border-2 border-[#1a1a1a] border-t-transparent"></div>
</div>
```

- [ ] **Step 8: Update onMounted to attach scroll**

```typescript
onMounted(async () => {
  await loadConversationInfo()
  await loadMessages()
  nextTick(() => {
    messageInput.value?.focus()
    attachMessagesScroll()
  })
  pollInterval = setInterval(pollMessages, 1000)
})
```

- [ ] **Step 9: Update loadConversationInfo**

The `loadConversationInfo` function (line 380) calls `socialService.getConversations()` which now returns `{ items, hasMore }`. Update:

```typescript
const conversationsResult = await socialService.getConversations()
const convo = conversationsResult.items.find((c: any) => c.id === conversationId.value)
```

- [ ] **Step 10: Commit**

```bash
git add src/Web/vue-app/src/views/social/SocialConversation.vue
git commit -m "feat: infinite scroll on DM messages (scroll up for history)"
```

---

### Task 10: Frontend — Integrate infinite scroll into SocialMessages.vue (conversations list)

**Files:**
- Modify: `src/Web/vue-app/src/views/social/SocialMessages.vue`

- [ ] **Step 1: Add import and set up composable**

```typescript
import { useInfiniteScroll } from '@/composables/useInfiniteScroll'
```

Change the conversation list container at line 62 to add a ref:

```html
<div v-else-if="!showNewConvo" ref="convoListContainer" class="flex-1 overflow-y-auto">
```

Replace the `conversations` ref (line 112), `loading` ref (line 113), and `loadConversations` function (lines 229-236):

```typescript
const convoListContainer = ref<HTMLElement | null>(null)

const {
  items: rawConversations,
  loading,
  loadingMore: loadingMoreConvos,
  hasMore: hasMoreConvos,
  load: loadRawConversations,
  refreshFirst: refreshConvosFirst,
  attachScroll: attachConvosScroll,
} = useInfiniteScroll<Conversation>({
  fetchFn: (page) => socialService.getConversations(page),
  scrollContainer: convoListContainer,
  direction: 'down',
  threshold: 300,
})

const conversations = computed(() => rawConversations.value.filter((c: any) => c.lastMessage))
```

Add `computed` to the vue import if not already there.

- [ ] **Step 2: Add loading-more spinner**

After the `</router-link>` loop (line ~93), before the closing `</div>` of the list container, add:

```html
<div v-if="loadingMoreConvos" class="flex justify-center py-3">
  <div class="h-4 w-4 animate-spin rounded-full border-2 border-[#1a1a1a] border-t-transparent"></div>
</div>
```

- [ ] **Step 3: Create a loadConversations wrapper**

```typescript
async function loadConversations() {
  await loadRawConversations()
  populateRegistryFromConvos(conversations.value)
}
```

- [ ] **Step 4: Update polling**

Replace the polling interval (lines 249-255):

```typescript
pollInterval = setInterval(async () => {
  try {
    await refreshConvosFirst()
    populateRegistryFromConvos(conversations.value)
  } catch { /* */ }
}, 3000)
```

- [ ] **Step 5: Update onMounted**

```typescript
onMounted(() => {
  loadConversations()
  onMessage(onNewMessage)
  nextTick(() => attachConvosScroll())
  pollInterval = setInterval(async () => {
    try {
      await refreshConvosFirst()
      populateRegistryFromConvos(conversations.value)
    } catch { /* */ }
  }, 3000)
})
```

- [ ] **Step 6: Commit**

```bash
git add src/Web/vue-app/src/views/social/SocialMessages.vue
git commit -m "feat: infinite scroll on conversations list"
```

---

### Task 11: Frontend — Update SocialAnnouncement.vue (comments on detail view)

**Files:**
- Modify: `src/Web/vue-app/src/views/social/SocialAnnouncement.vue`

- [ ] **Step 1: Update getComments calls**

The `loadComments` function and polling both call `socialService.getComments()` which now returns `{ items, hasMore }`. Update:

```typescript
// In loadComments:
const result = await socialService.getComments(postId.value)
comments.value = result.items

// In the polling interval (inside onMounted):
const result = await socialService.getComments(postId.value)
comments.value = result.items
```

- [ ] **Step 2: Commit**

```bash
git add src/Web/vue-app/src/views/social/SocialAnnouncement.vue
git commit -m "feat: update SocialAnnouncement to handle new comments response format"
```

---

### Task 12: End-to-end verification

- [ ] **Step 1: Build and run backend**

```bash
dotnet build
```

Expected: PASS

- [ ] **Step 2: Type-check frontend**

```bash
cd src/Web/vue-app && npx vue-tsc --noEmit
```

Expected: PASS (or only pre-existing errors)

- [ ] **Step 3: Start dev server and test in browser**

```bash
cd src/Web/vue-app && npm run dev
```

Test each view:
1. **Group feed** — scroll down, verify new posts load automatically
2. **Announcements** — scroll down, verify more announcements appear
3. **Conversations list** — scroll down if many conversations
4. **DM messages** — scroll UP in a long conversation, verify old messages appear above without jumping scroll position
5. **Comments** — expand comments on a post with many comments, verify they load

- [ ] **Step 4: Final commit if any fixes needed**
