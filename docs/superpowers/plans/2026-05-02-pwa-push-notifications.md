# PWA + Notifications Push — Plan d'implémentation

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal :** Permettre aux membres du portail social de recevoir des push notifications sur leurs appareils (DM, posts dans groupes, annonces) via une PWA installable sur l'écran d'accueil.

**Architecture :** PWA via `vite-plugin-pwa` (manifest + service worker custom). Backend Web Push standard (VAPID) avec lib `Lib.Net.Http.WebPush`. Préférences par user, opt-out par groupe pour les posts. Settings dans Mon compte > Notifications. Push skippé si l'app est focused (évite double notif avec le toast SignalR existant).

**Tech Stack :** Vue 3, vite-plugin-pwa, .NET 10, FastEndpoints, EF Core, PostgreSQL, Lib.Net.Http.WebPush, xUnit + Moq.

**Spec :** `docs/superpowers/specs/2026-05-02-pwa-push-notifications-design.md`

---

## Convention pour les commits

Branche : `feat/social-updates` (existante, OK pour ce travail).
Format : `feat(push): ...`, `feat(pwa): ...`, `test(push): ...`, `chore(push): ...`.

---

## Task 1 : Setup PWA (manifest + service worker minimal)

**Files :**
- Modify : `src/Web/vue-app/package.json`
- Modify : `src/Web/vue-app/vite.config.ts`
- Create : `src/Web/vue-app/src/sw.ts`
- Modify : `src/Web/vue-app/src/main.ts`
- Modify : `src/Web/vue-app/index.html`
- Create : `src/Web/vue-app/public/icons/192.png`
- Create : `src/Web/vue-app/public/icons/512.png`
- Create : `src/Web/vue-app/public/icons/badge.png`

- [ ] **Step 1 : Installer les dépendances**

```bash
cd src/Web/vue-app && npm install --save-dev vite-plugin-pwa workbox-window
```

Vérifier dans `package.json` que les deux apparaissent dans `devDependencies`.

- [ ] **Step 2 : Générer les icônes PWA**

Source : `src/Web/vue-app/public/edb_logo.svg` (ou n'importe quel logo carré existant ; sinon utiliser le favicon).

Si le logo source existe en SVG :
```bash
cd src/Web/vue-app/public/icons
# 192x192
rsvg-convert -w 192 -h 192 ../edb_logo.svg -o 192.png
# 512x512
rsvg-convert -w 512 -h 512 ../edb_logo.svg -o 512.png
# Badge monochrome 96x96 (silhouette pour Android)
rsvg-convert -w 96 -h 96 ../edb_logo.svg -o badge.png
```

Si `rsvg-convert` n'est pas installé, alternative ImageMagick :
```bash
magick ../edb_logo.svg -resize 192x192 192.png
magick ../edb_logo.svg -resize 512x512 512.png
magick ../edb_logo.svg -resize 96x96 -threshold 50% badge.png
```

Si aucun outil de conversion n'est disponible et qu'aucun PNG carré n'existe, demander à l'user de fournir 3 fichiers PNG (192, 512, 96) et les déposer dans `public/icons/`. **Ne pas inventer / ne pas committer de placeholders.**

- [ ] **Step 3 : Configurer `vite-plugin-pwa` dans `vite.config.ts`**

Ouvrir `src/Web/vue-app/vite.config.ts` et ajouter l'import + le plugin dans le tableau `plugins` :

```ts
import { VitePWA } from 'vite-plugin-pwa'

// ... dans plugins: [
VitePWA({
  registerType: 'autoUpdate',
  strategies: 'injectManifest',
  srcDir: 'src',
  filename: 'sw.ts',
  injectManifest: {
    globPatterns: ['**/*.{js,css,html,svg,png,woff2}']
  },
  manifest: {
    name: 'Expression Danse Beauport',
    short_name: 'EDB',
    description: 'Portail communautaire d\'Expression Danse Beauport',
    theme_color: '#be1e2c',
    background_color: '#ffffff',
    display: 'standalone',
    orientation: 'portrait',
    start_url: '/social',
    scope: '/',
    icons: [
      { src: '/icons/192.png', sizes: '192x192', type: 'image/png' },
      { src: '/icons/512.png', sizes: '512x512', type: 'image/png' },
      { src: '/icons/512.png', sizes: '512x512', type: 'image/png', purpose: 'maskable' }
    ]
  },
  devOptions: {
    enabled: false
  }
})
// ]
```

- [ ] **Step 4 : Créer `src/Web/vue-app/src/sw.ts` (service worker minimal)**

```ts
/// <reference lib="webworker" />
import { precacheAndRoute } from 'workbox-precaching'

declare const self: ServiceWorkerGlobalScope

precacheAndRoute(self.__WB_MANIFEST)

self.addEventListener('install', () => {
  self.skipWaiting()
})

self.addEventListener('activate', (event) => {
  event.waitUntil(self.clients.claim())
})
```

- [ ] **Step 5 : Enregistrer le service worker dans `main.ts`**

À la fin de `src/Web/vue-app/src/main.ts`, ajouter :

```ts
import { registerSW } from 'virtual:pwa-register'
if ('serviceWorker' in navigator) {
  registerSW({ immediate: true })
}
```

- [ ] **Step 6 : Lien manifest dans `index.html`**

Dans `<head>` de `src/Web/vue-app/index.html`, juste avant `</head>` (le plugin l'injecte automatiquement, mais on ajoute aussi les meta iOS) :

```html
<meta name="apple-mobile-web-app-capable" content="yes">
<meta name="apple-mobile-web-app-status-bar-style" content="default">
<meta name="apple-mobile-web-app-title" content="EDB">
<link rel="apple-touch-icon" href="/icons/192.png">
```

- [ ] **Step 7 : Build et vérifier**

```bash
cd src/Web/vue-app && npm run build
```

Attendu : build OK, `dist/manifest.webmanifest` et `dist/sw.js` existent.

- [ ] **Step 8 : Test manuel install**

Lancer en dev (`make dev` à la racine) puis dans Chrome desktop, ouvrir le site, ouvrir les DevTools → Application → Manifest. Vérifier : nom, icônes affichées, "Installable" sans warning. Cliquer l'icône d'install dans la barre d'adresse → l'app s'ouvre en standalone.

- [ ] **Step 9 : Commit**

```bash
git add src/Web/vue-app/package.json src/Web/vue-app/package-lock.json src/Web/vue-app/vite.config.ts src/Web/vue-app/src/sw.ts src/Web/vue-app/src/main.ts src/Web/vue-app/index.html src/Web/vue-app/public/icons/
git commit -m "feat(pwa): manifest + service worker, app installable sur écran d'accueil"
```

---

## Task 2 : Backend — NuGet WebPush + génération des clés VAPID

**Files :**
- Modify : `src/Application/Application.csproj` (ajouter package)
- Modify : `src/Web/appsettings.json`
- Modify : `src/Web/appsettings.Development.json`

- [ ] **Step 1 : Installer le package**

```bash
cd src/Application && dotnet add package Lib.Net.Http.WebPush
```

Vérifier dans `Application.csproj` : `<PackageReference Include="Lib.Net.Http.WebPush" Version="..." />` apparaît.

- [ ] **Step 2 : Générer les clés VAPID via dotnet script one-shot**

Créer un fichier temporaire `tools/generate-vapid.csx` ou exécuter en interactif :

```bash
cd src/Application
dotnet run --project /dev/stdin << 'EOF' 2>/dev/null || cat > /tmp/gen-vapid.cs << 'EOF2'
using Lib.Net.Http.WebPush.Authentication;
var keys = VapidHelper.GenerateVapidKeys();
Console.WriteLine($"Public:  {keys.PublicKey}");
Console.WriteLine($"Private: {keys.PrivateKey}");
EOF2
```

Plus simple — depuis n'importe où :

```bash
cd /tmp && mkdir -p vapidgen && cd vapidgen
dotnet new console -n VapidGen
cd VapidGen
dotnet add package Lib.Net.Http.WebPush
cat > Program.cs << 'EOF'
using Lib.Net.Http.WebPush.Authentication;
var keys = VapidHelper.GenerateVapidKeys();
Console.WriteLine($"VAPID_PUBLIC_KEY={keys.PublicKey}");
Console.WriteLine($"VAPID_PRIVATE_KEY={keys.PrivateKey}");
EOF
dotnet run
```

Attendu : 2 lignes avec les clés (~88 chars chacune en base64url).

**Stocker les valeurs dans un endroit sûr** — la publique sera commitée, la privée jamais.

- [ ] **Step 3 : Ajouter la section Vapid dans `appsettings.json`**

Ajouter à la racine de `src/Web/appsettings.json` :

```json
"Vapid": {
  "Subject": "mailto:contact@expressiondansebeauport.com",
  "PublicKey": "<PUBLIC_KEY générée à l'étape 2>",
  "PrivateKey": ""
}
```

(La clé privée vide = lue depuis env var en prod / user-secrets en dev.)

- [ ] **Step 4 : Configurer la clé privée en dev via user-secrets**

```bash
cd src/Web
dotnet user-secrets init
dotnet user-secrets set "Vapid:PrivateKey" "<PRIVATE_KEY générée à l'étape 2>"
```

- [ ] **Step 5 : Documenter la clé prod dans le README ou env de prod**

Ajouter dans `docker-compose.prod.yml` la variable d'env :

```yaml
services:
  web:
    environment:
      - Vapid__PrivateKey=${VAPID_PRIVATE_KEY}
```

Et noter dans `README.md` (section Déploiement) qu'il faut ajouter `VAPID_PRIVATE_KEY=...` au `.env` de prod.

- [ ] **Step 6 : Commit**

```bash
git add src/Application/Application.csproj src/Web/appsettings.json docker-compose.prod.yml README.md
git commit -m "chore(push): add Lib.Net.Http.WebPush + VAPID config"
```

---

## Task 3 : Domain — entités Push

**Files :**
- Create : `src/Domain/Entities/PushSubscription.cs`
- Create : `src/Domain/Entities/UserNotificationPreferences.cs`
- Create : `src/Domain/Entities/UserGroupNotificationPreferences.cs`

- [ ] **Step 1 : Créer `PushSubscription.cs`**

```csharp
using Domain.Common;

namespace Domain.Entities;

public class PushSubscription : AuditableEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public string Endpoint { get; private set; } = null!;
    public string P256dh { get; private set; } = null!;
    public string Auth { get; private set; } = null!;
    public NodaTime.Instant LastUsedAt { get; private set; }

    private PushSubscription() { }

    public PushSubscription(Guid userId, string endpoint, string p256dh, string auth)
    {
        UserId = userId;
        Endpoint = endpoint;
        P256dh = p256dh;
        Auth = auth;
        LastUsedAt = Domain.Helpers.InstantHelper.GetLocalNow();
    }

    public void TouchLastUsed() => LastUsedAt = Domain.Helpers.InstantHelper.GetLocalNow();
    public void UpdateKeys(string p256dh, string auth)
    {
        P256dh = p256dh;
        Auth = auth;
    }
}
```

- [ ] **Step 2 : Créer `UserNotificationPreferences.cs`**

```csharp
using Domain.Common;

namespace Domain.Entities;

public class UserNotificationPreferences : AuditableEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public bool NotifyOnDirectMessage { get; private set; } = true;
    public bool NotifyOnAnnouncement { get; private set; } = true;
    public bool NotifyOnGroupPost { get; private set; } = true;

    private UserNotificationPreferences() { }

    public UserNotificationPreferences(Guid userId)
    {
        UserId = userId;
    }

    public void UpdatePreferences(bool dm, bool announcement, bool groupPost)
    {
        NotifyOnDirectMessage = dm;
        NotifyOnAnnouncement = announcement;
        NotifyOnGroupPost = groupPost;
    }
}
```

- [ ] **Step 3 : Créer `UserGroupNotificationPreferences.cs`**

```csharp
using Domain.Common;

namespace Domain.Entities;

public class UserGroupNotificationPreferences : AuditableEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public Guid GroupId { get; private set; }
    public Group Group { get; private set; } = null!;
    public bool Enabled { get; private set; }

    private UserGroupNotificationPreferences() { }

    public UserGroupNotificationPreferences(Guid userId, Guid groupId, bool enabled)
    {
        UserId = userId;
        GroupId = groupId;
        Enabled = enabled;
    }

    public void SetEnabled(bool enabled) => Enabled = enabled;
}
```

- [ ] **Step 4 : Build pour vérifier**

```bash
dotnet build src/Domain/Domain.csproj
```

Attendu : Build succeeded, 0 errors.

- [ ] **Step 5 : Commit**

```bash
git add src/Domain/Entities/PushSubscription.cs src/Domain/Entities/UserNotificationPreferences.cs src/Domain/Entities/UserGroupNotificationPreferences.cs
git commit -m "feat(push): add domain entities for push subscriptions and prefs"
```

---

## Task 4 : Persistence — EF Configurations + DbSets + Migration

**Files :**
- Create : `src/Persistence/Configurations/PushSubscriptionConfiguration.cs`
- Create : `src/Persistence/Configurations/UserNotificationPreferencesConfiguration.cs`
- Create : `src/Persistence/Configurations/UserGroupNotificationPreferencesConfiguration.cs`
- Modify : `src/Persistence/GarneauTemplateDbContext.cs`
- Create : `src/Persistence/Migrations/<timestamp>_AddPushNotifications.cs` (généré)

- [ ] **Step 1 : Lire un example de config existante**

```bash
cat src/Persistence/Configurations/JoinRequestConfiguration.cs
```

(pour copier le style.)

- [ ] **Step 2 : Créer `PushSubscriptionConfiguration.cs`**

```csharp
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class PushSubscriptionConfiguration : IEntityTypeConfiguration<PushSubscription>
{
    public void Configure(EntityTypeBuilder<PushSubscription> builder)
    {
        builder.ToTable("PushSubscriptions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Endpoint).IsRequired().HasMaxLength(500);
        builder.Property(x => x.P256dh).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Auth).IsRequired().HasMaxLength(100);

        builder.HasIndex(x => x.Endpoint).IsUnique();
        builder.HasIndex(x => x.UserId);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

- [ ] **Step 3 : Créer `UserNotificationPreferencesConfiguration.cs`**

```csharp
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class UserNotificationPreferencesConfiguration : IEntityTypeConfiguration<UserNotificationPreferences>
{
    public void Configure(EntityTypeBuilder<UserNotificationPreferences> builder)
    {
        builder.ToTable("UserNotificationPreferences");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.UserId).IsUnique();

        builder.Property(x => x.NotifyOnDirectMessage).HasDefaultValue(true);
        builder.Property(x => x.NotifyOnAnnouncement).HasDefaultValue(true);
        builder.Property(x => x.NotifyOnGroupPost).HasDefaultValue(true);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

- [ ] **Step 4 : Créer `UserGroupNotificationPreferencesConfiguration.cs`**

```csharp
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class UserGroupNotificationPreferencesConfiguration : IEntityTypeConfiguration<UserGroupNotificationPreferences>
{
    public void Configure(EntityTypeBuilder<UserGroupNotificationPreferences> builder)
    {
        builder.ToTable("UserGroupNotificationPreferences");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.UserId, x.GroupId }).IsUnique();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Group)
            .WithMany()
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

- [ ] **Step 5 : Ajouter les `DbSet` dans `GarneauTemplateDbContext.cs`**

Repérer les autres `DbSet<...>` dans le fichier et ajouter à côté :

```csharp
public DbSet<PushSubscription> PushSubscriptions => Set<PushSubscription>();
public DbSet<UserNotificationPreferences> UserNotificationPreferences => Set<UserNotificationPreferences>();
public DbSet<UserGroupNotificationPreferences> UserGroupNotificationPreferences => Set<UserGroupNotificationPreferences>();
```

- [ ] **Step 6 : Générer la migration**

```bash
cd src/Web
dotnet ef migrations add AddPushNotifications --project ../Persistence
```

Attendu : 2 fichiers créés dans `src/Persistence/Migrations/<timestamp>_AddPushNotifications.cs` + `.Designer.cs`. Snapshot mis à jour.

- [ ] **Step 7 : Vérifier la migration générée**

Ouvrir le `.cs` (pas le Designer) et confirmer :
- 3 `CreateTable` : `PushSubscriptions`, `UserNotificationPreferences`, `UserGroupNotificationPreferences`
- Index unique sur `Endpoint`, sur `UserId` (prefs), sur `(UserId, GroupId)`
- Default values sur les bool

S'il y a quelque chose d'anormal (ex : table renommée), corriger en ajustant le code et regénérer.

- [ ] **Step 8 : Appliquer la migration localement**

```bash
docker compose up db -d --wait
cd src/Web && dotnet ef database update --project ../Persistence
```

Attendu : "Done." Vérifier dans la DB que les 3 tables existent :

```bash
docker compose exec db psql -U postgres -d garneau -c '\dt' | grep -i push
```

- [ ] **Step 9 : Commit**

```bash
git add src/Persistence/Configurations/PushSubscriptionConfiguration.cs src/Persistence/Configurations/UserNotificationPreferencesConfiguration.cs src/Persistence/Configurations/UserGroupNotificationPreferencesConfiguration.cs src/Persistence/GarneauTemplateDbContext.cs src/Persistence/Migrations/
git commit -m "feat(push): EF configurations + migration for push tables"
```

---

## Task 5 : Repositories — interfaces

**Files :**
- Create : `src/Domain/Repositories/IPushSubscriptionRepository.cs`
- Create : `src/Domain/Repositories/INotificationPreferencesRepository.cs`

- [ ] **Step 1 : Créer `IPushSubscriptionRepository.cs`**

```csharp
using Domain.Entities;

namespace Domain.Repositories;

public interface IPushSubscriptionRepository
{
    Task<PushSubscription?> FindByEndpoint(string endpoint);
    Task<List<PushSubscription>> GetByUserId(Guid userId);
    Task Add(PushSubscription subscription);
    Task Update(PushSubscription subscription);
    Task DeleteByEndpoint(string endpoint);
    Task DeleteById(Guid id);
}
```

- [ ] **Step 2 : Créer `INotificationPreferencesRepository.cs`**

```csharp
using Domain.Entities;

namespace Domain.Repositories;

public interface INotificationPreferencesRepository
{
    Task<UserNotificationPreferences?> FindByUserId(Guid userId);
    Task<UserNotificationPreferences> GetOrCreate(Guid userId);
    Task UpdatePreferences(Guid userId, bool dm, bool announcement, bool groupPost);

    Task<List<UserGroupNotificationPreferences>> GetGroupOverridesForUser(Guid userId);
    Task<UserGroupNotificationPreferences?> FindGroupOverride(Guid userId, Guid groupId);
    Task SetGroupOverride(Guid userId, Guid groupId, bool enabled);
}
```

- [ ] **Step 3 : Build**

```bash
dotnet build src/Domain/Domain.csproj
```

Attendu : 0 errors.

- [ ] **Step 4 : Commit**

```bash
git add src/Domain/Repositories/IPushSubscriptionRepository.cs src/Domain/Repositories/INotificationPreferencesRepository.cs
git commit -m "feat(push): repository interfaces"
```

---

## Task 6 : Repositories — implémentations

**Files :**
- Create : `src/Infrastructure/Repositories/Notifications/PushSubscriptionRepository.cs`
- Create : `src/Infrastructure/Repositories/Notifications/NotificationPreferencesRepository.cs`

- [ ] **Step 1 : Créer `PushSubscriptionRepository.cs`**

```csharp
using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.Notifications;

public class PushSubscriptionRepository : IPushSubscriptionRepository
{
    private readonly GarneauTemplateDbContext _context;

    public PushSubscriptionRepository(GarneauTemplateDbContext context) => _context = context;

    public Task<PushSubscription?> FindByEndpoint(string endpoint)
        => _context.PushSubscriptions.FirstOrDefaultAsync(s => s.Endpoint == endpoint);

    public Task<List<PushSubscription>> GetByUserId(Guid userId)
        => _context.PushSubscriptions.AsNoTracking().Where(s => s.UserId == userId).ToListAsync();

    public async Task Add(PushSubscription subscription)
    {
        _context.PushSubscriptions.Add(subscription);
        await _context.SaveChangesAsync();
    }

    public async Task Update(PushSubscription subscription)
    {
        _context.PushSubscriptions.Update(subscription);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteByEndpoint(string endpoint)
    {
        var sub = await _context.PushSubscriptions.FirstOrDefaultAsync(s => s.Endpoint == endpoint);
        if (sub != null)
        {
            _context.PushSubscriptions.Remove(sub);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteById(Guid id)
    {
        var sub = await _context.PushSubscriptions.FindAsync(id);
        if (sub != null)
        {
            _context.PushSubscriptions.Remove(sub);
            await _context.SaveChangesAsync();
        }
    }
}
```

- [ ] **Step 2 : Créer `NotificationPreferencesRepository.cs`**

```csharp
using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.Notifications;

public class NotificationPreferencesRepository : INotificationPreferencesRepository
{
    private readonly GarneauTemplateDbContext _context;

    public NotificationPreferencesRepository(GarneauTemplateDbContext context) => _context = context;

    public Task<UserNotificationPreferences?> FindByUserId(Guid userId)
        => _context.UserNotificationPreferences.FirstOrDefaultAsync(p => p.UserId == userId);

    public async Task<UserNotificationPreferences> GetOrCreate(Guid userId)
    {
        var existing = await _context.UserNotificationPreferences.FirstOrDefaultAsync(p => p.UserId == userId);
        if (existing != null) return existing;

        var fresh = new UserNotificationPreferences(userId);
        _context.UserNotificationPreferences.Add(fresh);
        await _context.SaveChangesAsync();
        return fresh;
    }

    public async Task UpdatePreferences(Guid userId, bool dm, bool announcement, bool groupPost)
    {
        var prefs = await GetOrCreate(userId);
        prefs.UpdatePreferences(dm, announcement, groupPost);
        await _context.SaveChangesAsync();
    }

    public Task<List<UserGroupNotificationPreferences>> GetGroupOverridesForUser(Guid userId)
        => _context.UserGroupNotificationPreferences.AsNoTracking().Where(g => g.UserId == userId).ToListAsync();

    public Task<UserGroupNotificationPreferences?> FindGroupOverride(Guid userId, Guid groupId)
        => _context.UserGroupNotificationPreferences.FirstOrDefaultAsync(g => g.UserId == userId && g.GroupId == groupId);

    public async Task SetGroupOverride(Guid userId, Guid groupId, bool enabled)
    {
        var existing = await _context.UserGroupNotificationPreferences
            .FirstOrDefaultAsync(g => g.UserId == userId && g.GroupId == groupId);

        if (existing == null)
        {
            _context.UserGroupNotificationPreferences.Add(new UserGroupNotificationPreferences(userId, groupId, enabled));
        }
        else
        {
            existing.SetEnabled(enabled);
        }
        await _context.SaveChangesAsync();
    }
}
```

- [ ] **Step 3 : Enregistrer les repositories dans `Infrastructure/ConfigureServices.cs`**

Ouvrir `src/Infrastructure/ConfigureServices.cs`, repérer les autres `services.AddScoped<I...Repository, ...Repository>()` et ajouter :

```csharp
services.AddScoped<IPushSubscriptionRepository, PushSubscriptionRepository>();
services.AddScoped<INotificationPreferencesRepository, NotificationPreferencesRepository>();
```

(Imports nécessaires : `using Domain.Repositories;` et `using Infrastructure.Repositories.Notifications;`)

- [ ] **Step 4 : Build**

```bash
dotnet build src/Infrastructure/Infrastructure.csproj
```

Attendu : 0 errors.

- [ ] **Step 5 : Commit**

```bash
git add src/Infrastructure/Repositories/Notifications/ src/Infrastructure/ConfigureServices.cs
git commit -m "feat(push): repository implementations + DI registration"
```

---

## Task 7 : Push Dispatcher — interface et types

**Files :**
- Create : `src/Application/Services/Push/IPushNotificationDispatcher.cs`
- Create : `src/Application/Services/Push/PushPayload.cs`
- Create : `src/Application/Services/Push/PushNotificationType.cs`

- [ ] **Step 1 : Créer `PushNotificationType.cs`**

```csharp
namespace Application.Services.Push;

public enum PushNotificationType
{
    DirectMessage,
    GroupPost,
    Announcement
}
```

- [ ] **Step 2 : Créer `PushPayload.cs`**

```csharp
namespace Application.Services.Push;

public class PushPayload
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Url { get; set; } = "/social";
    public string? Tag { get; set; }
    public Guid? GroupId { get; set; }  // utilisé seulement pour GroupPost (filtrage opt-out par groupe)
}
```

- [ ] **Step 3 : Créer `IPushNotificationDispatcher.cs`**

```csharp
namespace Application.Services.Push;

public interface IPushNotificationDispatcher
{
    Task SendToUserAsync(Guid userId, PushNotificationType type, PushPayload payload, CancellationToken ct = default);
    Task SendToManyAsync(IEnumerable<Guid> userIds, PushNotificationType type, PushPayload payload, CancellationToken ct = default);
}
```

- [ ] **Step 4 : Build**

```bash
dotnet build src/Application/Application.csproj
```

Attendu : 0 errors.

- [ ] **Step 5 : Commit**

```bash
git add src/Application/Services/Push/
git commit -m "feat(push): dispatcher interface + payload types"
```

---

## Task 8 : Push Dispatcher — tests TDD

**Files :**
- Create : `tests/Tests.Application/Services/Push/PushNotificationDispatcherTests.cs`

- [ ] **Step 1 : Créer le fichier de test avec un test qui échoue**

```csharp
using Application.Services.Push;
using Domain.Entities;
using Domain.Repositories;
using Lib.Net.Http.WebPush;
using Microsoft.Extensions.Logging;
using Tests.Application.TestCollections;

namespace Tests.Application.Services.Push;

[Collection(ApplicationTestCollection.NAME)]
public class PushNotificationDispatcherTests
{
    private readonly Mock<IPushSubscriptionRepository> _subRepo;
    private readonly Mock<INotificationPreferencesRepository> _prefRepo;
    private readonly Mock<IPushSenderClient> _client;
    private readonly Mock<ILogger<PushNotificationDispatcher>> _logger;
    private readonly PushNotificationDispatcher _dispatcher;

    private readonly Guid _userId = Guid.NewGuid();

    public PushNotificationDispatcherTests()
    {
        _subRepo = new Mock<IPushSubscriptionRepository>();
        _prefRepo = new Mock<INotificationPreferencesRepository>();
        _client = new Mock<IPushSenderClient>();
        _logger = new Mock<ILogger<PushNotificationDispatcher>>();

        _dispatcher = new PushNotificationDispatcher(_subRepo.Object, _prefRepo.Object, _client.Object, _logger.Object);
    }

    [Fact]
    public async Task GivenUserHasNoSubscriptions_WhenSendToUser_ThenDoesNothing()
    {
        // Arrange
        _prefRepo.Setup(r => r.FindByUserId(_userId)).ReturnsAsync(new UserNotificationPreferences(_userId));
        _subRepo.Setup(r => r.GetByUserId(_userId)).ReturnsAsync(new List<PushSubscription>());

        // Act
        await _dispatcher.SendToUserAsync(_userId, PushNotificationType.DirectMessage, new PushPayload { Title = "T", Body = "B" });

        // Assert
        _client.Verify(c => c.SendAsync(It.IsAny<PushSubscription>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GivenUserHasDmDisabled_WhenSendDmToUser_ThenDoesNotSend()
    {
        // Arrange
        var prefs = new UserNotificationPreferences(_userId);
        prefs.UpdatePreferences(dm: false, announcement: true, groupPost: true);
        _prefRepo.Setup(r => r.FindByUserId(_userId)).ReturnsAsync(prefs);
        _subRepo.Setup(r => r.GetByUserId(_userId)).ReturnsAsync(new List<PushSubscription>
        {
            new PushSubscription(_userId, "https://push.example/1", "p1", "a1")
        });

        // Act
        await _dispatcher.SendToUserAsync(_userId, PushNotificationType.DirectMessage, new PushPayload { Title = "T", Body = "B" });

        // Assert
        _client.Verify(c => c.SendAsync(It.IsAny<PushSubscription>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GivenUserHasDmEnabled_WhenSendDmToUser_ThenSendsToAllSubscriptions()
    {
        // Arrange
        var prefs = new UserNotificationPreferences(_userId);
        var sub1 = new PushSubscription(_userId, "https://push.example/1", "p1", "a1");
        var sub2 = new PushSubscription(_userId, "https://push.example/2", "p2", "a2");
        _prefRepo.Setup(r => r.FindByUserId(_userId)).ReturnsAsync(prefs);
        _subRepo.Setup(r => r.GetByUserId(_userId)).ReturnsAsync(new List<PushSubscription> { sub1, sub2 });

        // Act
        await _dispatcher.SendToUserAsync(_userId, PushNotificationType.DirectMessage, new PushPayload { Title = "T", Body = "B" });

        // Assert
        _client.Verify(c => c.SendAsync(sub1, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _client.Verify(c => c.SendAsync(sub2, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GivenGroupPostAndUserHasMutedThatGroup_WhenSendGroupPost_ThenDoesNotSend()
    {
        // Arrange
        var prefs = new UserNotificationPreferences(_userId);
        var groupId = Guid.NewGuid();
        _prefRepo.Setup(r => r.FindByUserId(_userId)).ReturnsAsync(prefs);
        _prefRepo.Setup(r => r.FindGroupOverride(_userId, groupId))
            .ReturnsAsync(new UserGroupNotificationPreferences(_userId, groupId, enabled: false));
        _subRepo.Setup(r => r.GetByUserId(_userId)).ReturnsAsync(new List<PushSubscription>
        {
            new PushSubscription(_userId, "https://push.example/1", "p1", "a1")
        });

        // Act
        await _dispatcher.SendToUserAsync(_userId, PushNotificationType.GroupPost,
            new PushPayload { Title = "T", Body = "B", GroupId = groupId });

        // Assert
        _client.Verify(c => c.SendAsync(It.IsAny<PushSubscription>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GivenSendReturnsGone_WhenSendToUser_ThenDeletesExpiredSubscription()
    {
        // Arrange
        var prefs = new UserNotificationPreferences(_userId);
        var sub = new PushSubscription(_userId, "https://push.example/1", "p1", "a1");
        _prefRepo.Setup(r => r.FindByUserId(_userId)).ReturnsAsync(prefs);
        _subRepo.Setup(r => r.GetByUserId(_userId)).ReturnsAsync(new List<PushSubscription> { sub });
        _client.Setup(c => c.SendAsync(sub, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new PushServiceClientException("Gone", System.Net.HttpStatusCode.Gone));

        // Act
        await _dispatcher.SendToUserAsync(_userId, PushNotificationType.DirectMessage, new PushPayload { Title = "T", Body = "B" });

        // Assert
        _subRepo.Verify(r => r.DeleteByEndpoint(sub.Endpoint), Times.Once);
    }
}
```

- [ ] **Step 2 : Créer le wrapper interface `IPushSenderClient` (pour pouvoir mocker `WebPushClient`)**

Créer `src/Application/Services/Push/IPushSenderClient.cs` :

```csharp
using Domain.Entities;

namespace Application.Services.Push;

public interface IPushSenderClient
{
    Task SendAsync(PushSubscription subscription, string payloadJson, CancellationToken ct);
}
```

- [ ] **Step 3 : Faire échouer le test (compile error)**

```bash
dotnet test tests/Tests.Application/Tests.Application.csproj --filter PushNotificationDispatcherTests
```

Attendu : compile error sur `PushNotificationDispatcher` (pas encore créé). C'est bien — on va l'implémenter à la prochaine task.

- [ ] **Step 4 : Commit le test**

```bash
git add tests/Tests.Application/Services/Push/ src/Application/Services/Push/IPushSenderClient.cs
git commit -m "test(push): failing tests for PushNotificationDispatcher + sender interface"
```

---

## Task 9 : Push Dispatcher — implémentation

**Files :**
- Create : `src/Application/Services/Push/PushNotificationDispatcher.cs`
- Create : `src/Infrastructure/Services/Push/WebPushSenderClient.cs`

- [ ] **Step 1 : Créer `PushNotificationDispatcher.cs`**

```csharp
using System.Net;
using System.Text.Json;
using Domain.Entities;
using Domain.Repositories;
using Lib.Net.Http.WebPush;
using Microsoft.Extensions.Logging;

namespace Application.Services.Push;

public class PushNotificationDispatcher : IPushNotificationDispatcher
{
    private readonly IPushSubscriptionRepository _subRepo;
    private readonly INotificationPreferencesRepository _prefRepo;
    private readonly IPushSenderClient _client;
    private readonly ILogger<PushNotificationDispatcher> _logger;

    public PushNotificationDispatcher(
        IPushSubscriptionRepository subRepo,
        INotificationPreferencesRepository prefRepo,
        IPushSenderClient client,
        ILogger<PushNotificationDispatcher> logger)
    {
        _subRepo = subRepo;
        _prefRepo = prefRepo;
        _client = client;
        _logger = logger;
    }

    public async Task SendToUserAsync(Guid userId, PushNotificationType type, PushPayload payload, CancellationToken ct = default)
    {
        var prefs = await _prefRepo.FindByUserId(userId);
        if (prefs == null) return;  // user n'a jamais touché aux prefs => row pas créée; faut subscribe d'abord pour avoir push

        if (!IsTypeEnabled(prefs, type)) return;

        if (type == PushNotificationType.GroupPost && payload.GroupId.HasValue)
        {
            var groupOverride = await _prefRepo.FindGroupOverride(userId, payload.GroupId.Value);
            if (groupOverride != null && !groupOverride.Enabled) return;
        }

        var subs = await _subRepo.GetByUserId(userId);
        if (subs.Count == 0) return;

        var json = JsonSerializer.Serialize(new
        {
            title = payload.Title,
            body = payload.Body,
            url = payload.Url,
            tag = payload.Tag
        });

        var tasks = subs.Select(sub => SendOneAsync(sub, json, ct));
        await Task.WhenAll(tasks);
    }

    public async Task SendToManyAsync(IEnumerable<Guid> userIds, PushNotificationType type, PushPayload payload, CancellationToken ct = default)
    {
        var tasks = userIds.Select(uid => SendToUserAsync(uid, type, payload, ct));
        await Task.WhenAll(tasks);
    }

    private async Task SendOneAsync(PushSubscription sub, string json, CancellationToken ct)
    {
        try
        {
            await _client.SendAsync(sub, json, ct);
        }
        catch (PushServiceClientException ex) when (ex.StatusCode == HttpStatusCode.Gone || ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogInformation("Push subscription {Endpoint} expired (status {Status}); deleting.", sub.Endpoint, ex.StatusCode);
            await _subRepo.DeleteByEndpoint(sub.Endpoint);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Push send failed for subscription {Endpoint}", sub.Endpoint);
        }
    }

    private static bool IsTypeEnabled(UserNotificationPreferences prefs, PushNotificationType type) => type switch
    {
        PushNotificationType.DirectMessage => prefs.NotifyOnDirectMessage,
        PushNotificationType.GroupPost => prefs.NotifyOnGroupPost,
        PushNotificationType.Announcement => prefs.NotifyOnAnnouncement,
        _ => false
    };
}
```

- [ ] **Step 2 : Créer `WebPushSenderClient.cs` (impl de `IPushSenderClient`)**

```csharp
using Application.Services.Push;
using Domain.Entities;
using Lib.Net.Http.WebPush;
using Lib.Net.Http.WebPush.Authentication;
using Microsoft.Extensions.Configuration;
using LibPushSubscription = Lib.Net.Http.WebPush.PushSubscription;

namespace Infrastructure.Services.Push;

public class WebPushSenderClient : IPushSenderClient
{
    private readonly PushServiceClient _client;
    private readonly VapidAuthentication _vapid;

    public WebPushSenderClient(IConfiguration config)
    {
        var subject = config["Vapid:Subject"] ?? throw new InvalidOperationException("Vapid:Subject missing");
        var publicKey = config["Vapid:PublicKey"] ?? throw new InvalidOperationException("Vapid:PublicKey missing");
        var privateKey = config["Vapid:PrivateKey"] ?? throw new InvalidOperationException("Vapid:PrivateKey missing");

        _vapid = new VapidAuthentication(publicKey, privateKey) { Subject = subject };
        _client = new PushServiceClient { DefaultAuthentication = _vapid };
    }

    public Task SendAsync(Domain.Entities.PushSubscription subscription, string payloadJson, CancellationToken ct)
    {
        var libSub = new LibPushSubscription
        {
            Endpoint = subscription.Endpoint,
            Keys = new Dictionary<string, string>
            {
                ["p256dh"] = subscription.P256dh,
                ["auth"] = subscription.Auth
            }
        };
        var msg = new PushMessage(payloadJson) { Topic = subscription.Endpoint.GetHashCode().ToString("X") };
        return _client.RequestPushMessageDeliveryAsync(libSub, msg, ct);
    }
}
```

- [ ] **Step 3 : Enregistrer dans DI**

Dans `src/Application/ConfigureServices.cs`, ajouter :

```csharp
services.AddScoped<IPushNotificationDispatcher, PushNotificationDispatcher>();
```

(Import : `using Application.Services.Push;`)

Dans `src/Infrastructure/ConfigureServices.cs`, ajouter :

```csharp
services.AddSingleton<IPushSenderClient, WebPushSenderClient>();
```

(Import : `using Application.Services.Push;` et `using Infrastructure.Services.Push;`)

- [ ] **Step 4 : Faire passer les tests**

```bash
dotnet test tests/Tests.Application/Tests.Application.csproj --filter PushNotificationDispatcherTests
```

Attendu : 5 tests PASS. Si fail, lire le message et corriger.

- [ ] **Step 5 : Commit**

```bash
git add src/Application/Services/Push/PushNotificationDispatcher.cs src/Infrastructure/Services/Push/WebPushSenderClient.cs src/Application/ConfigureServices.cs src/Infrastructure/ConfigureServices.cs
git commit -m "feat(push): dispatcher + WebPush sender implementations"
```

---

## Task 10 : Endpoint — GET /social/push/vapid-public-key

**Files :**
- Create : `src/Web/Features/Social/Push/GetVapidPublicKey/GetVapidPublicKeyEndpoint.cs`

- [ ] **Step 1 : Créer l'endpoint**

```csharp
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;

namespace Web.Features.Social.Push.GetVapidPublicKey;

public record GetVapidPublicKeyResponse(string PublicKey);

public class GetVapidPublicKeyEndpoint : EndpointWithoutRequest<GetVapidPublicKeyResponse>
{
    private readonly IConfiguration _config;

    public GetVapidPublicKeyEndpoint(IConfiguration config) => _config = config;

    public override void Configure()
    {
        Get("social/push/vapid-public-key");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override Task HandleAsync(CancellationToken ct)
    {
        var key = _config["Vapid:PublicKey"] ?? string.Empty;
        return Send.OkAsync(new GetVapidPublicKeyResponse(key), ct);
    }
}
```

- [ ] **Step 2 : Build**

```bash
dotnet build src/Web/Web.csproj
```

Attendu : 0 errors.

- [ ] **Step 3 : Test manuel rapide**

```bash
make dev  # dans un autre terminal
# puis (en se connectant comme membre, avec un cookie/JWT valide)
curl -b cookies.txt http://localhost:5280/api/social/push/vapid-public-key
```

Attendu : `{"publicKey":"..."}` avec la clé publique.

- [ ] **Step 4 : Commit**

```bash
git add src/Web/Features/Social/Push/GetVapidPublicKey/
git commit -m "feat(push): GET /social/push/vapid-public-key endpoint"
```

---

## Task 11 : Endpoint — POST /social/push/subscriptions

**Files :**
- Create : `src/Web/Features/Social/Push/CreateSubscription/CreateSubscriptionEndpoint.cs`
- Create : `src/Web/Features/Social/Push/CreateSubscription/CreateSubscriptionRequest.cs`

- [ ] **Step 1 : Créer le request**

```csharp
namespace Web.Features.Social.Push.CreateSubscription;

public class CreateSubscriptionRequest
{
    public string Endpoint { get; set; } = string.Empty;
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
}
```

- [ ] **Step 2 : Créer l'endpoint**

```csharp
using Application.Interfaces.Services.Users;
using Domain.Common;
using Domain.Entities;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Push.CreateSubscription;

public class CreateSubscriptionEndpoint : Endpoint<CreateSubscriptionRequest, SucceededOrNotResponse>
{
    private readonly IPushSubscriptionRepository _subRepo;
    private readonly INotificationPreferencesRepository _prefRepo;
    private readonly IAuthenticatedUserService _authUser;

    public CreateSubscriptionEndpoint(
        IPushSubscriptionRepository subRepo,
        INotificationPreferencesRepository prefRepo,
        IAuthenticatedUserService authUser)
    {
        _subRepo = subRepo;
        _prefRepo = prefRepo;
        _authUser = authUser;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("social/push/subscriptions");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreateSubscriptionRequest req, CancellationToken ct)
    {
        var user = _authUser.GetAuthenticatedUser()!;

        // upsert sur Endpoint (unique)
        var existing = await _subRepo.FindByEndpoint(req.Endpoint);
        if (existing == null)
        {
            await _subRepo.Add(new PushSubscription(user.Id, req.Endpoint, req.P256dh, req.Auth));
        }
        else
        {
            existing.UpdateKeys(req.P256dh, req.Auth);
            existing.TouchLastUsed();
            await _subRepo.Update(existing);
        }

        // s'assurer qu'une row de prefs existe (defaults true)
        await _prefRepo.GetOrCreate(user.Id);

        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
```

- [ ] **Step 3 : Build**

```bash
dotnet build src/Web/Web.csproj
```

Attendu : 0 errors.

- [ ] **Step 4 : Commit**

```bash
git add src/Web/Features/Social/Push/CreateSubscription/
git commit -m "feat(push): POST /social/push/subscriptions endpoint"
```

---

## Task 12 : Endpoint — DELETE /social/push/subscriptions

**Files :**
- Create : `src/Web/Features/Social/Push/DeleteSubscription/DeleteSubscriptionEndpoint.cs`

- [ ] **Step 1 : Créer l'endpoint**

```csharp
using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Push.DeleteSubscription;

public class DeleteSubscriptionRequest
{
    public string Endpoint { get; set; } = string.Empty;
}

public class DeleteSubscriptionEndpoint : Endpoint<DeleteSubscriptionRequest, SucceededOrNotResponse>
{
    private readonly IPushSubscriptionRepository _subRepo;

    public DeleteSubscriptionEndpoint(IPushSubscriptionRepository subRepo) => _subRepo = subRepo;

    public override void Configure()
    {
        Delete("social/push/subscriptions");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(DeleteSubscriptionRequest req, CancellationToken ct)
    {
        await _subRepo.DeleteByEndpoint(req.Endpoint);
        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
```

- [ ] **Step 2 : Build + commit**

```bash
dotnet build src/Web/Web.csproj
git add src/Web/Features/Social/Push/DeleteSubscription/
git commit -m "feat(push): DELETE /social/push/subscriptions endpoint"
```

---

## Task 13 : Endpoints — GET et PUT /social/push/preferences

**Files :**
- Create : `src/Web/Features/Social/Push/GetPreferences/GetPreferencesEndpoint.cs`
- Create : `src/Web/Features/Social/Push/UpdatePreferences/UpdatePreferencesEndpoint.cs`

- [ ] **Step 1 : Créer `GetPreferencesEndpoint.cs`**

```csharp
using Application.Interfaces.Services.Users;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Push.GetPreferences;

public record MutedGroupDto(Guid GroupId);
public record GetPreferencesResponse(
    bool DirectMessage,
    bool Announcement,
    bool GroupPost,
    List<MutedGroupDto> MutedGroups
);

public class GetPreferencesEndpoint : EndpointWithoutRequest<GetPreferencesResponse>
{
    private readonly INotificationPreferencesRepository _prefRepo;
    private readonly IAuthenticatedUserService _authUser;

    public GetPreferencesEndpoint(INotificationPreferencesRepository prefRepo, IAuthenticatedUserService authUser)
    {
        _prefRepo = prefRepo;
        _authUser = authUser;
    }

    public override void Configure()
    {
        Get("social/push/preferences");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var user = _authUser.GetAuthenticatedUser()!;
        var prefs = await _prefRepo.GetOrCreate(user.Id);
        var overrides = await _prefRepo.GetGroupOverridesForUser(user.Id);
        var muted = overrides.Where(o => !o.Enabled).Select(o => new MutedGroupDto(o.GroupId)).ToList();

        await Send.OkAsync(new GetPreferencesResponse(
            prefs.NotifyOnDirectMessage,
            prefs.NotifyOnAnnouncement,
            prefs.NotifyOnGroupPost,
            muted
        ), ct);
    }
}
```

- [ ] **Step 2 : Créer `UpdatePreferencesEndpoint.cs`**

```csharp
using Application.Interfaces.Services.Users;
using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Push.UpdatePreferences;

public class UpdatePreferencesRequest
{
    public bool DirectMessage { get; set; }
    public bool Announcement { get; set; }
    public bool GroupPost { get; set; }
}

public class UpdatePreferencesEndpoint : Endpoint<UpdatePreferencesRequest, SucceededOrNotResponse>
{
    private readonly INotificationPreferencesRepository _prefRepo;
    private readonly IAuthenticatedUserService _authUser;

    public UpdatePreferencesEndpoint(INotificationPreferencesRepository prefRepo, IAuthenticatedUserService authUser)
    {
        _prefRepo = prefRepo;
        _authUser = authUser;
    }

    public override void Configure()
    {
        Put("social/push/preferences");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdatePreferencesRequest req, CancellationToken ct)
    {
        var user = _authUser.GetAuthenticatedUser()!;
        await _prefRepo.UpdatePreferences(user.Id, req.DirectMessage, req.Announcement, req.GroupPost);
        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
```

- [ ] **Step 3 : Build + commit**

```bash
dotnet build src/Web/Web.csproj
git add src/Web/Features/Social/Push/GetPreferences/ src/Web/Features/Social/Push/UpdatePreferences/
git commit -m "feat(push): GET + PUT /social/push/preferences endpoints"
```

---

## Task 14 : Endpoint — PUT /social/push/preferences/groups/{groupId}

**Files :**
- Create : `src/Web/Features/Social/Push/UpdateGroupPreference/UpdateGroupPreferenceEndpoint.cs`

- [ ] **Step 1 : Créer l'endpoint**

```csharp
using Application.Interfaces.Services.Users;
using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Push.UpdateGroupPreference;

public class UpdateGroupPreferenceRequest
{
    public Guid GroupId { get; set; }
    public bool Enabled { get; set; }
}

public class UpdateGroupPreferenceEndpoint : Endpoint<UpdateGroupPreferenceRequest, SucceededOrNotResponse>
{
    private readonly INotificationPreferencesRepository _prefRepo;
    private readonly IAuthenticatedUserService _authUser;

    public UpdateGroupPreferenceEndpoint(INotificationPreferencesRepository prefRepo, IAuthenticatedUserService authUser)
    {
        _prefRepo = prefRepo;
        _authUser = authUser;
    }

    public override void Configure()
    {
        Put("social/push/preferences/groups/{GroupId}");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateGroupPreferenceRequest req, CancellationToken ct)
    {
        var user = _authUser.GetAuthenticatedUser()!;
        await _prefRepo.SetGroupOverride(user.Id, req.GroupId, req.Enabled);
        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
```

- [ ] **Step 2 : Build + commit**

```bash
dotnet build src/Web/Web.csproj
git add src/Web/Features/Social/Push/UpdateGroupPreference/
git commit -m "feat(push): PUT /social/push/preferences/groups/{id} endpoint"
```

---

## Task 15 : Frontend — service `pushService.ts`

**Files :**
- Create : `src/Web/vue-app/src/services/pushService.ts`

- [ ] **Step 1 : Lire un service existant pour suivre le pattern**

```bash
cat src/Web/vue-app/src/services/socialService.ts | head -40
```

- [ ] **Step 2 : Créer `pushService.ts`**

```ts
import axios from 'axios'

export interface PushSubscriptionPayload {
  endpoint: string
  p256dh: string
  auth: string
}

export interface PushPreferences {
  directMessage: boolean
  announcement: boolean
  groupPost: boolean
  mutedGroups: { groupId: string }[]
}

export interface PushPreferencesUpdate {
  directMessage: boolean
  announcement: boolean
  groupPost: boolean
}

const pushService = {
  async getVapidPublicKey(): Promise<string> {
    const { data } = await axios.get<{ publicKey: string }>('/api/social/push/vapid-public-key')
    return data.publicKey
  },

  async createSubscription(payload: PushSubscriptionPayload): Promise<void> {
    await axios.post('/api/social/push/subscriptions', payload)
  },

  async deleteSubscription(endpoint: string): Promise<void> {
    await axios.delete('/api/social/push/subscriptions', { data: { endpoint } })
  },

  async getPreferences(): Promise<PushPreferences> {
    const { data } = await axios.get<PushPreferences>('/api/social/push/preferences')
    return data
  },

  async updatePreferences(prefs: PushPreferencesUpdate): Promise<void> {
    await axios.put('/api/social/push/preferences', prefs)
  },

  async updateGroupPreference(groupId: string, enabled: boolean): Promise<void> {
    await axios.put(`/api/social/push/preferences/groups/${groupId}`, { groupId, enabled })
  }
}

export default pushService
```

- [ ] **Step 3 : Commit**

```bash
git add src/Web/vue-app/src/services/pushService.ts
git commit -m "feat(push): frontend pushService REST client"
```

---

## Task 16 : Frontend — composable `usePushSubscription.ts`

**Files :**
- Create : `src/Web/vue-app/src/composables/usePushSubscription.ts`

- [ ] **Step 1 : Créer le composable**

```ts
import { ref, computed, onMounted } from 'vue'
import pushService from '@/services/pushService'

function urlBase64ToUint8Array(base64String: string): Uint8Array {
  const padding = '='.repeat((4 - base64String.length % 4) % 4)
  const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/')
  const rawData = atob(base64)
  const output = new Uint8Array(rawData.length)
  for (let i = 0; i < rawData.length; ++i) output[i] = rawData.charCodeAt(i)
  return output
}

function arrayBufferToBase64(buffer: ArrayBuffer): string {
  let binary = ''
  const bytes = new Uint8Array(buffer)
  for (let i = 0; i < bytes.length; i++) binary += String.fromCharCode(bytes[i])
  return btoa(binary).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '')
}

export function usePushSubscription() {
  const permission = ref<NotificationPermission>('default')
  const subscription = ref<PushSubscription | null>(null)
  const isSubscribing = ref(false)

  const isPwaInstalled = computed(() => {
    if (typeof window === 'undefined') return false
    const standalone = window.matchMedia('(display-mode: standalone)').matches
    const iosStandalone = (navigator as unknown as { standalone?: boolean }).standalone === true
    return standalone || iosStandalone
  })

  const isSupported = computed(() => {
    return typeof window !== 'undefined'
      && 'serviceWorker' in navigator
      && 'PushManager' in window
      && 'Notification' in window
  })

  const isSubscribed = computed(() => subscription.value !== null && permission.value === 'granted')

  async function refreshState() {
    if (!isSupported.value) return
    permission.value = Notification.permission
    const reg = await navigator.serviceWorker.ready
    subscription.value = await reg.pushManager.getSubscription()
  }

  async function subscribe(): Promise<boolean> {
    if (!isSupported.value || !isPwaInstalled.value) return false
    isSubscribing.value = true
    try {
      const perm = await Notification.requestPermission()
      permission.value = perm
      if (perm !== 'granted') return false

      const reg = await navigator.serviceWorker.ready
      const publicKey = await pushService.getVapidPublicKey()
      const sub = await reg.pushManager.subscribe({
        userVisibleOnly: true,
        applicationServerKey: urlBase64ToUint8Array(publicKey)
      })

      const json = sub.toJSON()
      const p256dh = json.keys?.p256dh ?? arrayBufferToBase64(sub.getKey('p256dh')!)
      const auth = json.keys?.auth ?? arrayBufferToBase64(sub.getKey('auth')!)

      await pushService.createSubscription({
        endpoint: sub.endpoint,
        p256dh,
        auth
      })

      subscription.value = sub
      return true
    } finally {
      isSubscribing.value = false
    }
  }

  async function unsubscribe(): Promise<void> {
    if (subscription.value) {
      const endpoint = subscription.value.endpoint
      await subscription.value.unsubscribe()
      await pushService.deleteSubscription(endpoint)
      subscription.value = null
    }
  }

  onMounted(refreshState)

  return {
    permission,
    subscription,
    isSubscribing,
    isPwaInstalled,
    isSupported,
    isSubscribed,
    subscribe,
    unsubscribe,
    refreshState
  }
}
```

- [ ] **Step 2 : Commit**

```bash
git add src/Web/vue-app/src/composables/usePushSubscription.ts
git commit -m "feat(push): usePushSubscription composable"
```

---

## Task 17 : Frontend — étendre le service worker pour push

**Files :**
- Modify : `src/Web/vue-app/src/sw.ts`

- [ ] **Step 1 : Ajouter les listeners push + notificationclick**

Remplacer le contenu de `src/Web/vue-app/src/sw.ts` par :

```ts
/// <reference lib="webworker" />
import { precacheAndRoute } from 'workbox-precaching'

declare const self: ServiceWorkerGlobalScope

precacheAndRoute(self.__WB_MANIFEST)

self.addEventListener('install', () => {
  self.skipWaiting()
})

self.addEventListener('activate', (event) => {
  event.waitUntil(self.clients.claim())
})

interface PushPayload {
  title: string
  body: string
  url?: string
  tag?: string
}

self.addEventListener('push', (event) => {
  if (!event.data) return

  let payload: PushPayload
  try {
    payload = event.data.json() as PushPayload
  } catch {
    payload = { title: 'Expression Danse Beauport', body: event.data.text() }
  }

  event.waitUntil((async () => {
    // Skip si une fenêtre du portail est focused/visible
    const clients = await self.clients.matchAll({ type: 'window', includeUncontrolled: true })
    const visible = clients.some(c => (c as WindowClient).visibilityState === 'visible' && (c as WindowClient).focused)
    if (visible) return

    await self.registration.showNotification(payload.title, {
      body: payload.body,
      icon: '/icons/192.png',
      badge: '/icons/badge.png',
      tag: payload.tag,
      data: { url: payload.url ?? '/social' }
    })
  })())
})

self.addEventListener('notificationclick', (event) => {
  event.notification.close()
  const url = (event.notification.data?.url as string) ?? '/social'
  event.waitUntil((async () => {
    const clients = await self.clients.matchAll({ type: 'window', includeUncontrolled: true })
    const existing = clients.find(c => (c as WindowClient).url.includes(url))
    if (existing) {
      await (existing as WindowClient).focus()
    } else {
      await self.clients.openWindow(url)
    }
  })())
})
```

- [ ] **Step 2 : Build pour vérifier**

```bash
cd src/Web/vue-app && npm run build
```

Attendu : 0 errors, `dist/sw.js` contient les listeners push.

- [ ] **Step 3 : Commit**

```bash
git add src/Web/vue-app/src/sw.ts
git commit -m "feat(push): service worker handles push + notificationclick"
```

---

## Task 18 : Frontend — composant `NotificationsCard.vue`

**Files :**
- Create : `src/Web/vue-app/src/components/social/account/NotificationsCard.vue`

- [ ] **Step 1 : Lire la dernière card existante de `SocialAccount.vue` pour suivre le style**

Déjà fait pendant le brainstorming — pattern `<section class="soc-account__card">` avec header (icône + h3) et body.

- [ ] **Step 2 : Créer le composant**

```vue
<template>
  <section class="soc-account__card">
    <div class="soc-account__card-header">
      <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
        <path d="M18 8a6 6 0 00-12 0c0 7-3 9-3 9h18s-3-2-3-9"/>
        <path d="M13.73 21a2 2 0 01-3.46 0"/>
      </svg>
      <h3>Notifications</h3>
    </div>

    <div class="soc-account__card-body">
      <!-- Pas supporté -->
      <div v-if="!isSupported" class="text-sm" :style="{ color: 'var(--soc-text-muted)' }">
        Votre navigateur ne supporte pas les notifications push.
      </div>

      <!-- PWA pas installée -->
      <div v-else-if="!isPwaInstalled">
        <p class="text-sm mb-3" :style="{ color: 'var(--soc-text)' }">
          Pour recevoir des notifications, vous devez d'abord ajouter l'application à votre écran d'accueil.
        </p>
        <div v-if="isIOS" class="text-sm space-y-2" :style="{ color: 'var(--soc-text-muted)' }">
          <p><strong>Sur iPhone :</strong></p>
          <ol class="list-decimal pl-5 space-y-1">
            <li>Touchez le bouton <strong>Partager</strong> en bas de Safari</li>
            <li>Sélectionnez <strong>« Sur l'écran d'accueil »</strong></li>
            <li>Ouvrez l'application depuis l'icône de votre écran d'accueil</li>
          </ol>
        </div>
        <div v-else-if="isAndroid" class="text-sm space-y-2" :style="{ color: 'var(--soc-text-muted)' }">
          <p><strong>Sur Android :</strong></p>
          <ol class="list-decimal pl-5 space-y-1">
            <li>Touchez le menu <strong>⋮</strong> de Chrome</li>
            <li>Sélectionnez <strong>« Installer l'application »</strong></li>
            <li>Ouvrez l'application depuis l'icône de votre écran d'accueil</li>
          </ol>
        </div>
        <div v-else class="text-sm space-y-2" :style="{ color: 'var(--soc-text-muted)' }">
          <p><strong>Sur ordinateur :</strong> Cliquez sur l'icône d'installation dans la barre d'adresse de votre navigateur.</p>
        </div>
      </div>

      <!-- PWA installée mais permission pas accordée -->
      <div v-else-if="!isSubscribed">
        <p class="text-sm mb-3" :style="{ color: 'var(--soc-text)' }">
          Activez les notifications sur cet appareil pour recevoir messages, publications et annonces.
        </p>
        <button
          type="button"
          class="soc-account__btn-primary inline-flex items-center gap-2"
          :disabled="isSubscribing"
          @click="onActivate"
        >
          <span>{{ isSubscribing ? 'Activation...' : 'Activer sur cet appareil' }}</span>
        </button>
        <p v-if="permission === 'denied'" class="text-sm mt-3 text-red-600">
          Les notifications ont été bloquées. Pour les réactiver, allez dans les paramètres de votre navigateur.
        </p>
      </div>

      <!-- Activé : afficher les switches -->
      <div v-else class="space-y-3">
        <ToggleRow v-model="prefs.directMessage" label="Messages privés" @update:modelValue="savePrefs" />
        <ToggleRow v-model="prefs.announcement" label="Annonces" @update:modelValue="savePrefs" />

        <div>
          <ToggleRow v-model="prefs.groupPost" label="Publications dans mes groupes" @update:modelValue="savePrefs" />

          <div v-if="prefs.groupPost && groups.length > 0" class="mt-2 ml-4 pl-3 border-l-2 space-y-2" :style="{ borderColor: 'var(--soc-border)' }">
            <ToggleRow
              v-for="g in groups"
              :key="g.id"
              :model-value="!mutedGroupIds.has(g.id)"
              :label="g.name"
              size="sm"
              @update:model-value="(v: boolean) => onGroupToggle(g.id, v)"
            />
          </div>
        </div>

        <div class="pt-2">
          <button
            type="button"
            class="text-xs text-red-600 hover:underline"
            @click="onDeactivate"
          >
            Désactiver les notifications sur cet appareil
          </button>
        </div>
      </div>
    </div>
  </section>
</template>

<script lang="ts" setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { usePushSubscription } from '@/composables/usePushSubscription'
import pushService, { type PushPreferences } from '@/services/pushService'
import socialService from '@/services/socialService'
import ToggleRow from './ToggleRow.vue'

const { permission, isSupported, isPwaInstalled, isSubscribed, isSubscribing, subscribe, unsubscribe, refreshState } = usePushSubscription()

const prefs = reactive<{ directMessage: boolean; announcement: boolean; groupPost: boolean }>({
  directMessage: true,
  announcement: true,
  groupPost: true
})

const mutedGroupIds = ref(new Set<string>())
const groups = ref<{ id: string; name: string }[]>([])

const ua = computed(() => navigator.userAgent)
const isIOS = computed(() => /iPad|iPhone|iPod/.test(ua.value) && !(window as unknown as { MSStream?: unknown }).MSStream)
const isAndroid = computed(() => /Android/.test(ua.value))

async function loadPrefs() {
  if (!isSubscribed.value) return
  const data: PushPreferences = await pushService.getPreferences()
  prefs.directMessage = data.directMessage
  prefs.announcement = data.announcement
  prefs.groupPost = data.groupPost
  mutedGroupIds.value = new Set(data.mutedGroups.map(m => m.groupId))
}

async function loadGroups() {
  // socialService doit exposer une méthode pour lister les groupes du membre.
  // Si elle n'existe pas, utiliser la même approche que le menu de groupes existant.
  const list = await socialService.getMyGroups()
  groups.value = list.map(g => ({ id: g.id, name: g.name }))
}

async function savePrefs() {
  await pushService.updatePreferences({
    directMessage: prefs.directMessage,
    announcement: prefs.announcement,
    groupPost: prefs.groupPost
  })
}

async function onGroupToggle(groupId: string, enabled: boolean) {
  await pushService.updateGroupPreference(groupId, enabled)
  if (enabled) mutedGroupIds.value.delete(groupId)
  else mutedGroupIds.value.add(groupId)
}

async function onActivate() {
  const ok = await subscribe()
  if (ok) {
    await loadPrefs()
    await loadGroups()
  }
}

async function onDeactivate() {
  await unsubscribe()
}

onMounted(async () => {
  await refreshState()
  if (isSubscribed.value) {
    await loadPrefs()
    await loadGroups()
  }
})
</script>
```

- [ ] **Step 3 : Créer le sous-composant `ToggleRow.vue`**

`src/Web/vue-app/src/components/social/account/ToggleRow.vue` :

```vue
<template>
  <label class="flex items-center justify-between cursor-pointer" :class="size === 'sm' ? 'text-sm' : ''">
    <span :style="{ color: 'var(--soc-text)' }">{{ label }}</span>
    <span class="relative inline-block" :class="size === 'sm' ? 'w-9 h-5' : 'w-11 h-6'">
      <input type="checkbox" :checked="modelValue" class="peer sr-only" @change="onChange" />
      <span class="absolute inset-0 rounded-full transition-colors peer-checked:bg-red-600 bg-gray-300"></span>
      <span
        class="absolute top-0.5 left-0.5 bg-white rounded-full transition-transform"
        :class="size === 'sm' ? 'w-4 h-4 peer-checked:translate-x-4' : 'w-5 h-5 peer-checked:translate-x-5'"
        :style="{ transform: modelValue ? (size === 'sm' ? 'translateX(1rem)' : 'translateX(1.25rem)') : 'translateX(0)' }"
      ></span>
    </span>
  </label>
</template>

<script lang="ts" setup>
defineProps<{
  modelValue: boolean
  label: string
  size?: 'sm' | 'md'
}>()
const emit = defineEmits<{ (e: 'update:modelValue', v: boolean): void }>()
function onChange(e: Event) {
  emit('update:modelValue', (e.target as HTMLInputElement).checked)
}
</script>
```

- [ ] **Step 4 : Vérifier que `socialService.getMyGroups()` existe**

```bash
grep -n "getMyGroups\|getGroups" src/Web/vue-app/src/services/socialService.ts
```

Si la méthode n'existe pas, l'ajouter :

```ts
async getMyGroups(): Promise<{ id: string; name: string }[]> {
  const { data } = await axios.get<{ id: string; name: string }[]>('/api/social/groups/mine')
  return data
}
```

(Et confirmer que l'endpoint `/api/social/groups/mine` existe ; sinon réutiliser celui qui retourne les groupes du membre courant — chercher dans `src/Web/Features/Social/Groups/`.)

- [ ] **Step 5 : Commit**

```bash
git add src/Web/vue-app/src/components/social/account/
git commit -m "feat(push): NotificationsCard component with toggles + install instructions"
```

---

## Task 19 : Frontend — intégrer dans `SocialAccount.vue`

**Files :**
- Modify : `src/Web/vue-app/src/views/social/SocialAccount.vue`

- [ ] **Step 1 : Importer et insérer la card**

Ouvrir `src/Web/vue-app/src/views/social/SocialAccount.vue`. Dans le `<script setup>`, ajouter l'import :

```ts
import NotificationsCard from '@/components/social/account/NotificationsCard.vue'
```

Dans le `<template>`, après la card "Mot de passe" (ou à l'endroit qui te semble logique), insérer :

```html
<NotificationsCard />
```

- [ ] **Step 2 : Test manuel**

```bash
make dev
```

Naviguer vers `/social/mon-compte` (ou la route exacte de SocialAccount). Vérifier :
- Si dans navigateur normal (pas PWA) : voir les instructions d'installation
- Si PWA installée : voir le bouton "Activer sur cet appareil"
- Cliquer activer → prompt de permission → toggles apparaissent

- [ ] **Step 3 : Commit**

```bash
git add src/Web/vue-app/src/views/social/SocialAccount.vue
git commit -m "feat(push): NotificationsCard intégrée dans Mon compte"
```

---

## Task 20 : Backend integration — DM push depuis `SendMessageEndpoint`

**Files :**
- Modify : `src/Web/Features/Social/Messages/Send/SendMessageEndpoint.cs`

- [ ] **Step 1 : Injecter `IPushNotificationDispatcher`**

Ouvrir `src/Web/Features/Social/Messages/Send/SendMessageEndpoint.cs`. Ajouter au constructeur :

```csharp
private readonly IPushNotificationDispatcher _dispatcher;

public SendMessageEndpoint(
    IConversationService conversationService,
    IConversationRepository conversationRepository,
    IAuthenticatedUserService authenticatedUserService,
    IMemberRepository memberRepository,
    IHubContext<ChatHub> hubContext,
    IPushNotificationDispatcher dispatcher)  // <-- ajouter
{
    // ...
    _dispatcher = dispatcher;  // <-- ajouter
}
```

Import à ajouter en haut : `using Application.Services.Push;`

- [ ] **Step 2 : Appeler le dispatcher après le SignalR push**

Juste après le bloc `if (recipientUser != null) { ... await _hubContext.Clients.Client(connectionId).SendAsync(...) ... }`, mais à l'extérieur du `if (connectionId != null)` (on push même si SignalR pas connecté) :

Trouver dans le fichier le bloc :
```csharp
if (recipientUser != null)
{
    var connectionId = ChatHub.GetConnectionId(recipientUser.UserId.ToString());
    if (connectionId != null)
    {
        // ... SendAsync existant
    }
}
```

Le remplacer par :

```csharp
if (recipientUser != null)
{
    var connectionId = ChatHub.GetConnectionId(recipientUser.UserId.ToString());
    if (connectionId != null)
    {
        var mediaPayload = message.Media
            .OrderBy(m => m.SortOrder)
            .Select(m => new
            {
                m.Id,
                m.MediaUrl,
                m.ThumbnailUrl,
                m.OriginalUrl,
                m.ContentType,
                m.Size,
                m.SortOrder
            })
            .ToList();

        await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveMessage", new
        {
            message.Id,
            message.Content,
            SenderName = member.FullName,
            message.ConversationId,
            Media = mediaPayload
        }, ct);
    }

    // Push notification (s'envoie même si SignalR pas connecté ; le SW skip si l'app est focused)
    var preview = TruncatePreview(req.Content);
    await _dispatcher.SendToUserAsync(recipientUser.UserId, PushNotificationType.DirectMessage, new PushPayload
    {
        Title = member.FullName,
        Body = preview,
        Url = $"/social/messages/{req.ConversationId}",
        Tag = $"dm-{req.ConversationId}"
    }, ct);
}
```

Et ajouter en bas de la classe :

```csharp
private static string TruncatePreview(string content, int max = 120)
{
    if (string.IsNullOrEmpty(content)) return "Nouveau message";
    var trimmed = content.Trim();
    return trimmed.Length <= max ? trimmed : trimmed.Substring(0, max - 1).TrimEnd() + "…";
}
```

- [ ] **Step 3 : Build**

```bash
dotnet build src/Web/Web.csproj
```

Attendu : 0 errors.

- [ ] **Step 4 : Commit**

```bash
git add src/Web/Features/Social/Messages/Send/SendMessageEndpoint.cs
git commit -m "feat(push): trigger push on new direct message"
```

---

## Task 21 : Backend integration — push sur post de groupe

**Files :**
- Modify : `src/Web/Features/Social/Posts/Create/CreatePostEndpoint.cs`

- [ ] **Step 1 : Trouver comment lister les members d'un groupe**

```bash
cat src/Domain/Repositories/IGroupMemberRepository.cs
```

Identifier la méthode (probablement `GetByGroupId(Guid groupId)` ou similaire) qui retourne les `GroupMember` du groupe. Noter le nom exact pour l'utiliser plus bas.

- [ ] **Step 2 : Modifier `CreatePostEndpoint.cs`**

Injecter les nouvelles dépendances dans le constructeur :

```csharp
private readonly IPushNotificationDispatcher _dispatcher;
private readonly IGroupMemberRepository _groupMemberRepo;
private readonly IGroupRepository _groupRepo;

public CreatePostEndpoint(
    IPostService postService,
    IAuthenticatedUserService authenticatedUserService,
    IMemberRepository memberRepository,
    IPushNotificationDispatcher dispatcher,
    IGroupMemberRepository groupMemberRepo,
    IGroupRepository groupRepo)
{
    _postService = postService;
    _authenticatedUserService = authenticatedUserService;
    _memberRepository = memberRepository;
    _dispatcher = dispatcher;
    _groupMemberRepo = groupMemberRepo;
    _groupRepo = groupRepo;
}
```

Imports : `using Application.Services.Push;`

Modifier le `HandleAsync` — après `await _postService.CreatePost(...)`, capturer le post créé et fan-out :

```csharp
Domain.Entities.Post createdPost;
try
{
    createdPost = await _postService.CreatePost(req.GroupId, member.Id, req.Content, type, media);
}
catch (InvalidOperationException ex)
{
    await Send.OkAsync(new SucceededOrNotResponse(false, new Error("InvalidPost", ex.Message)), ct);
    return;
}

// Push fan-out aux membres du groupe (sauf l'auteur)
if (req.GroupId.HasValue)
{
    var group = await _groupRepo.FindById(req.GroupId.Value);
    var groupMembers = await _groupMemberRepo.GetByGroupId(req.GroupId.Value);
    var recipientUserIds = groupMembers
        .Where(gm => gm.MemberId != member.Id)
        .Select(gm => gm.Member.UserId)
        .Distinct()
        .ToList();

    var preview = TruncatePostPreview(req.Content);
    await _dispatcher.SendToManyAsync(recipientUserIds, PushNotificationType.GroupPost, new PushPayload
    {
        Title = $"{group?.Name ?? "Nouveau post"} · {member.FullName}",
        Body = preview,
        Url = $"/social/posts/{createdPost.Id}",
        Tag = $"post-{createdPost.Id}",
        GroupId = req.GroupId.Value
    }, ct);
}

await Send.OkAsync(new SucceededOrNotResponse(true), ct);
```

Ajouter en bas de la classe :

```csharp
private static string TruncatePostPreview(string content, int max = 120)
{
    if (string.IsNullOrEmpty(content)) return "Nouvelle publication";
    var stripped = System.Text.RegularExpressions.Regex.Replace(content, "<.*?>", string.Empty).Trim();
    if (string.IsNullOrEmpty(stripped)) return "Nouvelle publication";
    return stripped.Length <= max ? stripped : stripped.Substring(0, max - 1).TrimEnd() + "…";
}
```

**Note :** vérifier le type de retour de `_postService.CreatePost` — si elle retourne `void`, ajouter une variante qui retourne le `Post` créé, ou récupérer le dernier post créé via `_postRepository`. Si la méthode existante retourne déjà l'entité, tant mieux. Sinon, modifier la signature dans `IPostService` et `PostService` pour retourner `Task<Post>`.

Dans le doute, vérifier d'abord :

```bash
grep -A 3 "Task.*CreatePost" src/Application/Services/Posts/IPostService.cs src/Application/Services/Posts/PostService.cs
```

Si la signature actuelle est `Task CreatePost(...)`, la changer en `Task<Post> CreatePost(...)` et retourner le post à la fin de la méthode dans `PostService`. C'est un changement minimal, contained, et utile.

- [ ] **Step 3 : Vérifier que `req.GroupId` peut être `null` (cas annonce)**

Si `CreatePostEndpoint` gère aussi les annonces (post sans groupe), le `if (req.GroupId.HasValue)` ci-dessus skip le fan-out de groupe. Les annonces auront leur propre endpoint (Task 22) — donc pas besoin de gérer ici.

Confirmer en relisant `CreatePostRequest.cs` que `GroupId` est `Guid?`.

- [ ] **Step 4 : Build**

```bash
dotnet build src/Web/Web.csproj
```

Attendu : 0 errors.

- [ ] **Step 5 : Commit**

```bash
git add src/Web/Features/Social/Posts/Create/CreatePostEndpoint.cs src/Application/Services/Posts/
git commit -m "feat(push): fan-out push to group members on new post"
```

---

## Task 22 : Backend integration — push sur annonce

**Files :**
- Modify : `src/Web/Features/Social/Announcements/CreateAnnouncement/CreateAnnouncementEndpoint.cs`

- [ ] **Step 1 : Lire l'endpoint existant**

```bash
cat src/Web/Features/Social/Announcements/CreateAnnouncement/*.cs
```

Identifier la méthode utilisée pour créer l'annonce et le repository de members (déjà injecté ou non).

- [ ] **Step 2 : Injecter `IPushNotificationDispatcher` + `IMemberRepository` (si pas déjà là)**

```csharp
private readonly IPushNotificationDispatcher _dispatcher;
private readonly IMemberRepository _memberRepo;

public CreateAnnouncementEndpoint(
    /* deps existantes */,
    IPushNotificationDispatcher dispatcher,
    IMemberRepository memberRepo)
{
    /* assign existing */
    _dispatcher = dispatcher;
    _memberRepo = memberRepo;
}
```

Imports : `using Application.Services.Push;`

- [ ] **Step 3 : Vérifier que `IMemberRepository` a une méthode pour lister tous les members actifs**

```bash
cat src/Domain/Repositories/IMemberRepository.cs
```

Identifier la méthode existante. Si aucune ne convient (ex : `GetAll()` n'existe pas), en ajouter une :

Dans `IMemberRepository.cs` :

```csharp
Task<List<Member>> GetAllActive();
```

Dans `MemberRepository.cs` (impl) :

```csharp
public Task<List<Member>> GetAllActive()
    => _context.Members.AsNoTracking()
        .Where(m => m.User.IsActive)  // ajuster selon le modèle existant (peut-être m.IsActive ou autre)
        .ToListAsync();
```

(Adapter à la convention "actif" du repo — vérifier ce qui existe déjà.)

- [ ] **Step 4 : Fan-out après création**

Dans `HandleAsync`, après la création de l'annonce :

```csharp
var allMembers = await _memberRepo.GetAllActive();
var recipientUserIds = allMembers
    .Where(m => m.Id != currentMember.Id)
    .Select(m => m.UserId)
    .Distinct()
    .ToList();

var preview = TruncateAnnouncementPreview(req.Content);
await _dispatcher.SendToManyAsync(recipientUserIds, PushNotificationType.Announcement, new PushPayload
{
    Title = "📢 Nouvelle annonce",
    Body = preview,
    Url = "/social/annonces",
    Tag = $"announcement-{createdAnnouncement.Id}"
}, ct);
```

Ajouter en bas de la classe :

```csharp
private static string TruncateAnnouncementPreview(string content, int max = 120)
{
    if (string.IsNullOrEmpty(content)) return "Nouvelle annonce";
    var stripped = System.Text.RegularExpressions.Regex.Replace(content, "<.*?>", string.Empty).Trim();
    if (string.IsNullOrEmpty(stripped)) return "Nouvelle annonce";
    return stripped.Length <= max ? stripped : stripped.Substring(0, max - 1).TrimEnd() + "…";
}
```

(Ajuster les noms de variables `currentMember`, `createdAnnouncement` selon ce qui existe dans l'endpoint.)

- [ ] **Step 5 : Build**

```bash
dotnet build src/Web/Web.csproj
```

Attendu : 0 errors.

- [ ] **Step 6 : Commit**

```bash
git add src/Web/Features/Social/Announcements/CreateAnnouncement/ src/Domain/Repositories/IMemberRepository.cs src/Infrastructure/Repositories/
git commit -m "feat(push): fan-out push to all members on new announcement"
```

---

## Task 23 : Test E2E manuel sur téléphone

**Files :** aucun

- [ ] **Step 1 : Déployer en staging ou exposer le dev local en HTTPS**

Push notifications nécessitent HTTPS (sauf localhost). Pour tester sur un vrai téléphone, deux options :

**Option A** : Déployer sur le VPS de prod (branche feature ou environnement de staging si dispo).

**Option B** : Tunnel local HTTPS via `ngrok` :

```bash
ngrok http 5280
```

Ouvrir l'URL HTTPS retournée par ngrok sur le téléphone.

- [ ] **Step 2 : Test iPhone (PWA)**

1. Ouvrir l'URL dans Safari
2. Partager → Sur l'écran d'accueil
3. Lancer depuis l'icône
4. Mon compte > Notifications → "Activer sur cet appareil" → Autoriser
5. Toggle ses 3 switches
6. Verrouiller l'iPhone
7. Depuis un autre compte, envoyer un DM, créer un post de groupe, créer une annonce
8. Vérifier que les 3 notifs arrivent sur l'iPhone (même verrouillé)
9. Tap une notif → l'app s'ouvre sur la bonne page

- [ ] **Step 3 : Test Android Chrome**

Même flow mais via menu ⋮ > Installer l'application.

- [ ] **Step 4 : Test du skip-when-focused**

App ouverte et focused → envoyer un événement → vérifier qu'**aucune** notif système n'apparaît (juste le toast in-app SignalR existant pour les DM). Mettre l'app en arrière-plan → renvoyer → la notif système doit apparaître.

- [ ] **Step 5 : Test du mute par groupe**

1. Mute un groupe spécifique (Mon compte > Notifications > toggle de ce groupe OFF)
2. Depuis un autre compte, créer un post dans ce groupe
3. Vérifier que **aucune notif** n'arrive
4. Re-toggle le groupe ON
5. Recréer un post → notif arrive

- [ ] **Step 6 : Test du soft-delete sur subscription expirée**

1. Activer les notifs sur un appareil
2. Désinstaller la PWA / révoquer permission
3. Depuis un autre compte, déclencher un événement
4. Vérifier dans les logs backend que la subscription a été supprimée silencieusement (`Push subscription ... expired ... deleting.`)

- [ ] **Step 7 : Documenter les findings**

Si bugs/regressions trouvés → ouvrir des issues et les fixer avant de déclarer la feature complete. Sinon, ajouter une note dans le commit message du dernier fix ou en ouvrir un commit doc.

---

## Self-review

Une fois toutes les tasks complétées :

1. Tous les endpoints sont sous `/social/push/*` ✓
2. Les 3 types de notifications sont câblés (DM, post groupe, annonce) ✓
3. Préférences user-level + opt-out par groupe ✓
4. Skip-when-focused implémenté dans le SW ✓
5. Soft-delete sur 410/404 ✓
6. PWA installable + instructions par OS ✓
7. Tests unitaires sur le dispatcher (5 cas) ✓
8. Pas de tests E2E automatisés sur les notifs (manuel only) ✓
