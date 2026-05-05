# PWA + Notifications Push — Design

**Date :** 2026-05-02
**Status :** Approuvé
**Branche :** feat/social-updates (ou nouvelle branche dédiée)

## Objectif

Permettre aux membres du portail social de recevoir des notifications push sur leur téléphone (et autres appareils) pour 3 types d'événements :
1. Nouveau message privé (DM)
2. Nouvelle publication dans un groupe dont je suis membre
3. Nouvelle annonce

L'app doit être installable sur l'écran d'accueil (PWA). Les notifs push ne sont disponibles qu'une fois l'app installée (limitation iOS, traité de façon uniforme aussi pour Android).

## Décisions clés

- **Web Push standard auto-hébergé** avec VAPID — lib `Lib.Net.Http.WebPush` côté .NET. Pas de service tiers (OneSignal, Firebase). Cohérent avec l'infra existante (Docker sur VPS, zéro dépendance externe).
- **Préférences par user** (pas par appareil). Un toggle s'applique à tous les appareils du user.
- **Préférences par groupe pour les posts** : un master switch + opt-out par groupe. Default = ON quand l'user rejoint un nouveau groupe.
- **Push seulement si l'app est en arrière-plan** : le service worker checke `clients.matchAll()` et n'affiche pas la notif si une fenêtre du portail est focused/visible. Évite la double notif (toast in-app + push système) quand l'user est déjà dans l'app.
- **Notifs avec preview du contenu** : titre + extrait du message/post/annonce. Le portail est privé entre membres, et c'est le comportement attendu par les users.
- **Pas de bannière d'install** : l'utilisateur va dans Mon compte > Notifications. Si l'app n'est pas installée, on affiche les instructions d'installation. Si installée, on affiche les switches.
- **Pas de prompt automatique** : la permission browser est demandée uniquement quand l'user toggle un switch ON pour la 1re fois.

## Architecture

### Frontend (Vue 3 + Vite PWA)

**Plugin** : `vite-plugin-pwa` en mode `injectManifest` (custom service worker).

**Manifest** (`vite.config.ts` config) :
- `name` : "Expression Danse Beauport"
- `short_name` : "EDB"
- `theme_color` : `#be1e2c`
- `background_color` : `#ffffff`
- `display` : `standalone`
- `start_url` : `/social`
- `icons` : 192×192 et 512×512 PNG (à générer depuis le logo existant)

**Service worker** (`src/sw.ts`) :
- Listener `push` : décode le payload JSON, skip si une fenêtre est visible/focused, sinon affiche la notif via `self.registration.showNotification()`
- Listener `notificationclick` : ferme la notif et ouvre l'URL ciblée via `clients.openWindow()`
- Listener `pushsubscriptionchange` : ré-enregistre la subscription auprès du backend

**Composable** `usePushSubscription.ts` :
- `isPwaInstalled` : computed sur `display-mode: standalone` + `navigator.standalone`
- `permission` : ref sur `Notification.permission`
- `subscribe()` : demande permission, crée subscription via `pushManager.subscribe()`, POST au backend
- `unsubscribe()` : `subscription.unsubscribe()` + DELETE au backend
- `syncPreferences(prefs)` : PUT au backend

**Service** `pushService.ts` (couche REST) :
- `getVapidPublicKey()`
- `createSubscription(sub)`
- `deleteSubscription(endpoint)`
- `getPreferences()`
- `updatePreferences(prefs)`
- `updateGroupPreference(groupId, enabled)`

**Composant** `NotificationsCard.vue` (dans `src/components/social/account/`) :
- Section ajoutée à `SocialAccount.vue` (même pattern que les cards existantes : Avatar, Infos perso, Mot de passe)
- Si `!isPwaInstalled` : affiche les instructions d'installation (variantes iOS / Android / desktop selon UA detection)
- Si `isPwaInstalled` :
  - 3 switches principaux : Messages privés, Publications dans mes groupes (master), Annonces
  - Sous le master "Publications" : liste des groupes du user, chacun avec son propre switch (défault ON)
  - Si `permission !== 'granted'` : bouton "Activer sur cet appareil" qui déclenche le subscribe flow
  - Si permission révoquée par le browser : message d'erreur + lien "Comment réactiver"

### Backend (.NET 10 + FastEndpoints)

**Lib NuGet** : `Lib.Net.Http.WebPush` (mature, RFC 8030 conforme)

**Configuration** :
- `Vapid:PublicKey` (commitable)
- `Vapid:PrivateKey` (env var en prod, secrets.json en dev)
- `Vapid:Subject` : `mailto:contact@expressiondansebeauport.com`

Génération des clés une fois via script CLI ou code one-shot (`VapidHelper.GenerateVapidKeys()`).

**Tables** (migration EF Core) :

```
PushSubscriptions
  Id              uuid PK
  UserId          uuid FK Users
  Endpoint        varchar(500) UNIQUE
  P256dh          varchar(200)
  Auth            varchar(100)
  CreatedAt       timestamptz
  LastUsedAt      timestamptz
  -- Index sur UserId

UserNotificationPreferences
  UserId                    uuid PK FK Users
  NotifyOnDirectMessage     bool default true
  NotifyOnAnnouncement      bool default true
  NotifyOnGroupPost         bool default true
  -- Row créée lazy au premier GET /preferences si absente

UserGroupNotificationPreferences
  UserId    uuid FK Users
  GroupId   uuid FK Groups
  Enabled   bool
  PRIMARY KEY (UserId, GroupId)
  -- "no row" = défaut ON. Row insérée seulement si l'user mute ou réactive explicitement.
```

**Service** `IPushNotificationDispatcher` (singleton, scoped DbContext via factory) :

```csharp
Task SendToUserAsync(Guid userId, PushNotificationType type, PushPayload payload, CancellationToken ct);
Task SendToManyAsync(IEnumerable<Guid> userIds, PushNotificationType type, PushPayload payload, CancellationToken ct);
```

Logique :
1. Charger `UserNotificationPreferences` du user
2. Selon `type`, vérifier le bon flag (et pour `GroupPost`, vérifier aussi `UserGroupNotificationPreferences` pour ce groupe)
3. Charger toutes les `PushSubscriptions` du user
4. Fan-out parallèle (Task.WhenAll) avec timeout
5. Si `WebPushClient` retourne 410/404 → soft-delete la sub
6. Autres erreurs → log warning, ne pas crasher

`PushPayload` = `{ title, body, url, tag }`

**Endpoints** (FastEndpoints, sous `/social/push/`, JWT requis, role MEMBER+) :
- `GET /social/push/vapid-public-key` → `{ publicKey }`
- `POST /social/push/subscriptions` body `{ endpoint, p256dh, auth }` → upsert sur `Endpoint` unique
- `DELETE /social/push/subscriptions` body `{ endpoint }` → soft-delete
- `GET /social/push/preferences` → `{ dm, posts, announcements, mutedGroups: [] }`
- `PUT /social/push/preferences` body `{ dm, posts, announcements }` → upsert dans `UserNotificationPreferences`
- `PUT /social/push/preferences/groups/{groupId}` body `{ enabled }` → upsert dans `UserGroupNotificationPreferences`

### Intégration aux événements existants

**`SendMessageEndpoint`** (`Features/Social/Messages/Send/`) :
Après le `_hubContext.Clients.Client(...).SendAsync("ReceiveMessage", ...)`, ajouter :
```csharp
await _dispatcher.SendToUserAsync(recipientUserId, PushNotificationType.DirectMessage, new PushPayload {
    Title = member.FullName,
    Body = TruncatePreview(req.Content, 120),
    Url = $"/social/messages/{req.ConversationId}",
    Tag = $"dm-{req.ConversationId}"
}, ct);
```

**`CreatePostEndpoint`** (`Features/Social/Posts/Create/`) :
Après création du post, query `_memberRepository.GetByGroupId(groupId)` (excluant l'auteur), puis :
```csharp
var userIds = members.Select(m => m.UserId);
await _dispatcher.SendToManyAsync(userIds, PushNotificationType.GroupPost, new PushPayload {
    Title = $"{groupName} • {authorName}",
    Body = TruncatePreview(StripHtml(post.Content), 120),
    Url = $"/social/posts/{post.Id}",
    Tag = $"post-{post.Id}"
}, ct);
```

**`CreateAnnouncementEndpoint`** (`Features/Social/Announcements/CreateAnnouncement/`) :
Query tous les members actifs (excluant l'auteur) :
```csharp
await _dispatcher.SendToManyAsync(userIds, PushNotificationType.Announcement, new PushPayload {
    Title = "📢 Nouvelle annonce",
    Body = TruncatePreview(StripHtml(announcement.Content), 120),
    Url = $"/social/annonces",
    Tag = $"announcement-{announcement.Id}"
}, ct);
```

## Flow utilisateur (golden path)

1. Alice ouvre `social.expressiondansebeauport.com` dans Safari iOS
2. Va dans **Mon compte > Notifications** → voit les instructions "Sur iPhone : Partager → Sur l'écran d'accueil"
3. Installe l'app sur l'écran d'accueil
4. Lance l'app depuis l'icône → s'ouvre en standalone
5. Retourne dans **Mon compte > Notifications** → voit maintenant les 3 switches + bouton "Activer sur cet appareil"
6. Clique "Activer" → prompt iOS "Voulez-vous autoriser les notifications ?" → Autoriser
7. Toggle ses 3 switches comme elle veut
8. Bob lui envoie un DM
9. Téléphone d'Alice (écran verrouillé ou app pas ouverte) → notif "Bob — Hey, t'as vu le nouveau cours ?"
10. Tap sur la notif → app s'ouvre directement sur la conversation

## Edge cases & error handling

- **Subscription expirée (410/404 du push service)** : soft-delete automatique dans le dispatcher
- **Permission révoquée par l'user après abonnement** : detect au load de la card, désactive switches, affiche un message
- **User pas dans Pwa mais essaie d'activer** : impossible (les switches sont cachés)
- **Service worker pas encore enregistré** : `usePushSubscription.subscribe()` await `navigator.serviceWorker.ready` avant
- **Push avec app focused** : service worker skip silencieusement (gardé pour le toast in-app via SignalR existant)
- **Multiple appareils** : fan-out à toutes les `PushSubscriptions` du user, indépendant
- **Erreur d'envoi non-410** : log warning, n'interrompt pas le endpoint qui a déclenché le push
- **`pushsubscriptionchange`** : le SW ré-enregistre automatiquement la nouvelle sub auprès du backend (rare mais arrive)

## Testing

- **Backend** : tests unitaires sur `PushNotificationDispatcher` (mock `WebPushClient`, vérifier filtrage des prefs, soft-delete sur 410)
- **Backend** : tests d'intégration sur les endpoints (subscription, preferences)
- **Frontend** : test manuel sur Chrome desktop, Safari iOS (PWA installée), Chrome Android
- **Pas de tests E2E sur les notifs elles-mêmes** : trop dépendant du service de push externe

## Hors scope (v2 potentielle)

- Notifs sur commentaires, likes, mentions, demandes d'adhésion, sondages
- Préférences par appareil (revenir à ça si demandé)
- Quiet hours / batching
- Email fallback si push pas disponible
- Action buttons dans la notif (Répondre, Marquer lu)
- Badge count sur l'icône PWA

## Fichiers touchés (estimation)

**Nouveaux** :
- `src/Web/vue-app/src/sw.ts`
- `src/Web/vue-app/src/composables/usePushSubscription.ts`
- `src/Web/vue-app/src/services/pushService.ts`
- `src/Web/vue-app/src/components/social/account/NotificationsCard.vue`
- `src/Web/vue-app/public/icons/192.png`, `512.png`, `badge.png`
- `src/Application/Services/Push/PushNotificationDispatcher.cs`
- `src/Application/Services/Push/IPushNotificationDispatcher.cs`
- `src/Application/Services/Push/PushPayload.cs`
- `src/Application/Services/Push/PushNotificationType.cs`
- `src/Domain/Entities/PushSubscription.cs`
- `src/Domain/Entities/UserNotificationPreferences.cs`
- `src/Domain/Entities/UserGroupNotificationPreferences.cs`
- `src/Infrastructure/Repositories/PushSubscriptionRepository.cs`
- `src/Infrastructure/Repositories/NotificationPreferencesRepository.cs`
- `src/Infrastructure/Migrations/<timestamp>_AddPushNotifications.cs`
- `src/Web/Features/Social/Push/*` (5 endpoints)

**Modifiés** :
- `src/Web/vue-app/vite.config.ts` (vite-plugin-pwa)
- `src/Web/vue-app/package.json` (+ `vite-plugin-pwa`, `workbox-window`)
- `src/Web/vue-app/index.html` (link manifest)
- `src/Web/vue-app/src/main.ts` (registerSW)
- `src/Web/vue-app/src/views/social/SocialAccount.vue` (ajouter NotificationsCard)
- `src/Web/vue-app/src/locales/fr.json` (textes)
- `src/Web/Features/Social/Messages/Send/SendMessageEndpoint.cs`
- `src/Web/Features/Social/Posts/Create/CreatePostEndpoint.cs`
- `src/Web/Features/Social/Announcements/CreateAnnouncement/CreateAnnouncementEndpoint.cs`
- `src/Web/appsettings.json` (Vapid:PublicKey)
- `src/Web/Program.cs` (DI : `WebPushClient`, `IPushNotificationDispatcher`)
- `src/Infrastructure/Persistence/GarneauTemplateDbContext.cs` (DbSets)
