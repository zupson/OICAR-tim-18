# Spendly — Web Frontend

Angular 21 web frontend for the Spendly personal finance management application.

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | Angular 21 (standalone components) |
| Language | TypeScript 5 |
| Reactivity | Angular Signals (`signal`, `computed`) |
| Styling | SCSS (global `src/styles.scss`) |
| HTTP | `HttpClient` with `HttpInterceptorFn` (JWT) |
| Routing | Angular Router with lazy-loaded page components |
| Auth guard | `CanActivateFn` with auto-login from `localStorage` |
| Build tool | Angular CLI / `@angular/build` (esbuild) |
| Package manager | npm |

---

## Project Structure

```
SpendlyWeb/
├── src/
│   ├── app/
│   │   ├── core/
│   │   │   ├── guards/         # authGuard (CanActivateFn)
│   │   │   ├── interceptors/   # jwtInterceptor (HttpInterceptorFn)
│   │   │   └── services/
│   │   │       ├── api.service.ts          # Generic HttpClient wrapper
│   │   │       ├── auth.service.ts         # Login, register, loadUserData
│   │   │       ├── notification.service.ts # localStorage notifications + budget alerts
│   │   │       └── state.service.ts        # Global signal-based state
│   │   ├── models/             # TypeScript interfaces (User, Cost, Revenue, Budget, ...)
│   │   ├── pages/
│   │   │   ├── login/
│   │   │   ├── register/
│   │   │   ├── dashboard/
│   │   │   ├── transactions/
│   │   │   ├── categories/
│   │   │   ├── reports/
│   │   │   ├── family/
│   │   │   ├── budget/
│   │   │   ├── notifications/
│   │   │   └── settings/
│   │   └── shared/
│   │       └── layout/
│   │           ├── shell/      # App shell (loads user data on init)
│   │           ├── sidebar/    # Desktop navigation
│   │           └── bottom-nav/ # Mobile navigation
│   ├── styles.scss             # Global dark-theme styles
│   └── main.ts
├── angular.json
├── proxy.conf.json             # Dev proxy -> backend on localhost:5153
├── package.json
└── tsconfig.json
```

---

## Prerequisites

- **Node.js** 20 or later
- **npm** 9 or later
- **.NET 8 SDK** — required to run the backend API

---

## Getting Started

### 1. Install dependencies

```bash
cd SpendlyWeb
npm install
```

### 2. Start the backend

The frontend proxies all `/api` requests to the backend running on port **5153**.

```bash
cd ../SpendlyWebAPI
dotnet run --launch-profile http
```

### 3. Start the dev server

```bash
npx ng serve
```

Open http://localhost:4200.

---

## Available Scripts

| Command | Description |
|---------|-------------|
| `npx ng serve` | Start development server with hot reload |
| `npx ng build` | Production build (output: `dist/spendly`) |
| `npx ng build --configuration development` | Development build with source maps |

---

## API Proxy

During development, the Angular dev server proxies all `/api/*` requests to the backend:

```
/api/* -> http://localhost:5153
```

Configured in `proxy.conf.json`. No CORS configuration is needed for local development.

---

## Pages

| Route | Page | Description |
|-------|------|-------------|
| `/login` | Login | Username + password authentication |
| `/register` | Register | New account creation |
| `/dashboard` | Dashboard | Monthly overview, charts, recent transactions |
| `/transactions` | Transactions | Full transaction table with filtering and search |
| `/categories` | Categories | Expense and income category management |
| `/reports` | Reports | 6-month trend charts and detailed breakdown |
| `/family` | Family Group | Member list, role overview, invite system |
| `/budget` | Budget | Monthly spending limit with progress tracking |
| `/notifications` | Notifications | Budget alerts and system notifications |
| `/settings` | Settings | Profile, currency, notification preferences |

---

## Authentication

- JWT token is stored in `localStorage` under the key `spendly_token`
- The `jwtInterceptor` automatically attaches the token to every outgoing API request
- The `authGuard` redirects unauthenticated users to `/login`
- On page refresh, `tryAutoLogin()` restores the session from `localStorage`

---

## Notifications

Budget alerts are calculated client-side via `NotificationService`:

- **80% alert** — fires when monthly spending reaches 80% of the budget limit
- **100% alert** — fires when the budget is exceeded
- Alerts are stored in `localStorage` and deduplicated by ID
- Deleting a transaction re-evaluates thresholds so alerts can re-fire if spending drops back below a threshold and then rises again
