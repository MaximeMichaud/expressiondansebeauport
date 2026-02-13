# Expression Danse de Beauport
.NET CI

## LIENS IMPORTANTS

GitHub : https://github.com/MaximeMichaud/expressiondansebeauport
Trello : https://trello.com/b/cn6UmcsK/projet-integrateur
Hébergement : https://expressiondansebeauport.azurewebsites.net

## DESCRIPTION

Ce projet consiste en le développement d'une application web pour Expression Danse de Beauport inc., un organisme à but non lucratif offrant des cours de danse à Québec depuis plus de 30 ans, dans le cadre du cours Projet intégrateur au Cégep Garneau.

L'application permet la gestion administrative de l'école de danse : gestion des membres, des produits, authentification sécurisée avec 2FA, interface bilingue (fr/en) et intégration avec des services externes (Xero, SendGrid, Azure Blob Storage).

## FONCTIONNALITÉS PRINCIPALES

- **Gestion des membres** : Création, modification, suppression et recherche de membres
- **Gestion des produits** : Catalogue de produits avec formulaires dynamiques
- **Authentification sécurisée** : JWT, Argon2, authentification à deux facteurs (2FA)
- **Interface bilingue** : Français (fr-CA) et anglais (en-CA)
- **Tableau de bord administratif** : Interface complète de gestion
- **Intégrations externes** : Xero (comptabilité), SendGrid (courriels), Azure Blob Storage (fichiers)

## TECHNOLOGIES UTILISÉES

### Backend

- **Framework** : ASP.NET Core avec FastEndpoints
- **Langage** : C# (.NET 10)
- **Base de données** : SQL Server 2022
- **ORM** : Entity Framework Core 10
- **Authentification** : JWT Bearer + Argon2
- **Architecture** : Clean Architecture (Domain, Application, Infrastructure, Persistence, Web)

### Frontend

- **Framework** : Vue 3 (Composition API)
- **Build** : Vite 7
- **Langage** : TypeScript
- **CSS** : Tailwind CSS 4
- **State** : Pinia
- **Routing** : Vue Router 5
- **UI** : Reka-ui, Lucide Icons
- **i18n** : vue3-i18n

### Outils de développement

- **Gestion de version** : Git / GitHub
- **Gestion de projet** : Trello
- **CI/CD** : GitHub Actions
- **Conteneurisation** : Docker / Docker Compose

## PRÉREQUIS

- .NET SDK 10
- Node.js 22+ (via nvm)
- SQL Server 2022 (ou Docker)
- Git
- dotnet-ef CLI tool

## INSTALLATION

### Démarrage rapide (Docker)

```bash
# 1. Cloner le repository
git clone https://github.com/MaximeMichaud/expressiondansebeauport.git
cd expressiondansebeauport

# 2. Lancer avec Docker
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
│   ├── Infrastructure/      # Intégrations externes (Xero, SendGrid, Azure)
│   ├── Persistence/         # DbContext, migrations, configurations EF
│   └── Web/                 # API FastEndpoints + frontend Vue
│       ├── Features/        # Endpoints organisés par fonctionnalité
│       └── vue-app/         # Application Vue 3 SPA
├── tests/
│   ├── Tests.Application/
│   ├── Tests.Domain/
│   ├── Tests.Infrastructure/
│   ├── Tests.Web/
│   └── Tests.Common/
├── docker-compose.yml
├── Dockerfile
└── .github/workflows/       # CI GitHub Actions
```

## INSTANT (NodaTime)

Un `Instant` représente un moment dans le temps, toujours en UTC. `InstantHelper.GetUtcNow()` retourne la date/heure UTC courante.

Lors du parsing d'une chaîne vers un Instant, la date est conservée telle quelle mais sauvegardée en UTC en base de données. La conversion est automatique à la lecture.

**Comparaison avec la date courante :**
```csharp
myObject.ItsInstant.ToDateTimeUtc() < DateTime.Now
```

## LICENCE

Ce projet est développé dans le cadre du cours Projet intégrateur au Cégep Garneau.