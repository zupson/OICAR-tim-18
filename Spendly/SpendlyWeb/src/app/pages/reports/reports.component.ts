import { Component, inject, computed, signal } from '@angular/core';
import { StateService, MONTHS_CAP, MONTHS_S, COLORS } from '../../core/services/state.service';

@Component({
  selector: 'app-reports',
  template: `
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

    <!-- 3 summary stat cards -->
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

    <!-- Trend bar chart -->
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
      <div style="padding:14px 16px 8px">
        <div class="card-title">Detaljna raščlamba</div>
        <div class="card-sub">Sve transakcije ovog mjeseca</div>
      </div>
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
              <td class="rep-td" style="font-weight:700;white-space:nowrap"
                [style.color]="t.isIncome ? '#22C55E' : '#EF4444'">
                {{ t.isIncome ? '+' : '-' }}{{ fmt(t.amount) }}
              </td>
            </tr>
          }
        </tbody>
      </table>
      @if (!breakdown().length) {
        <div style="text-align:center;padding:24px;color:#64748B;font-size:13px">Nema podataka za ovaj mjesec.</div>
      }
    </div>
  `,
})
export class ReportsComponent {
  private state = inject(StateService);
  readonly MONTHS_CAP = MONTHS_CAP;

  readonly repMonth = signal<number | null>(null);
  readonly repYear  = signal<number | null>(null);
  readonly month = computed(() => this.repMonth() ?? (new Date().getMonth() + 1));
  readonly year  = computed(() => this.repYear()  ?? new Date().getFullYear());

  readonly mCosts = computed(() => {
    const m = this.month(), y = this.year();
    return this.state.costs().filter(c => { const d = new Date(c.transactionDate); return d.getMonth()+1===m && d.getFullYear()===y; });
  });
  readonly mRevs = computed(() => {
    const m = this.month(), y = this.year();
    return this.state.revenues().filter(r => { const d = new Date(r.transactionDate); return d.getMonth()+1===m && d.getFullYear()===y; });
  });

  readonly totalExp = computed(() => this.mCosts().reduce((s, c) => s + c.amount, 0));
  readonly totalInc = computed(() => this.mRevs().reduce((s, r) => s + r.amount, 0));
  readonly net      = computed(() => this.totalInc() - this.totalExp());

  readonly expByCat = computed(() => {
    const byType: Record<number,number> = {};
    this.mCosts().forEach(c => { byType[c.costTypeId] = (byType[c.costTypeId] ?? 0) + c.amount; });
    const total = this.totalExp() || 1;
    return Object.entries(byType)
      .map(([id, amt], i) => ({ name: this.state.costTypes().find(x => x.id === +id)?.name ?? 'Ostalo', amt, pct: Math.max(4, Math.round(amt/total*100)), color: COLORS[i%COLORS.length] }))
      .sort((a,b) => b.amt - a.amt).slice(0, 6);
  });

  readonly incByCat = computed(() => {
    const byType: Record<number,number> = {};
    this.mRevs().forEach(r => { byType[r.revenueTypeId] = (byType[r.revenueTypeId] ?? 0) + r.amount; });
    const total = this.totalInc() || 1;
    return Object.entries(byType)
      .map(([id, amt], i) => ({ name: this.state.revenueTypes().find(x => x.id === +id)?.name ?? 'Ostalo', amt, pct: Math.max(4, Math.round(amt/total*100)), color: COLORS[i%COLORS.length] }))
      .sort((a,b) => b.amt - a.amt).slice(0, 6);
  });

  readonly trend = computed(() => {
    // Anchor the 6-month window on the currently selected month
    const months = Array.from({length:6}, (_,i) => { const d = new Date(this.year(), this.month()-1-5+i); return { m: d.getMonth()+1, y: d.getFullYear(), label: MONTHS_S[d.getMonth()] }; });
    const vals = months.map(({m,y,label}) => ({
      inc: this.state.revenues().filter(r => { const d = new Date(r.transactionDate); return d.getMonth()+1===m && d.getFullYear()===y; }).reduce((s,r) => s+r.amount, 0),
      exp: this.state.costs().filter(c => { const d = new Date(c.transactionDate); return d.getMonth()+1===m && d.getFullYear()===y; }).reduce((s,c) => s+c.amount, 0),
      label,
    }));
    const max = Math.max(...vals.map(v => Math.max(v.inc, v.exp)), 1);
    return vals.map(v => ({ ...v, incPct: Math.round(v.inc/max*100), expPct: Math.round(v.exp/max*100) }));
  });

  // ── Month pill selector ──────────────────────────────────────────────────
  readonly monthPills = computed(() => {
    const now = new Date();
    return Array.from({length:6}, (_,i) => {
      const d = new Date(now.getFullYear(), now.getMonth()-5+i);
      const m = d.getMonth()+1, y = d.getFullYear();
      const isThisYear = y === now.getFullYear();
      return {
        m, y,
        value: `${y}-${String(m).padStart(2,'0')}`,
        label: isThisYear ? MONTHS_CAP[m-1].slice(0,3) : `${MONTHS_CAP[m-1].slice(0,3)} ${y}`,
      };
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

  // ── Breakdown table ──────────────────────────────────────────────────────
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
    return [...costs,...revs].sort((a,b) => b.date.getTime()-a.date.getTime()).slice(0,20);
  });

  fmt(n: number) { return this.state.fmt(n); }
}
