# Expression Danse Beauport - Frontend Vue

## Installation

```bash
npm install
```

## Scripts

```bash
npm run dev          # serveur Vite sur http://localhost:8080
npm run build        # build de production vers ../wwwroot
npm run build:check  # type-check Vue/TypeScript + build de production
npm run lint         # ESLint
npm run lint:a11y    # lint ciblé accessibilité
npm run test:a11y    # tests Playwright + axe
npm run lighthouse:a11y
```

## Variables Vite

- `.env.development` configure le serveur Vite local.
- `.env.production` configure le build Docker et le build CI.
- Les valeurs `VITE_*` sont intégrées au bundle au moment du build.

## Stack

- Vue 3 + TypeScript
- Vite 8
- Tailwind CSS 4
- Pinia
- Vue Router
- vue-i18n
- TipTap
- Vite PWA
- Playwright + axe pour les tests d'accessibilité

## Assets statiques

Les fichiers dans `public/` sont copiés par Vite vers `../wwwroot` au build. Les médias seedés doivent rester dans `public/uploads/`, car le Dockerfile copie le build Vue puis synchronise ces fichiers vers `/app/seed-uploads`.
