# Contribuer

## Branches

- **main** - Production uniquement. Protégée, aucun push direct. Reçoit les merges depuis `dev` lors des releases.
- **dev** - Branche d'intégration. Cible de toutes les PRs (`feat/*`, `fix/*`, `chore/*`).
- **feat/\*, fix/\*, chore/\*** - Branches de travail, créées depuis `dev`.

Quand `dev` est stable, une PR `dev → main` est créée pour déployer en production.

## Commits

Le projet suit la convention [Conventional Commits 1.0.0](https://www.conventionalcommits.org/fr/v1.0.0/).

Format : `<type>[scope optionnel]: <description>`

Types : `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `build`, `chore`, `ci`.

Le titre doit être suffisamment clair par lui-même. Le body est optionnel et reste très court si présent.
