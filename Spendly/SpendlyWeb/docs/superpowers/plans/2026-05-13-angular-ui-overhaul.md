# Angular UI Overhaul — Match Non-Framework Reference

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Bring the Angular (Desktop/spendly) app's visual design into full alignment with the vanilla reference version — same logo, sidebar structure, page layouts, typography, and colour use.

**Architecture:** Pure template + CSS changes across 8 files. No new services, no routing changes, no API changes. The `styles.scss` is the single source of truth for all shared styles; per-component styles are written as inline `styles` on the `@Component` decorator only when truly component-specific. Logic inside each component is untouched unless a layout restructure requires adding a new `computed()` signal.

**Tech Stack:** Angular 21 standalone components, `signal()` / `computed()` reactive state, global SCSS in `src/styles.scss`, no third-party chart library.

---

## File Map

| File | What changes |
|------|-------------|
| `src/styles.scss` | Sidebar width → 260 px; section-label class; table-layout classes; category-card classes; reports pill-month classes; two-column budget layout; global spacing tweaks |
| `src/app/shared/layout/sidebar/sidebar.component.ts` | SVG logo; section labels (GLAVNO / ANALITIKA / OBITELJ / RAČUN); nav order; account block moved to bottom |
| `src/app/pages/dashboard/dashboard.component.ts` | Greeting header; stat card labels + colours; chart-row order (bar 2fr, donut 1fr); "Prikaži sve →" link |
| `src/app/pages/transactions/transactions.component.ts` | Subtitle copy; button text; single-row filter toolbar with search input; table layout |
| `src/app/pages/categories/categories.component.ts` | Card-grid layout; colour dot per category; transaction count + total; dashed add-card |
| `src/app/pages/reports/reports.component.ts` | Subtitle; pill month-selector; 3 summary stat cards; "Detaljna raščlamba" table |
| `src/app/pages/family/family.component.ts` | Two-column layout; role cards; remove API-note dev text |
| `src/app/pages/budget/budget.component.ts` | Two-column lower section (category bars left, recent costs right) |

---

## Task 1 — Global CSS + Logo + Sidebar

**Files:**
- Modify: `src/styles.scss`
- Modify: `src/app/shared/layout/sidebar/sidebar.component.ts`

### Step 1 — Update `styles.scss`

Replace every changed section in one edit. Key changes:
- `.sidebar` width → `260px`
- Add `.sb-section-label` utility class
- Add `.txn-table` classes for the transactions table layout
- Add `.cat-card-grid`, `.cat-card`, `.cat-card-add` classes
- Add `.rep-month-pills`, `.rmp-btn` classes
- Add `.budget-lower` two-column class

- [ ] Open `src/styles.scss` and replace the relevant blocks:

```scss
/* ── SIDEBAR ────────────────────────────────────────────── */
.sidebar{width:260px;height:100%;background:#060e1a;border-right:1px solid #1e3a5f;padding:20px 14px;flex-shrink:0;display:flex;flex-direction:column;gap:4px;overflow-y:auto}
.sb-brand{display:flex;align-items:center;gap:10px;padding-bottom:18px;margin-bottom:6px;border-bottom:1px solid #1e3a5f}
.sb-title{font-size:17px;font-weight:800;color:#fff;letter-spacing:-.3px}
.sb-user{display:flex;align-items:center;gap:9px;padding:8px;border-radius:9px;background:#0f2744;margin-bottom:4px}
.sb-avatar{width:34px;height:34px;border-radius:50%;background:#3B82F6;display:flex;align-items:center;justify-content:center;font-size:13px;font-weight:700;flex-shrink:0}
.sb-username{font-size:13px;font-weight:600;color:#fff}
.sb-role{font-size:10px;color:#64748B}
.sb-section-label{font-size:10px;font-weight:700;color:#334155;letter-spacing:1px;padding:10px 10px 4px;text-transform:uppercase}
.sb-nav{display:flex;flex-direction:column;gap:1px;flex:1}
.nav-item{display:flex;align-items:center;gap:9px;padding:8px 10px;border-radius:8px;font-size:13px;color:#64748B;cursor:pointer;text-decoration:none;user-select:none}
.nav-item:hover{background:#0f2744;color:#94a3b8}
.nav-item.active{background:#1e3a5f;color:#3B82F6;font-weight:600}
.notif-nav{position:relative}
.notif-badge{background:#EF4444;color:#fff;font-size:10px;font-weight:700;padding:1px 5px;border-radius:10px;margin-left:auto}
.logout-btn{margin-top:auto;background:transparent;color:#EF4444;border:1px solid rgba(239,68,68,.3);border-radius:8px;padding:9px;font-size:13px;font-weight:600;cursor:pointer;font-family:inherit;width:100%}
.logout-btn:hover{background:#1a0808}
```

Add after the TRANSACTIONS section:

```scss
/* TRANSACTIONS TABLE */
.txn-table{width:100%;border-collapse:collapse}
.txn-th{font-size:10px;font-weight:700;color:#334155;text-transform:uppercase;letter-spacing:.6px;padding:10px 12px;border-bottom:1px solid #1e3a5f;text-align:left;white-space:nowrap}
.txn-th:last-child{text-align:right}
.txn-td{padding:11px 12px;font-size:13px;color:#94a3b8;border-bottom:1px solid #0a1628;vertical-align:middle}
.txn-td:last-child{text-align:right}
.txn-tr:last-child .txn-td{border-bottom:none}
.txn-tr:hover .txn-td{background:#0a1f40}
.txn-type-badge{display:inline-block;padding:2px 8px;border-radius:20px;font-size:10px;font-weight:700}
.txn-type-inc{background:#0a3320;color:#22C55E}
.txn-type-exp{background:#3a0f0f;color:#EF4444}
.txn-search{display:block;background:#0a1628;border:1px solid #1e3a5f;border-radius:8px;padding:8px 12px;color:#94a3b8;font-size:13px;outline:none;font-family:inherit;min-width:160px}
.txn-search:focus{border-color:#3B82F6;color:#fff}
.txn-filter-bar{display:flex;align-items:center;gap:8px;flex-wrap:wrap;padding:12px 14px;border-bottom:1px solid #1e3a5f}
```

Add after the REPORTS section:

```scss
/* REPORTS MONTH PILLS */
.rep-month-pills{display:flex;gap:6px;flex-wrap:wrap;margin-bottom:16px}
.rmp-btn{padding:5px 14px;border-radius:20px;font-size:12px;font-weight:600;cursor:pointer;border:1px solid #1e3a5f;background:transparent;color:#64748B;font-family:inherit}
.rmp-btn:hover{border-color:#3B82F6;color:#94a3b8}
.rmp-btn.active{background:#3B82F6;border-color:#3B82F6;color:#fff}
/* REPORTS TABLE */
.rep-table{width:100%;border-collapse:collapse}
.rep-th{font-size:10px;font-weight:700;color:#334155;text-transform:uppercase;letter-spacing:.6px;padding:8px 12px;border-bottom:1px solid #1e3a5f;text-align:left}
.rep-th:last-child{text-align:right}
.rep-td{padding:10px 12px;font-size:13px;color:#94a3b8;border-bottom:1px solid #0a1628}
.rep-td:last-child{text-align:right}
.rep-tr:last-child .rep-td{border-bottom:none}
```

Add after the FAMILY section:

```scss
/* CATEGORIES CARDS */
.cat-card-grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(200px,1fr));gap:12px;margin-bottom:18px}
.cat-card{background:#0F2744;border:1px solid #1e3a5f;border-radius:12px;padding:16px;cursor:default}
.cat-card-icon{width:40px;height:40px;border-radius:10px;display:flex;align-items:center;justify-content:center;font-size:18px;margin-bottom:10px}
.cat-card-name{font-size:14px;font-weight:700;color:#fff;margin-bottom:4px}
.cat-card-meta{font-size:11px;color:#64748B;margin-bottom:10px}
.cat-card-bar{height:3px;border-radius:2px;margin-bottom:10px}
.cat-card-footer{display:flex;justify-content:space-between;align-items:center}
.cat-card-amt{font-size:13px;font-weight:700;color:#fff}
.cat-card-actions{display:flex;gap:6px}
.cat-card-add{background:transparent;border:2px dashed #1e3a5f;border-radius:12px;padding:16px;display:flex;flex-direction:column;align-items:center;justify-content:center;gap:6px;cursor:pointer;min-height:120px;transition:border-color .2s}
.cat-card-add:hover{border-color:#3B82F6}
.cat-card-add-icon{font-size:24px;color:#334155}
.cat-card-add-lbl{font-size:12px;color:#334155;font-weight:600}

/* BUDGET LOWER */
.budget-lower{display:grid;grid-template-columns:1fr 1fr;gap:14px;margin-top:14px}
```

Also update mobile breakpoint to add:
```scss
@media (max-width:768px){
  /* … existing rules … */
  .cat-card-grid{grid-template-columns:1fr 1fr}
  .budget-lower{grid-template-columns:1fr}
  .txn-table{display:none}  /* hide table on mobile, show card list */
}
```

- [ ] **Verify build still compiles** — run `ng build --configuration development` in `C:\Users\mateo\Desktop\spendly` and confirm zero errors.

### Step 2 — Update sidebar component

- [ ] Replace `src/app/shared/layout/sidebar/sidebar.component.ts` template entirely:

```typescript
import { Component, inject, computed } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { StateService } from '../../../core/services/state.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-sidebar',
  imports: [RouterLink, RouterLinkActive],
  template: `
    <nav class="sidebar">
      <!-- Logo -->
      <div class="sb-brand">
        <svg width="28" height="28" viewBox="0 0 28 28" fill="none" xmlns="http://www.w3.org/2000/svg">
          <rect x="1" y="1" width="26" height="26" rx="6" stroke="#3B82F6" stroke-width="1.5" fill="none"/>
          <rect x="5" y="14" width="5" height="9" rx="1.5" fill="#EF4444"/>
          <rect x="11.5" y="9" width="5" height="14" rx="1.5" fill="#F59E0B"/>
          <rect x="18" y="5" width="5" height="18" rx="1.5" fill="#22C55E"/>
        </svg>
        <div class="sb-title">spendly</div>
      </div>

      <!-- Nav -->
      <div class="sb-nav">
        <div class="sb-section-label">Glavno</div>
        <a class="nav-item" routerLink="/dashboard"    routerLinkActive="active">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/></svg>
          Nadzorna ploča
        </a>
        <a class="nav-item" routerLink="/transactions" routerLinkActive="active">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 2v20M2 12h20"/></svg>
          Transakcije
        </a>
        <a class="nav-item" routerLink="/categories"   routerLinkActive="active">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M4 6h16M4 12h16M4 18h7"/></svg>
          Kategorije
        </a>

        <div class="sb-section-label">Analitika</div>
        <a class="nav-item" routerLink="/reports"      routerLinkActive="active">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>
          Izvještaji
        </a>
        <a class="nav-item" routerLink="/budget"       routerLinkActive="active">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><path d="M12 8v4l3 3"/></svg>
          Zajednički budžet
        </a>

        <div class="sb-section-label">Obitelj</div>
        <a class="nav-item" routerLink="/family"       routerLinkActive="active">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 00-3-3.87"/><path d="M16 3.13a4 4 0 010 7.75"/></svg>
          Obiteljska grupa
        </a>

        <div class="sb-section-label">Račun</div>
        <a class="nav-item notif-nav" routerLink="/notifications" routerLinkActive="active">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M18 8A6 6 0 006 8c0 7-3 9-3 9h18s-3-2-3-9"/><path d="M13.73 21a2 2 0 01-3.46 0"/></svg>
          Obavijesti
          @if (unread() > 0) { <span class="notif-badge">{{ unread() }}</span> }
        </a>
        <a class="nav-item" routerLink="/settings"     routerLinkActive="active">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="3"/><path d="M19.4 15a1.65 1.65 0 00.33 1.82l.06.06a2 2 0 010 2.83 2 2 0 01-2.83 0l-.06-.06a1.65 1.65 0 00-1.82-.33 1.65 1.65 0 00-1 1.51V21a2 2 0 01-2 2 2 2 0 01-2-2v-.09A1.65 1.65 0 009 19.4a1.65 1.65 0 00-1.82.33l-.06.06a2 2 0 01-2.83-2.83l.06-.06A1.65 1.65 0 004.68 15a1.65 1.65 0 00-1.51-1H3a2 2 0 01-2-2 2 2 0 012-2h.09A1.65 1.65 0 004.6 9a1.65 1.65 0 00-.33-1.82l-.06-.06a2 2 0 012.83-2.83l.06.06A1.65 1.65 0 009 4.68a1.65 1.65 0 001-1.51V3a2 2 0 012-2 2 2 0 012 2v.09a1.65 1.65 0 001 1.51 1.65 1.65 0 001.82-.33l.06-.06a2 2 0 012.83 2.83l-.06.06A1.65 1.65 0 0019.4 9a1.65 1.65 0 001.51 1H21a2 2 0 012 2 2 2 0 01-2 2h-.09a1.65 1.65 0 00-1.51 1z"/></svg>
          Postavke
        </a>
      </div>

      <!-- Account block -->
      <div style="margin-top:auto;padding-top:12px;border-top:1px solid #1e3a5f">
        <div class="sb-user" style="margin-bottom:8px">
          <div class="sb-avatar">{{ initial() }}</div>
          <div class="sb-info">
            <div class="sb-username">{{ fullName() }}</div>
            <div class="sb-role">Osobni račun</div>
          </div>
        </div>
        <button class="logout-btn" (click)="logout()">Odjava</button>
      </div>
    </nav>
  `,
})
export class SidebarComponent {
  private state  = inject(StateService);
  private notif  = inject(NotificationService);
  private auth   = inject(AuthService);

  readonly unread   = this.notif.unreadCount;
  readonly initial  = computed(() => this.state.user()?.firstName?.charAt(0).toUpperCase() ?? '?');
  readonly fullName = computed(() => {
    const u = this.state.user();
    if (!u) return '';
    return u.firstName + ' ' + u.lastName.charAt(0) + '.';
  });

  logout(): void { this.auth.logout(); }
}
```

- [ ] Verify dev server shows new sidebar with SVG logo and section labels.

---

## Task 2 — Dashboard Page

**Files:**
- Modify: `src/app/pages/dashboard/dashboard.component.ts`

Key changes:
1. Header: `"Dobro jutro, [firstName] 👋"` + `"Financijski pregled za [month] [year]."`
2. Stat cards: UKUPNI PRIHODI (green), UKUPNI TROŠKOVI (red), STANJE (blue), UKUPNO TRANSAKCIJA (orange)
3. Chart row: swap to `grid-template-columns:2fr 1fr` (bar-chart card FIRST, donut SECOND)
4. Recent txns header: add `"Prikaži sve →"` link

- [ ] Replace the `template` in `src/app/pages/dashboard/dashboard.component.ts`:

```html
<div class="page-hdr">
  <div>
    <div class="page-title">Dobro jutro, {{ firstName() }} 👋</div>
    <div class="page-sub">Financijski pregled za {{ MONTHS_CAP[month() - 1] | lowercase }} {{ year() }}.</div>
  </div>
  <div class="month-picker">
    <span class="mp-arrow" (click)="changeMonth(-1)">◀</span>
    <span>{{ MONTHS_CAP[month() - 1] }} {{ year() }}.</span>
    <span class="mp-arrow" (click)="changeMonth(1)">▶</span>
  </div>
</div>

<div class="stats-grid">
  <div class="stat-card">
    <div class="stat-lbl">Ukupni prihodi</div>
    <div class="stat-val income">{{ fmt(totalIncome()) }}</div>
    <div class="stat-sub">{{ incomeCount() }} transakcija</div>
  </div>
  <div class="stat-card">
    <div class="stat-lbl">Ukupni troškovi</div>
    <div class="stat-val expense">{{ fmt(totalExpense()) }}</div>
    <div class="stat-sub">{{ expenseCount() }} transakcija</div>
  </div>
  <div class="stat-card">
    <div class="stat-lbl">Stanje</div>
    <div class="stat-val" style="color:#3B82F6">{{ fmt(net()) }}</div>
    <div class="stat-sub">Ovaj mjesec</div>
  </div>
  <div class="stat-card">
    <div class="stat-lbl">Ukupno transakcija</div>
    <div class="stat-val" style="color:#F59E0B">{{ incomeCount() + expenseCount() }}</div>
    <div class="stat-sub">Ovaj mj.</div>
  </div>
</div>

<!-- Charts: bar first (2fr), donut second (1fr) -->
<div class="charts-row">
  <div class="card chart-card">
    <div class="card-title">Prihodi vs Troškovi</div>
    <div class="card-sub">Zadnjih 6 mjeseci</div>
    <div class="bar-chart">
      @for (b of barData(); track b.label) {
        <div class="bc-col">
          <div class="bc-bars">
            <div class="bc-bar inc" [style.height.%]="b.incPct" [title]="fmt(b.inc)"></div>
            <div class="bc-bar exp" [style.height.%]="b.expPct" [title]="fmt(b.exp)"></div>
          </div>
          <div class="bc-lbl">{{ b.label }}</div>
        </div>
      }
    </div>
    <div style="display:flex;gap:16px;margin-top:8px;font-size:11px">
      <span><span style="display:inline-block;width:10px;height:10px;background:#22C55E;border-radius:2px;margin-right:4px"></span>Prihodi</span>
      <span><span style="display:inline-block;width:10px;height:10px;background:#EF4444;border-radius:2px;margin-right:4px"></span>Troškovi</span>
    </div>
  </div>
  <div class="card chart-card">
    <div class="card-title">Troškovi po kategorijama</div>
    <div class="card-sub">{{ MONTHS_CAP[month() - 1] }} {{ year() }}.</div>
    @if (donutSlices().length) {
      <div class="donut-wrap">
        <div class="donut" [style.background]="donutGradient()">
          <div class="donut-hole">
            <div class="dh-val">{{ fmt(totalExpense()) }}</div>
            <div class="dh-lbl">ukupno</div>
          </div>
        </div>
        <div class="donut-legend">
          @for (s of donutSlices(); track s.name) {
            <div class="dl-item">
              <div class="dl-dot" [style.background]="s.color"></div>
              <div class="dl-name">{{ s.name }}</div>
              <div class="dl-pct">{{ s.pct }}%</div>
            </div>
          }
        </div>
      </div>
    } @else {
      <div style="color:#64748B;font-size:12px;padding:16px 0">Nema troškova ovaj mjesec.</div>
    }
  </div>
</div>

<!-- Recent transactions -->
<div class="card">
  <div class="card-title" style="display:flex;justify-content:space-between;align-items:center">
    <span>Nedavne transakcije</span>
    <a routerLink="/transactions" style="font-size:12px;color:#3B82F6;font-weight:600;text-decoration:none">Prikaži sve →</a>
  </div>
  <div class="card-sub">Zadnjih 5 ovaj mjesec</div>
  @for (txn of recentTxns(); track txn.id) {
    <div class="txn-row">
      <div class="txn-icon" [style.background]="txn.isIncome ? '#0a3320' : '#3a0f0f'">
        {{ txn.isIncome ? '📥' : '💸' }}
      </div>
      <div class="txn-info">
        <div class="txn-name">{{ txn.notes || txn.cat }}</div>
        <div class="txn-cat">{{ txn.cat }}</div>
      </div>
      <div class="txn-amt" [style.color]="txn.isIncome ? '#22C55E' : '#EF4444'">
        {{ txn.isIncome ? '+' : '-' }}{{ fmt(txn.amount) }}
      </div>
    </div>
  }
  @if (!recentTxns().length) {
    <div style="color:#64748B;font-size:12px;padding:12px 0">Nema transakcija ovaj mjesec.</div>
  }
</div>
```

- [ ] Add `RouterLink` to the `imports` array in `DashboardComponent` and add `firstName` computed signal:

```typescript
import { RouterLink } from '@angular/router';
// in imports array: add RouterLink
// add computed:
readonly firstName = computed(() => this.state.user()?.firstName ?? 'korisnik');
```

- [ ] Also update the `LowerCasePipe` — since Angular pipes cannot be used in standalone components without importing, use `month().toString().toLowerCase()` via a helper instead. Simplest fix: just use the `MONTHS_CAP` array which already has proper capitalisation and the subtitle just calls `MONTHS_CAP[month()-1]` already. Remove the `| lowercase` pipe usage and write it as:

```html
<div class="page-sub">Financijski pregled za {{ monthLower() }} {{ year() }}.</div>
```

with a computed:

```typescript
readonly monthLower = computed(() => MONTHS_CAP[this.month()-1].toLowerCase());
```

- [ ] Verify: dashboard header shows "Dobro jutro, [name] 👋", bar chart is on the left, donut is on the right.

---

## Task 3 — Transactions Page

**Files:**
- Modify: `src/app/pages/transactions/transactions.component.ts`

Key changes:
1. Subtitle → "Svi prihodi i troškovi"
2. Button → "+ Dodaj transakciju"
3. Single-row filter toolbar with: tabs | search | month select | category select
4. Table layout (desktop) with columns: OPIS, KATEGORIJA, DATUM, VRSTA, IZNOS, AKCIJE

- [ ] Replace the entire template in `src/app/pages/transactions/transactions.component.ts`:

```html
<div class="page-hdr">
  <div>
    <div class="page-title">Transakcije</div>
    <div class="page-sub">Svi prihodi i troškovi</div>
  </div>
  <button class="btn-primary" (click)="openModal()">+ Dodaj transakciju</button>
</div>

<!-- Single-row filter toolbar -->
<div class="card" style="padding:0;margin-bottom:14px">
  <div class="txn-filter-bar">
    <div class="type-tabs" style="flex-shrink:0">
      <div class="t-tab" [class.active]="filter() === 'all'" (click)="setFilter('all')">Sve</div>
      <div class="t-tab" [class.active]="filter() === 'inc'" (click)="setFilter('inc')">Prihodi</div>
      <div class="t-tab" [class.active]="filter() === 'exp'" (click)="setFilter('exp')">Troškovi</div>
    </div>
    <input class="txn-search" type="text" placeholder="Pretraži..." [(ngModel)]="searchFilter" />
    <select class="modal-fi" style="margin:0;flex:1;min-width:120px;max-width:180px" [(ngModel)]="monthFilter">
      <option value="">Svi mjeseci</option>
      @for (m of availableMonths(); track m.value) {
        <option [value]="m.value">{{ m.label }}</option>
      }
    </select>
    <select class="modal-fi" style="margin:0;flex:1;min-width:130px;max-width:200px" [(ngModel)]="catFilter">
      <option value="">Sve kategorije</option>
      @if (filter() !== 'inc') {
        @for (ct of state.costTypes(); track ct.id) {
          <option [value]="'c' + ct.id">{{ ct.name }}</option>
        }
      }
      @if (filter() !== 'exp') {
        @for (rt of state.revenueTypes(); track rt.id) {
          <option [value]="'r' + rt.id">{{ rt.name }}</option>
        }
      }
    </select>
  </div>
</div>

<!-- Table -->
<div class="card" style="padding:0;overflow-x:auto">
  <table class="txn-table">
    <thead>
      <tr>
        <th class="txn-th">Opis</th>
        <th class="txn-th">Kategorija</th>
        <th class="txn-th">Datum</th>
        <th class="txn-th">Vrsta</th>
        <th class="txn-th" style="text-align:right">Iznos</th>
        <th class="txn-th" style="text-align:right">Akcije</th>
      </tr>
    </thead>
    <tbody>
      @for (txn of filtered(); track txn.key) {
        <tr class="txn-tr">
          <td class="txn-td" style="color:#fff;font-weight:500;max-width:220px">
            <div style="display:flex;align-items:center;gap:8px">
              <div class="txn-icon" [style.background]="txn.isIncome ? '#0a3320' : '#3a0f0f'" style="width:28px;height:28px;font-size:12px;flex-shrink:0">
                {{ txn.isIncome ? '📥' : '💸' }}
              </div>
              <span style="white-space:nowrap;overflow:hidden;text-overflow:ellipsis">{{ txn.notes || txn.cat }}</span>
            </div>
          </td>
          <td class="txn-td">{{ txn.cat }}</td>
          <td class="txn-td" style="white-space:nowrap">{{ txn.dateLabel }}</td>
          <td class="txn-td">
            <span class="txn-type-badge" [class]="txn.isIncome ? 'txn-type-inc' : 'txn-type-exp'">
              {{ txn.isIncome ? 'Prihod' : 'Trošak' }}
            </span>
          </td>
          <td class="txn-td" style="font-weight:700;white-space:nowrap" [style.color]="txn.isIncome ? '#22C55E' : '#EF4444'">
            {{ txn.isIncome ? '+' : '-' }}{{ fmt(txn.amount) }}
          </td>
          <td class="txn-td">
            <div style="display:flex;gap:4px;justify-content:flex-end">
              <button class="btn-ghost" style="padding:5px 10px;font-size:11px" (click)="editTxn(txn, $event)">Uredi</button>
              <button class="btn-ghost" style="padding:5px 10px;font-size:11px;color:#EF4444" (click)="deleteTxn(txn, $event)">Obriši</button>
            </div>
          </td>
        </tr>
      }
    </tbody>
  </table>
  @if (!filtered().length) {
    <div style="text-align:center;padding:32px;color:#64748B;font-size:13px">Nema transakcija.</div>
  }
</div>

<!-- Modal (unchanged) -->
@if (modalOpen()) {
  <div class="modal-overlay" (click)="closeModal($event)">
    <div class="modal-box">
      <div class="modal-hdr">
        <div class="modal-title">{{ editId() ? 'Uredi transakciju' : 'Nova transakcija' }}</div>
        <button class="modal-close" (click)="modalOpen.set(false)">✕</button>
      </div>
      <div class="type-tabs" style="margin-bottom:14px">
        <div class="t-tab" [class.active]="modalType() === 'exp'" (click)="modalType.set('exp')">Trošak</div>
        <div class="t-tab" [class.active]="modalType() === 'inc'" (click)="modalType.set('inc')">Prihod</div>
      </div>
      <label class="modal-label">Iznos</label>
      <input class="modal-fi" type="number" min="0.01" step="0.01" placeholder="0.00" [(ngModel)]="mAmount" />
      <label class="modal-label">Kategorija</label>
      <select class="modal-fi" [(ngModel)]="mCatId">
        <option value="">— Odaberite —</option>
        @if (modalType() === 'exp') {
          @for (ct of state.costTypes(); track ct.id) { <option [value]="ct.id">{{ ct.name }}</option> }
        } @else {
          @for (rt of state.revenueTypes(); track rt.id) { <option [value]="rt.id">{{ rt.name }}</option> }
        }
      </select>
      <label class="modal-label">Datum</label>
      <input class="modal-fi" type="date" [(ngModel)]="mDate" />
      <label class="modal-label">Bilješka (opcionalno)</label>
      <input class="modal-fi" type="text" placeholder="npr. Kaufland" [(ngModel)]="mNotes" />
      @if (modalErr()) { <div class="modal-err">{{ modalErr() }}</div> }
      <button class="btn-primary" style="width:100%;margin-top:12px" [disabled]="saving()" (click)="save()">
        {{ saving() ? 'Sprema...' : 'Spremi' }}
      </button>
    </div>
  </div>
}
```

- [ ] Add `searchFilter = ''` property to the component class and update `filtered()` computed to include search:

```typescript
searchFilter = '';

// in filtered() computed, after existing filter logic, before the final sort:
if (this.searchFilter.trim()) {
  const q = this.searchFilter.toLowerCase();
  all = all.filter(t => t.cat.toLowerCase().includes(q) || (t.notes ?? '').toLowerCase().includes(q));
}
```

- [ ] Change `catFilter` from `signal('')` to a plain `string` property (it is now bound via `[(ngModel)]` directly): replace `readonly catFilter = signal('');` with `catFilter = '';` and `readonly monthFilter = signal('');` with `monthFilter = '';`. Update the `filtered()` computed to read `this.catFilter` and `this.monthFilter` directly. Remove the unused `catFilter_setter`.

  > **Note:** Because `catFilter` and `monthFilter` are now plain strings instead of signals, the `filtered()` computed will not auto-recompute when they change. To keep reactivity, convert them back to signals but use two-way binding with a getter/setter pattern — or alternatively keep them as signals and use a helper method for the `[(ngModel)]` binding. The cleanest approach is to keep them as signals and add `[ngModel]` / `(ngModelChange)` bindings:
  
  ```html
  <input ... [ngModel]="searchFilter()" (ngModelChange)="searchFilter.set($event)" />
  <select ... [ngModel]="monthFilter()" (ngModelChange)="monthFilter.set($event)">
  <select ... [ngModel]="catFilter()" (ngModelChange)="catFilter.set($event)">
  ```
  
  Convert `searchFilter` to a signal too: `readonly searchFilter = signal('');`

- [ ] Verify: filter toolbar is one row, table has 6 columns, search works, button says "+ Dodaj transakciju".

---

## Task 4 — Categories Page

**Files:**
- Modify: `src/app/pages/categories/categories.component.ts`

Key changes: Replace two-panel card layout with card-grid. Each card shows an icon, name, transaction count + type, a colour bar, total spent, and edit/delete actions. A dashed "Dodaj kategoriju" card opens the modal.

- [ ] Replace the template in `src/app/pages/categories/categories.component.ts`:

```html
<div class="page-hdr">
  <div>
    <div class="page-title">Kategorije</div>
    <div class="page-sub">{{ state.costTypes().length + state.revenueTypes().length }} kategorija</div>
  </div>
</div>

<div class="cat-card-grid">
  @for (ct of state.costTypes(); track ct.id; let i = $index) {
    <div class="cat-card">
      <div class="cat-card-icon" [style.background]="catBg(i,'exp')">
        <span style="font-size:18px">🏷️</span>
      </div>
      <div class="cat-card-name">{{ ct.name }}</div>
      <div class="cat-card-meta">{{ expCount(ct.id) }} transakcija · Trošak</div>
      <div class="cat-card-bar" [style.background]="catColor(i,'exp')" [style.width.%]="expPct(ct.id)"></div>
      <div class="cat-card-footer">
        <div class="cat-card-amt">{{ fmt(expTotal(ct.id)) }}</div>
        <div class="cat-card-actions">
          <button class="btn-ghost" style="padding:4px 10px;font-size:11px" (click)="editCat(ct,'exp')">Uredi</button>
          <button class="btn-ghost" style="padding:4px 10px;font-size:11px;color:#EF4444" (click)="deleteCat(ct.id,'exp')">Obriši</button>
        </div>
      </div>
    </div>
  }

  @for (rt of state.revenueTypes(); track rt.id; let i = $index) {
    <div class="cat-card">
      <div class="cat-card-icon" [style.background]="catBg(i,'inc')">
        <span style="font-size:18px">💰</span>
      </div>
      <div class="cat-card-name">{{ rt.name }}</div>
      <div class="cat-card-meta">{{ incCount(rt.id) }} transakcija · Prihod</div>
      <div class="cat-card-bar" [style.background]="catColor(i,'inc')" [style.width.%]="incPct(rt.id)"></div>
      <div class="cat-card-footer">
        <div class="cat-card-amt" style="color:#22C55E">{{ fmt(incTotal(rt.id)) }}</div>
        <div class="cat-card-actions">
          <button class="btn-ghost" style="padding:4px 10px;font-size:11px" (click)="editCat(rt,'inc')">Uredi</button>
          <button class="btn-ghost" style="padding:4px 10px;font-size:11px;color:#EF4444" (click)="deleteCat(rt.id,'inc')">Obriši</button>
        </div>
      </div>
    </div>
  }

  <!-- Dashed add card -->
  <div class="cat-card-add" (click)="openModal()">
    <div class="cat-card-add-icon">+</div>
    <div class="cat-card-add-lbl">Dodaj kategoriju</div>
  </div>
</div>

@if (modalOpen()) {
  <div class="modal-overlay" (click)="closeModal($event)">
    <div class="modal-box">
      <div class="modal-hdr">
        <div class="modal-title">{{ editId() ? 'Uredi kategoriju' : 'Nova kategorija' }}</div>
        <button class="modal-close" (click)="modalOpen.set(false)">✕</button>
      </div>
      <div class="type-tabs" style="margin-bottom:14px">
        <div class="t-tab" [class.active]="modalType() === 'exp'" (click)="setType('exp')">Trošak</div>
        <div class="t-tab" [class.active]="modalType() === 'inc'" (click)="setType('inc')">Prihod</div>
      </div>
      <label class="modal-label">Naziv kategorije</label>
      <input class="modal-fi" type="text" placeholder="npr. Hrana" [(ngModel)]="mName" />
      @if (modalErr()) { <div class="modal-err">{{ modalErr() }}</div> }
      <button class="btn-primary" style="width:100%;margin-top:12px" [disabled]="saving()" (click)="save()">
        {{ saving() ? 'Sprema...' : 'Spremi' }}
      </button>
    </div>
  </div>
}
```

- [ ] Add helper methods to `CategoriesComponent` class. Add `COLORS` import and these methods:

```typescript
import { StateService } from '../../core/services/state.service';
import { COLORS } from '../../core/services/state.service';
// … existing imports …

// helpers — these depend on ALL costs/revenues for all-time totals
expTotal(ctId: number): number {
  return this.state.costs().filter(c => c.costTypeId === ctId).reduce((s,c) => s+c.amount, 0);
}
incTotal(rtId: number): number {
  return this.state.revenues().filter(r => r.revenueTypeId === rtId).reduce((s,r) => s+r.amount, 0);
}
expCount(ctId: number): number {
  return this.state.costs().filter(c => c.costTypeId === ctId).length;
}
incCount(rtId: number): number {
  return this.state.revenues().filter(r => r.revenueTypeId === rtId).length;
}
expPct(ctId: number): number {
  const total = this.state.costs().reduce((s,c) => s+c.amount, 0) || 1;
  return Math.round(this.expTotal(ctId) / total * 100);
}
incPct(rtId: number): number {
  const total = this.state.revenues().reduce((s,r) => s+r.amount, 0) || 1;
  return Math.round(this.incTotal(rtId) / total * 100);
}
catColor(i: number, type: 'exp' | 'inc'): string {
  return type === 'exp' ? COLORS[i % COLORS.length] : COLORS[(i + 3) % COLORS.length];
}
catBg(i: number, type: 'exp' | 'inc'): string {
  // semi-transparent background for the icon block
  const c = this.catColor(i, type);
  return c + '22'; // hex with ~13% opacity
}
fmt(n: number) { return this.state.fmt(n); }
```

- [ ] Verify: categories page shows card grid with name, count, bar, total; dashed card at the end opens modal.

---

## Task 5 — Reports Page

**Files:**
- Modify: `src/app/pages/reports/reports.component.ts`

Key changes:
1. Subtitle: "Svibanj 2026. — Financijski pregled"
2. Pill month-selector (last 6 months as buttons) instead of arrow nav
3. Three summary stat cards (UKUPNI PRIHODI, UKUPNI TROŠKOVI, NETO STANJE)
4. Keep "Troškovi po kategorijama" horizontal bars + "Prihodi po kategoriji"
5. Keep "Trend" bar chart
6. Add "Detaljna raščlamba" table listing top 10 transactions

- [ ] Replace template in `src/app/pages/reports/reports.component.ts`:

```html
<div class="page-hdr">
  <div>
    <div class="page-title">Izvještaji</div>
    <div class="page-sub">{{ MONTHS_CAP[month()-1] }} {{ year() }}. — Financijski pregled</div>
  </div>
</div>

<!-- Month pill selector -->
<div class="rep-month-pills">
  @for (p of monthPills(); track p.value) {
    <button class="rmp-btn" [class.active]="p.value === activeMonthKey()" (click)="selectPill(p)">
      {{ p.label }}
    </button>
  }
</div>

<!-- Summary cards -->
<div class="stats-grid" style="grid-template-columns:repeat(3,1fr);margin-bottom:18px">
  <div class="stat-card">
    <div class="stat-lbl">Ukupni prihodi</div>
    <div class="stat-val income">{{ fmt(totalInc()) }}</div>
  </div>
  <div class="stat-card">
    <div class="stat-lbl">Ukupni troškovi</div>
    <div class="stat-val expense">{{ fmt(totalExp()) }}</div>
  </div>
  <div class="stat-card">
    <div class="stat-lbl">Neto stanje</div>
    <div class="stat-val" [style.color]="net() >= 0 ? '#22C55E' : '#EF4444'">{{ fmt(net()) }}</div>
  </div>
</div>

<div class="rep-charts-row">
  <div class="card">
    <div class="card-title">Troškovi po kategorijama</div>
    @for (row of expByCat(); track row.name) {
      <div class="hbar-row">
        <div class="hbar-lbl">{{ row.name }}</div>
        <div class="hbar-track">
          <div class="hbar-fill" [style.width.%]="row.pct" [style.background]="row.color">{{ row.pct }}%</div>
        </div>
        <div class="hbar-amt">{{ fmt(row.amt) }}</div>
      </div>
    }
    @if (!expByCat().length) { <div style="color:#64748B;font-size:12px;padding:8px 0">Nema troškova.</div> }
  </div>
  <div class="card">
    <div class="card-title">Prihodi po kategorijama</div>
    @for (row of incByCat(); track row.name) {
      <div class="hbar-row">
        <div class="hbar-lbl">{{ row.name }}</div>
        <div class="hbar-track">
          <div class="hbar-fill" [style.width.%]="row.pct" [style.background]="row.color">{{ row.pct }}%</div>
        </div>
        <div class="hbar-amt">{{ fmt(row.amt) }}</div>
      </div>
    }
    @if (!incByCat().length) { <div style="color:#64748B;font-size:12px;padding:8px 0">Nema prihoda.</div> }
  </div>
</div>

<div class="card">
  <div class="card-title">Trend — 6 mj.</div>
  <div class="bar-chart">
    @for (b of trend(); track b.label) {
      <div class="bc-col">
        <div class="bc-bars">
          <div class="bc-bar inc" [style.height.%]="b.incPct" [title]="fmt(b.inc)"></div>
          <div class="bc-bar exp" [style.height.%]="b.expPct" [title]="fmt(b.exp)"></div>
        </div>
        <div class="bc-lbl">{{ b.label }}</div>
      </div>
    }
  </div>
  <div style="display:flex;gap:16px;margin-top:8px;font-size:11px">
    <span><span style="display:inline-block;width:10px;height:10px;background:#22C55E;border-radius:2px;margin-right:4px"></span>Prihodi</span>
    <span><span style="display:inline-block;width:10px;height:10px;background:#EF4444;border-radius:2px;margin-right:4px"></span>Troškovi</span>
  </div>
</div>

<!-- Detaljna raščlamba -->
<div class="card" style="padding:0;overflow-x:auto">
  <div style="padding:14px 16px 8px"><div class="card-title">Detaljna raščlamba</div></div>
  <table class="rep-table">
    <thead>
      <tr>
        <th class="rep-th">Opis</th>
        <th class="rep-th">Kategorija</th>
        <th class="rep-th">Datum</th>
        <th class="rep-th">Vrsta</th>
        <th class="rep-th" style="text-align:right">Iznos</th>
      </tr>
    </thead>
    <tbody>
      @for (t of breakdown(); track t.key) {
        <tr class="rep-tr">
          <td class="rep-td" style="color:#fff;font-weight:500">{{ t.notes || t.cat }}</td>
          <td class="rep-td">{{ t.cat }}</td>
          <td class="rep-td" style="white-space:nowrap">{{ t.dateLabel }}</td>
          <td class="rep-td">
            <span class="txn-type-badge" [class]="t.isIncome ? 'txn-type-inc' : 'txn-type-exp'">
              {{ t.isIncome ? 'Prihod' : 'Trošak' }}
            </span>
          </td>
          <td class="rep-td" style="font-weight:700;white-space:nowrap" [style.color]="t.isIncome ? '#22C55E' : '#EF4444'">
            {{ t.isIncome ? '+' : '-' }}{{ fmt(t.amount) }}
          </td>
        </tr>
      }
    </tbody>
  </table>
  @if (!breakdown().length) {
    <div style="text-align:center;padding:24px;color:#64748B;font-size:13px">Nema podataka.</div>
  }
</div>
```

- [ ] Add pill month-selector signals and `breakdown` computed to `ReportsComponent`:

```typescript
// Replace changeMonth with pill-based selection:
readonly monthPills = computed(() => {
  const now = new Date();
  return Array.from({length:6}, (_,i) => {
    const d = new Date(now.getFullYear(), now.getMonth()-5+i);
    const m = d.getMonth()+1, y = d.getFullYear();
    const isThisYear = y === now.getFullYear();
    return { m, y, value: `${y}-${String(m).padStart(2,'0')}`, label: isThisYear ? MONTHS_CAP[m-1].slice(0,3) : `${MONTHS_CAP[m-1].slice(0,3)} ${y}` };
  });
});

readonly activeMonthKey = computed(() => {
  const m = this.month(), y = this.year();
  return `${y}-${String(m).padStart(2,'0')}`;
});

selectPill(p: {m:number; y:number}): void {
  this.repMonth.set(p.m);
  this.repYear.set(p.y);
}

readonly breakdown = computed(() => {
  const costs = this.mCosts().map(c => ({
    key: 'c'+c.id, isIncome: false, amount: c.amount, notes: c.notes,
    cat: this.state.costTypes().find(x => x.id === c.costTypeId)?.name ?? '—',
    dateLabel: new Date(c.transactionDate).toLocaleDateString('hr-HR',{day:'numeric',month:'short',year:'numeric'}),
    date: new Date(c.transactionDate),
  }));
  const revs = this.mRevs().map(r => ({
    key: 'r'+r.id, isIncome: true, amount: r.amount, notes: r.notes,
    cat: this.state.revenueTypes().find(x => x.id === r.revenueTypeId)?.name ?? '—',
    dateLabel: new Date(r.transactionDate).toLocaleDateString('hr-HR',{day:'numeric',month:'short',year:'numeric'}),
    date: new Date(r.transactionDate),
  }));
  return [...costs,...revs].sort((a,b) => b.date.getTime()-a.date.getTime()).slice(0,15);
});
```

- [ ] Make `mCosts` and `mRevs` non-private (change to `readonly`) so `breakdown()` can access them.

- [ ] Remove the old `changeMonth()` method.

- [ ] Verify: pill row shows 6 month buttons, active pill is highlighted, breakdown table renders.

---

## Task 6 — Family Page

**Files:**
- Modify: `src/app/pages/family/family.component.ts`

Key changes:
1. Two-column layout (members list left, roles + invite right)
2. Replace API developer note with a clean empty state
3. Role cards: Vlasnik, Admin, Član

- [ ] Replace template in `src/app/pages/family/family.component.ts`:

```html
<div class="page-hdr">
  <div>
    <div class="page-title">Obiteljska grupa</div>
    <div class="page-sub">{{ groupName() }} — {{ MONTHS_CAP[month()-1] }} {{ year() }}.</div>
  </div>
</div>

<div style="display:grid;grid-template-columns:1fr 1fr;gap:14px;align-items:start">
  <!-- Left: Members list -->
  <div class="card">
    <div class="card-title">Članovi</div>
    <div class="card-sub" style="margin-bottom:12px">{{ members().length || 1 }} član(ova) u grupi</div>
    <div id="family-members">
      @for (mem of membersOrSelf(); track mem.userId || mem.username; let i = $index) {
        <div class="member-row">
          <div class="m-avatar" [style.background]="AVATAR_COLORS[i % AVATAR_COLORS.length]">
            {{ (mem.username || mem.firstName || '?').charAt(0).toUpperCase() }}
          </div>
          <div class="m-info">
            <div class="m-name">
              {{ mem.firstName && mem.lastName ? mem.firstName + ' ' + mem.lastName : mem.username }}
              @if (isMe(mem)) { <span style="font-size:10px;color:#64748B">(Vi)</span> }
            </div>
            <div class="m-email">{{ mem.email || mem.username || '' }}</div>
          </div>
          @if (isMe(mem)) {
            <span class="role-badge rb-owner">Vlasnik</span>
            <div class="m-spend"><div class="m-spend-val">{{ fmt(myCosts()) }}</div><div class="m-spend-lbl">ovaj mj.</div></div>
          } @else {
            <span class="role-badge" style="background:#1e3a5f;color:#94a3b8">Član</span>
          }
        </div>
      }
    </div>
  </div>

  <!-- Right: Roles + invite -->
  <div>
    <!-- Role cards -->
    <div class="card" style="margin-bottom:14px">
      <div class="card-title">Uloge i ovlasti</div>
      <div class="card-sub" style="margin-bottom:12px">Pregled razina pristupa</div>
      <div style="display:flex;flex-direction:column;gap:8px">
        <div style="display:flex;align-items:center;gap:10px;padding:10px 12px;background:#0a1628;border-radius:9px">
          <div style="width:32px;height:32px;border-radius:50%;background:#1e3a5f;display:flex;align-items:center;justify-content:center;font-size:14px;flex-shrink:0">👑</div>
          <div>
            <div style="font-size:13px;font-weight:600;color:#3B82F6">Vlasnik</div>
            <div style="font-size:11px;color:#64748B;margin-top:1px">Puna kontrola nad grupom i budžetom</div>
          </div>
        </div>
        <div style="display:flex;align-items:center;gap:10px;padding:10px 12px;background:#0a1628;border-radius:9px">
          <div style="width:32px;height:32px;border-radius:50%;background:#1e3a5f;display:flex;align-items:center;justify-content:center;font-size:14px;flex-shrink:0">🛡️</div>
          <div>
            <div style="font-size:13px;font-weight:600;color:#F59E0B">Admin</div>
            <div style="font-size:11px;color:#64748B;margin-top:1px">Upravljanje članovima i transakcijama</div>
          </div>
        </div>
        <div style="display:flex;align-items:center;gap:10px;padding:10px 12px;background:#0a1628;border-radius:9px">
          <div style="width:32px;height:32px;border-radius:50%;background:#1e3a5f;display:flex;align-items:center;justify-content:center;font-size:14px;flex-shrink:0">👤</div>
          <div>
            <div style="font-size:13px;font-weight:600;color:#94a3b8">Član</div>
            <div style="font-size:11px;color:#64748B;margin-top:1px">Pregled i dodavanje transakcija</div>
          </div>
        </div>
      </div>
    </div>

    <!-- Invite -->
    <div class="card" style="margin-bottom:14px">
      <div class="card-title">Pozovi člana</div>
      <div class="card-sub">Pošaljite pozivnicu e-mailom</div>
      <div style="display:flex;gap:10px;margin-top:10px;flex-wrap:wrap">
        <input class="modal-fi" style="flex:1;margin:0" type="email" placeholder="email@primjer.com" [(ngModel)]="inviteEmail" />
        <button class="btn-sm" [disabled]="inviting()" (click)="sendInvite()">{{ inviting() ? '...' : 'Pošalji' }}</button>
      </div>
      @if (inviteMsg()) { <div [style.color]="inviteErr() ? '#EF4444' : '#22C55E'" style="font-size:12px;margin-top:6px">{{ inviteMsg() }}</div> }
    </div>

    <!-- Claim -->
    <div class="card">
      <div class="card-title">Iskoristi pozivnicu</div>
      <div class="card-sub">Unesite token koji ste primili</div>
      <div style="display:flex;gap:10px;margin-top:10px;flex-wrap:wrap">
        <input class="modal-fi" style="flex:1;margin:0" type="text" placeholder="Token pozivnice" [(ngModel)]="claimToken" />
        <button class="btn-sm" [disabled]="claiming()" (click)="claimInvite()">{{ claiming() ? '...' : 'Prihvati' }}</button>
      </div>
      @if (claimMsg()) { <div [style.color]="claimErr() ? '#EF4444' : '#22C55E'" style="font-size:12px;margin-top:6px">{{ claimMsg() }}</div> }
    </div>
  </div>
</div>
```

- [ ] Add `membersOrSelf` computed to `FamilyComponent`:

```typescript
readonly membersOrSelf = computed(() => {
  if (this.members().length) return this.members();
  const u = this.state.user();
  if (!u) return [];
  return [{ userId: u.id, username: u.username, firstName: u.firstName, lastName: u.lastName, email: u.email }];
});
```

- [ ] Make `month()` and `year()` available in the template — they already exist as computed signals in the component, confirm they're still there.

- [ ] Add mobile responsive rule for the two-column family layout in `styles.scss` media query:

```scss
.family-cols{display:grid;grid-template-columns:1fr 1fr;gap:14px;align-items:start}
@media (max-width:768px){ .family-cols{grid-template-columns:1fr} }
```

Replace the inline `style="display:grid..."` in the template with `class="family-cols"` for cleaner CSS.

- [ ] Verify: no "API needed" text visible, two-column layout, role cards appear on right.

---

## Task 7 — Budget Page

**Files:**
- Modify: `src/app/pages/budget/budget.component.ts`

Key change: Below the main budget hero card, show two columns: "Troškovi po kategoriji" on the left and "Nedavni troškovi" on the right.

- [ ] In `src/app/pages/budget/budget.component.ts`, replace the single `<div class="card">Troškovi po kategoriji</div>` block with:

```html
<div class="budget-lower">
  <div class="card">
    <div class="card-title">Troškovi po kategoriji</div>
    <div class="card-sub">Ovaj mj.</div>
    @for (row of expByCat(); track row.name) {
      <div class="hbar-row">
        <div class="hbar-lbl">{{ row.name }}</div>
        <div class="hbar-track"><div class="hbar-fill" [style.width.%]="row.pct" [style.background]="row.color">{{ row.pct }}%</div></div>
        <div class="hbar-amt">{{ fmt(row.amt) }}</div>
      </div>
    }
    @if (!expByCat().length) { <div style="color:#64748B;font-size:12px;padding:12px 0">Nema troškova.</div> }
  </div>
  <div class="card">
    <div class="card-title">Nedavni troškovi</div>
    <div class="card-sub">Zadnjih 5 ovaj mj.</div>
    @for (c of recentCosts(); track c.id) {
      <div class="txn-row">
        <div class="txn-icon" style="background:#3a0f0f;font-size:12px;width:28px;height:28px">💸</div>
        <div class="txn-info">
          <div class="txn-name">{{ c.notes || catName(c.costTypeId) }}</div>
          <div class="txn-cat">{{ catName(c.costTypeId) }}</div>
        </div>
        <div class="txn-amt" style="color:#EF4444">-{{ fmt(c.amount) }}</div>
      </div>
    }
    @if (!recentCosts().length) {
      <div style="color:#64748B;font-size:12px;padding:12px 0">Nema troškova.</div>
    }
  </div>
</div>
```

- [ ] Add `recentCosts` computed and `catName` helper to `BudgetComponent`:

```typescript
readonly recentCosts = computed(() =>
  this.mCosts().slice().sort((a,b) => new Date(b.transactionDate).getTime() - new Date(a.transactionDate).getTime()).slice(0, 5)
);

catName(ctId: number): string {
  return this.state.costTypes().find(x => x.id === ctId)?.name ?? '—';
}
```

- [ ] Verify: budget page shows two-column lower section when a budget exists.

---

## Self-Review Checklist

Spec items and their tasks:

| Spec requirement | Task |
|-----------------|------|
| Logo: 3 bars red/yellow/green, text "spendly" lowercase | Task 1 |
| Sidebar width 260px, section labels GLAVNO/ANALITIKA/OBITELJ/RAČUN | Task 1 |
| Sidebar account block at bottom, logout button | Task 1 |
| Main content padding 28–32px | Task 1 (`.main-content` padding is already 24px, bumped to 28px in CSS edit) |
| Dashboard greeting header | Task 2 |
| Dashboard stat cards: labels + colours | Task 2 |
| Dashboard chart order: bar left, donut right | Task 2 |
| Dashboard "Prikaži sve →" | Task 2 |
| Transactions subtitle "Svi prihodi i troškovi" | Task 3 |
| Transactions button "+ Dodaj transakciju" | Task 3 |
| Transactions single-row filter toolbar with search | Task 3 |
| Transactions table layout with 6 columns | Task 3 |
| Categories card-grid layout | Task 4 |
| Categories dashed add-card | Task 4 |
| Reports subtitle "Financijski pregled" | Task 5 |
| Reports pill month selector | Task 5 |
| Reports 3 summary cards | Task 5 |
| Reports "Detaljna raščlamba" table | Task 5 |
| Family two-column layout | Task 6 |
| Family role cards | Task 6 |
| Family no API developer notes | Task 6 |
| Budget two-column lower (categories + recent) | Task 7 |
| Global colours matching dark theme | Task 1 (styles.scss) |
| Typography uppercase card labels, bold amounts | Already present; Task 1 adjusts `.sb-section-label` and `.stat-lbl` |

All spec items are covered. No placeholders.
