# Infinite Scroll — Design Spec

## Objectif

Remplacer le chargement "page 1 seulement" par un infinite scroll sur toutes les vues sociales. Le contenu se charge progressivement au scroll, comme Instagram/Facebook.

## Vues concernées

| Vue | Direction | Page size | Trigger |
|-----|-----------|-----------|---------|
| SocialGroup.vue (feed) | Scroll down | 20 | Bas de la liste |
| SocialImportant.vue (annonces) | Scroll down | 10 | Bas de la liste |
| SocialAnnouncement.vue (commentaires) | Scroll down | 10 | Bas de la liste |
| SocialGroup.vue (commentaires inline) | Scroll down | 10 | Bas de la section |
| SocialConversation.vue (messages) | Scroll up | 30 | Haut du chat |
| SocialMessages.vue (conversations) | Scroll down | 20 | Bas de la liste |

## Backend — Réponse paginée avec `hasMore`

### Changement de format

Toutes les réponses de liste passent de `T[]` à :

```json
{
  "items": [ ... ],
  "hasMore": true
}
```

### Technique `hasMore` sans count query

Dans les services (`PostService`, `ConversationService`), on fetch `pageSize + 1` items. Si on en reçoit plus que `pageSize`, `hasMore = true` et on retourne seulement les `pageSize` premiers.

### Fichiers backend à modifier

**Services :**
- `IPostService.cs` — changer les return types de `Task<List<T>>` à un nouveau record `PaginatedResult<T>`
- `PostService.cs` — modifier `GetGroupFeed`, `GetAnnouncements`, `GetComments` pour fetch `pageSize + 1` et retourner `PaginatedResult<T>`
- `IConversationService.cs` — idem
- `ConversationService.cs` — modifier `GetConversations`, `GetMessages`

**Nouveau type :**
```csharp
// dans Application/Common/ ou similaire
public record PaginatedResult<T>(List<T> Items, bool HasMore);
```

**Endpoints (5 fichiers) — wrapper la réponse :**
- `GetFeedEndpoint.cs` → `new { Items = result, HasMore = ... }`
- `GetAnnouncementsEndpoint.cs` → idem
- `GetCommentsEndpoint.cs` → idem
- `GetConversationsEndpoint.cs` → idem
- `GetMessagesEndpoint.cs` → idem

### Page sizes (inchangées)

| Contenu | Page size |
|---------|-----------|
| Posts (feed) | 20 |
| Annonces | 10 |
| Commentaires | 10 |
| Messages | 30 |
| Conversations | 20 |

## Frontend — Composable `useInfiniteScroll`

### API du composable

```typescript
interface UseInfiniteScrollOptions<T> {
  fetchFn: (page: number) => Promise<{ items: T[]; hasMore: boolean }>
  scrollContainer: Ref<HTMLElement | null>
  direction?: 'down' | 'up'       // défaut: 'down'
  threshold?: number               // pixels avant le bord pour trigger, défaut: 200
  initialPage?: number             // défaut: 1
}

interface UseInfiniteScrollReturn<T> {
  items: Ref<T[]>
  loading: Ref<boolean>
  loadingMore: Ref<boolean>
  hasMore: Ref<boolean>
  page: Ref<number>
  load: () => Promise<void>        // charge la page courante (reset)
  loadMore: () => Promise<void>    // charge la page suivante
  prepend: (newItems: T[]) => void // ajouter en haut (pour polling)
  reset: () => Promise<void>       // reset à page 1 et recharge
}
```

### Comportement scroll down (feed, annonces, conversations list, commentaires)

1. Au mount, charge page 1
2. Observe le scroll du container
3. Quand l'utilisateur est à `threshold` px du bas et `hasMore && !loadingMore` → charge page suivante
4. Les nouveaux items s'ajoutent à la fin du tableau `items`
5. Un spinner s'affiche en bas pendant le chargement

### Comportement scroll up (messages DM)

1. Au mount, charge page 1 (messages les plus récents)
2. Le container scroll automatiquement en bas (messages récents visibles)
3. Quand l'utilisateur scroll vers le haut à `threshold` px du top et `hasMore && !loadingMore` → charge page suivante
4. Les vieux messages s'ajoutent au **début** du tableau `items`
5. **Scroll position preservation** : avant d'ajouter les vieux messages, sauvegarder `scrollHeight`. Après l'ajout, ajuster `scrollTop = newScrollHeight - oldScrollHeight` pour que l'utilisateur reste à la même position visuelle (comportement WhatsApp)
6. Un spinner s'affiche en haut pendant le chargement

### Fichier

`src/Web/vue-app/src/composables/useInfiniteScroll.ts`

## Frontend — Modifications des vues

### socialService.ts

Toutes les méthodes paginées retournent maintenant `Promise<{ items: T[]; hasMore: boolean }>` au lieu de `Promise<T[]>`.

Méthodes concernées :
- `getGroupFeed(groupId, page)` 
- `getAnnouncements(page)`
- `getComments(postId, page)`
- `getConversations(page)`
- `getMessages(conversationId, page)`

### SocialGroup.vue (feed de groupe)

- Utiliser `useInfiniteScroll` avec `direction: 'down'`
- Le container de scroll = le conteneur de la liste de posts
- Spinner en bas de la liste quand `loadingMore`
- Le polling (2s) refresh seulement la page 1 et met à jour les items existants via `prepend` pour les nouveaux posts

### SocialImportant.vue (annonces)

- Utiliser `useInfiniteScroll` avec `direction: 'down'`
- Le polling (30s) refresh seulement la page 1

### SocialAnnouncement.vue (commentaires d'une annonce)

- Utiliser `useInfiniteScroll` avec `direction: 'down'` pour les commentaires
- Le post unique reste chargé normalement (pas paginé)

### SocialGroup.vue (commentaires inline)

- Quand on expand les commentaires d'un post, charger page 1
- Bouton "Voir plus de commentaires" ou mini infinite scroll dans la section commentaires

### SocialConversation.vue (messages DM)

- Utiliser `useInfiniteScroll` avec `direction: 'up'`
- Scroll position preservation (WhatsApp style)
- Le polling (1s) refresh seulement la page 1 et ajoute les nouveaux messages en bas
- Quand l'utilisateur envoie un message → scroll auto en bas

### SocialMessages.vue (liste de conversations)

- Utiliser `useInfiniteScroll` avec `direction: 'down'`
- Le polling refresh la page 1

## Polling avec infinite scroll

### Stratégie : refresh page 1 seulement

Le polling existant continue mais ne refresh que les items les plus récents (page 1) :

1. Fetch page 1
2. Comparer avec les items actuels en haut de la liste
3. Si nouveaux items → les ajouter en haut (prepend)
4. Si items existants ont changé (likes, commentaires count) → mettre à jour en place
5. Ne PAS toucher aux pages déjà chargées au-delà de la page 1

### Intervalles (inchangés)

- Messages DM : 1s
- Feed de groupe : 2s
- Annonces : 30s
- Conversations : 3s

## UX — Spinners et états

### Loading initial

Le spinner existant de chargement de page reste pour le premier chargement.

### Loading more (infinite scroll)

Un petit spinner centré apparaît :
- En bas de la liste pour scroll down
- En haut du chat pour scroll up (messages)

### Fin de liste

Quand `hasMore = false`, le spinner disparaît. Pas de message "fin de liste" — la liste s'arrête simplement.

### État vide

Inchangé — les messages existants "Aucun post", "Aucune conversation", etc. restent.
