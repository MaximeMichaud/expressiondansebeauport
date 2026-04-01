# Expression Danse de Beauport

[![CI](https://github.com/MaximeMichaud/expressiondansebeauport/actions/workflows/ci.yml/badge.svg)](https://github.com/MaximeMichaud/expressiondansebeauport/actions/workflows/ci.yml)

## LIENS

- **Site :** https://expression.mich.sh
- **GitHub :** https://github.com/MaximeMichaud/expressiondansebeauport

## DESCRIPTION

Application web pour Expression Danse de Beauport inc., un organisme à but non lucratif offrant des cours de danse à Québec depuis plus de 30 ans.

L'application comprend un CMS sur mesure pour gérer le site public (pages, menus, médias) et une plateforme sociale interne permettant aux membres de communiquer, rejoindre des groupes de danse et recevoir des annonces.

## FONCTIONNALITÉS

### Site public
- Pages dynamiques avec éditeur visuel par blocs (texte riche, galerie d'images, Google Maps, FAQ, boutons d'appel à l'action) ou HTML brut avec CSS personnalisé
- Menus de navigation dynamiques (principal et footer) avec sous-menus
- Footer configurable avec liens sociaux et logos de partenaires

### Panneau d'administration
- Gestion des pages CMS (créer, modifier, dupliquer, supprimer)
- Bibliothèque de médias (images, documents, vidéos)
- Éditeur de menus avec drag-and-drop
- Personnalisation du site (couleurs, polices, logo, favicon, informations de contact)
- Santé du site (connectivité DB, version .NET, mémoire, statistiques de contenu)
- Import/export des données CMS en JSON
- Sauvegardes de la base de données (manuelles et planifiées, avec compression ZSTD ou export Azure BACPAC)
- Gestion des membres (liste, recherche, promotion/rétrogradation professeur)
- Gestion des groupes de danse (création, saisons, codes d'invitation, archivage)
- Annonces site-wide

### Plateforme sociale (EDB Social)
- Groupes de danse avec fil de publications (texte, photo, sondage, fichier)
- Réactions, commentaires, épinglage de publications
- Sondages avec vote
- Répertoire de membres avec recherche et profils
- Messagerie privée en temps réel (SignalR)
- Inscription avec confirmation par courriel

### Authentification
- JWT avec tokens stockés en cookies, refresh tokens (2 jours)
- Hachage des mots de passe avec Argon2
- Authentification à deux facteurs par courriel
- Réinitialisation de mot de passe via SendGrid

## TECHNOLOGIES

### Backend

- **Framework :** ASP.NET Core 10 avec FastEndpoints
- **Langage :** C# 14 (.NET 10)
- **Base de données :** SQL Server 2022 (dev) / Azure SQL Edge (prod)
- **ORM :** Entity Framework Core 10 avec NodaTime
- **Authentification :** JWT Bearer + Argon2 + 2FA par courriel
- **Temps réel :** SignalR
- **Courriels :** SendGrid
- **Journalisation :** Serilog
- **Architecture :** Clean Architecture (Domain, Application, Infrastructure, Persistence, Web)

### Frontend

- **Framework :** Vue 3 (Composition API)
- **Build :** Vite 8
- **Langage :** TypeScript 5
- **CSS :** Tailwind CSS 4
- **State :** Pinia 3
- **Routing :** Vue Router 5
- **Éditeur riche :** TipTap
- **Icônes :** Lucide, Material Design Icons
- **i18n :** vue-i18n (français uniquement)
- **IoC :** InversifyJS

### Infrastructure

- **Hébergement :** VPS Vultr + Docker Compose + Caddy (HTTPS automatique)
- **CI/CD :** GitHub Actions (lint, build, tests, PR previews sur Azure Container Apps)
- **Conteneurisation :** Docker (multi-stage build)

## BRANCHES

- **main** - Production uniquement. Protégée, aucun push direct. Reçoit les merges depuis `dev` lors des releases.
- **dev** - Branche d'intégration. Cible de toutes les PRs (`feat/*`, `fix/*`, `chore/*`).
- **feat/\*, fix/\*, chore/\*** - Branches de travail, créées depuis `dev`.

Quand `dev` est stable, une PR `dev → main` est créée pour déployer en production.

## PRÉREQUIS

- .NET SDK 10
- Node.js 22+
- SQL Server 2022 (ou Docker)
- Git

## INSTALLATION

### Démarrage rapide (Docker)

```bash
git clone https://github.com/MaximeMichaud/expressiondansebeauport.git
cd expressiondansebeauport
docker compose up
```

L'application sera accessible à : http://localhost:8080/

### Développement local

```bash
# 1. Installer les dépendances frontend
cd src/Web/vue-app
npm install

# 2. Générer une clé secrète JWT
node -e "console.log(require('crypto').randomBytes(32).toString('hex'))"
# Copier la valeur dans appsettings.Development.json -> JwtToken:SecretKey

# 3. Lancer le frontend (watch mode)
npm run dev

# 4. Lancer le backend (dans un autre terminal)
cd src/Web
dotnet dev-certs https --trust
dotnet watch run
```

### Seed

- Utilisateur par défaut : `admin@gmail.com` / `Qwerty123!`

### Migrations

```bash
cd src/Persistence

# Créer une migration
dotnet ef migrations add {NomMigration} --startup-project ../Web/

# Appliquer les migrations
dotnet ef database update --startup-project ../Web/
```

## STRUCTURE DU PROJET

```
expressiondansebeauport/
├── src/
│   ├── Domain/              # Entités, value objects, interfaces repositories
│   ├── Application/         # Services, DTOs, mappings, exceptions
│   ├── Infrastructure/      # SendGrid, repositories EF Core, backup providers
│   ├── Persistence/         # DbContext, migrations, configurations EF, intercepteurs
│   └── Web/                 # API FastEndpoints + frontend Vue
│       ├── Features/        # Endpoints (Admins, Public, Social, Users)
│       ├── Hubs/            # SignalR (ChatHub)
│       ├── BackgroundServices/  # Scheduler de sauvegardes
│       └── vue-app/         # Application Vue 3 SPA
├── tests/
│   ├── Tests.Application/
│   ├── Tests.Domain/
│   ├── Tests.Infrastructure/
│   ├── Tests.Web/
│   └── Tests.Common/
├── deploy/                  # Script de setup VPS
├── docker-compose.yml       # Développement
├── docker-compose.prod.yml  # Production
├── Caddyfile                # Reverse proxy HTTPS
├── Dockerfile               # Build multi-stage (Node + .NET)
└── .github/workflows/       # CI, PR previews, cleanup
```

## INSTANT (NodaTime)

Un `Instant` représente un moment dans le temps, toujours en UTC. `InstantHelper.GetLocalNow()` retourne la date/heure UTC courante.

Lors du parsing d'une chaîne vers un Instant, la date est conservée telle quelle mais sauvegardée en UTC en base de données.

## LICENCE

Ce projet est développé dans le cadre du cours Projet intégrateur au Cégep Garneau.
