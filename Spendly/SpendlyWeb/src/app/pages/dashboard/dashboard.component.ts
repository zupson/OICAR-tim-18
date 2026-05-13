import { Component, inject, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { StateService, MONTHS_CAP, MONTHS_S, COLORS } from '../../core/services/state.service';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink],
  template: `
    <div class="page-hdr">
      <div>
        <div class="page-title">Dobro jutro, {{ firstName() }} 👋</div>
        <div class="page-sub">Financijski pregled za {{ monthLower() }} {{ year() }}.</div>
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

    <!-- Charts: bar chart (2fr) left, donut (1fr) right -->
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
      <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:3px">
        <div class="card-title" style="margin-bottom:0">Nedavne transakcije</div>
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
  `,
})
export class DashboardComponent {
  private state = inject(StateService);
  readonly MONTHS_CAP = MONTHS_CAP;

  readonly dashMonth = signal<number | null>(null);
  readonly dashYear  = signal<number | null>(null);

  readonly month = computed(() => this.dashMonth() ?? (new Date().getMonth() + 1));
  readonly year  = computed(() => this.dashYear()  ?? new Date().getFullYear());

  readonly firstName  = computed(() => this.state.user()?.firstName ?? 'korisnik');
  readonly monthLower = computed(() => MONTHS_CAP[this.month() - 1].toLowerCase());

  private mCosts = computed(() => {
    const m = this.month(), y = this.year();
    return this.state.costs().filter(c => {
      const d = new Date(c.transactionDate);
      return d.getMonth() + 1 === m && d.getFullYear() === y;
    });
  });
  private mRevenues = computed(() => {
    const m = this.month(), y = this.year();
    return this.state.revenues().filter(r => {
      const d = new Date(r.transactionDate);
      return d.getMonth() + 1 === m && d.getFullYear() === y;
    });
  });

  readonly totalExpense = computed(() => this.mCosts().reduce((s, c) => s + c.amount, 0));
  readonly totalIncome  = computed(() => this.mRevenues().reduce((s, r) => s + r.amount, 0));
  readonly net          = computed(() => this.totalIncome() - this.totalExpense());
  readonly expenseCount = computed(() => this.mCosts().length);
  readonly incomeCount  = computed(() => this.mRevenues().length);

  readonly donutSlices = computed(() => {
    const byType: Record<number, number> = {};
    this.mCosts().forEach(c => { byType[c.costTypeId] = (byType[c.costTypeId] ?? 0) + c.amount; });
    const total = this.totalExpense();
    return Object.entries(byType)
      .map(([id, amt], i) => ({
        name:  this.state.costTypes().find(x => x.id === +id)?.name ?? 'Ostalo',
        amt,
        pct:   total > 0 ? Math.round(amt / total * 100) : 0,
        color: COLORS[i % COLORS.length],
      }))
      .sort((a, b) => b.amt - a.amt)
      .slice(0, 5);
  });

  readonly donutGradient = computed(() => {
    const slices = this.donutSlices();
    if (!slices.length) return '#1e3a5f';
    let deg = 0;
    const stops = slices.map(s => {
      const start = deg;
      deg += s.pct * 3.6;
      return `${s.color} ${start}deg ${deg}deg`;
    });
    return `conic-gradient(${stops.join(', ')})`;
  });

  readonly barData = computed(() => {
    // Anchor the 6-month window on the currently selected month
    const months = Array.from({ length: 6 }, (_, i) => {
      const d = new Date(this.year(), this.month() - 1 - 5 + i);
      return { m: d.getMonth() + 1, y: d.getFullYear(), label: MONTHS_S[d.getMonth()] };
    });
    const vals = months.map(({ m, y, label }) => {
      const inc = this.state.revenues()
        .filter(r => { const d = new Date(r.transactionDate); return d.getMonth()+1===m && d.getFullYear()===y; })
        .reduce((s, r) => s + r.amount, 0);
      const exp = this.state.costs()
        .filter(c => { const d = new Date(c.transactionDate); return d.getMonth()+1===m && d.getFullYear()===y; })
        .reduce((s, c) => s + c.amount, 0);
      return { inc, exp, label };
    });
    const max = Math.max(...vals.map(v => Math.max(v.inc, v.exp)), 1);
    return vals.map(v => ({ ...v, incPct: Math.round(v.inc / max * 100), expPct: Math.round(v.exp / max * 100) }));
  });

  readonly recentTxns = computed(() => {
    const costs = this.mCosts().map(c => ({
      id: 'c' + c.id, amount: c.amount, notes: c.notes, isIncome: false,
      cat: this.state.costTypes().find(x => x.id === c.costTypeId)?.name ?? '—',
      date: new Date(c.transactionDate),
    }));
    const revs = this.mRevenues().map(r => ({
      id: 'r' + r.id, amount: r.amount, notes: r.notes, isIncome: true,
      cat: this.state.revenueTypes().find(x => x.id === r.revenueTypeId)?.name ?? '—',
      date: new Date(r.transactionDate),
    }));
    return [...costs, ...revs].sort((a, b) => b.date.getTime() - a.date.getTime()).slice(0, 5);
  });

  fmt(n: number) { return this.state.fmt(n); }

  changeMonth(delta: number): void {
    let m = this.month() + delta, y = this.year();
    if (m > 12) { m = 1; y++; }
    if (m < 1)  { m = 12; y--; }
    this.dashMonth.set(m);
    this.dashYear.set(y);
  }
}
