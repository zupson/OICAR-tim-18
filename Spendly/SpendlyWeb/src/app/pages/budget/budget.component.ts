import { Component, inject, computed, signal, effect } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { StateService, MONTHS_CAP, COLORS } from '../../core/services/state.service';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { Budget, Cost, ROLE_OWNER } from '../../models';

@Component({
  selector: 'app-budget',
  imports: [FormsModule],
  template: `
    <div class="page-hdr">
      <div>
        <div class="page-title">Zajednički budžet</div>
        <div class="page-sub">{{ groupName() }} · {{ MONTHS_CAP[month()-1] }} {{ year() }}.</div>
      </div>
      <div class="month-picker">
        <span class="mp-arrow" (click)="changeMonth(-1)">◀</span>
        <span>{{ MONTHS_CAP[month()-1] }} {{ year() }}.</span>
        <span class="mp-arrow" (click)="changeMonth(1)">▶</span>
      </div>
    </div>

    @if (!budget()) {
      <div style="text-align:center;padding:28px 16px">
        <div style="font-size:36px;margin-bottom:8px">💰</div>
        <div style="font-size:15px;font-weight:700;color:#fff;margin-bottom:4px">Nema budžeta za {{ MONTHS_CAP[month()-1] }} {{ year() }}.</div>
        <div style="font-size:12px;color:#64748B;margin-bottom:18px">
          @if (amIOwner()) { Postavite budžet za praćenje potrošnje }
          @else { Samo vlasnik grupe može postaviti budžet. }
        </div>
      </div>
      @if (amIOwner()) {
        <div class="card">
          <div class="card-title">Postavi budžet — {{ MONTHS_CAP[month()-1] }} {{ year() }}.</div>
          <div class="card-sub">Odredite limit potrošnje za ovaj mjesec</div>
          <div style="display:flex;gap:12px;align-items:flex-end;margin-top:12px;flex-wrap:wrap">
            <div style="flex:1;min-width:150px">
              <label class="modal-label">Iznos budžeta</label>
              <input class="modal-fi" type="number" min="1" step="0.01" placeholder="npr. 3000.00" [(ngModel)]="newAmount" />
            </div>
            <button class="btn-sm" [disabled]="saving()" (click)="createBudget()">{{ saving() ? '...' : 'Spremi budžet' }}</button>
          </div>
          @if (createMsg()) {
            <div [style.color]="createErr() ? '#EF4444' : '#22C55E'" style="margin-top:8px;font-size:12px">{{ createMsg() }}</div>
          }
        </div>
      }
    } @else {
      <!-- Budget hero card -->
      <div class="card">
        <div class="budget-hero">
          <div class="bh-top">
            <div>
              <div class="bh-limit">Miesečni budžet — {{ groupName() }}</div>
              <div class="bh-val">{{ fmt(limit()) }} <span style="font-size:13px;color:#64748B;font-weight:400">limit</span></div>
            </div>
            <div style="text-align:right">
              <div style="font-size:12px;color:#64748B;margin-bottom:3px">Dosad potrošeno</div>
              <div style="font-size:22px;font-weight:700" [style.color]="barColor()">{{ fmt(spent()) }}</div>
              <div style="font-size:11px;color:#64748B">{{ pct() }}% iskorišteno</div>
            </div>
          </div>
          <div class="prog-bg" style="height:11px">
            <div class="prog-fill"
              [style.width.%]="pct()"
              [style.background]="'linear-gradient(90deg,#22C55E,' + barColor() + ')'"></div>
          </div>
          <div style="display:flex;justify-content:space-between;margin-top:5px;font-size:10px;color:#64748B">
            <span>0</span>
            <span [style.color]="barColor()" style="font-weight:600">{{ fmt(spent()) }} potrošeno</span>
            <span>{{ fmt(limit()) }}</span>
          </div>
          <div class="bh-stats">
            <div class="bh-stat">
              <div class="bh-stat-lbl">Preostalo</div>
              <div class="bh-stat-val" [style.color]="remaining() >= 0 ? '#22C55E' : '#EF4444'">{{ fmt(remaining()) }}</div>
            </div>
            <div class="bh-stat">
              <div class="bh-stat-lbl">Dana do kraja</div>
              <div class="bh-stat-val" style="color:#3B82F6">{{ daysLeft() }}</div>
            </div>
            <div class="bh-stat">
              <div class="bh-stat-lbl">Dnevni prosjek</div>
              <div class="bh-stat-val">{{ fmt(dailyAvg()) }}</div>
            </div>
            <div class="bh-stat">
              <div class="bh-stat-lbl">Transakcija</div>
              <div class="bh-stat-val">{{ mCosts().length }}</div>
            </div>
          </div>
        </div>
        @if (amIOwner()) {
          <div style="display:flex;justify-content:flex-end;gap:8px;margin-top:6px">
            <button class="btn-ghost" (click)="showEdit.set(!showEdit())">✏️ Uredi</button>
            <button class="btn-ghost" (click)="deleteBudget()">🗑 Obriši</button>
          </div>
        } @else {
          <div style="margin-top:6px;font-size:11px;color:#64748B;text-align:right">Samo vlasnik može mijenjati budžet.</div>
        }
        @if (showEdit()) {
          <div class="card" style="margin-top:10px">
            <div class="card-title" style="margin-bottom:10px">Uredi budžet — {{ MONTHS_CAP[month()-1] }} {{ year() }}.</div>
            <div style="display:flex;gap:12px;align-items:flex-end;flex-wrap:wrap">
              <div style="flex:1;min-width:150px">
                <label class="modal-label">Novi iznos</label>
                <input class="modal-fi" type="number" min="1" step="0.01" [(ngModel)]="editAmount" />
              </div>
              <button class="btn-sm" [disabled]="saving()" (click)="updateBudget()">{{ saving() ? '...' : 'Spremi' }}</button>
              <button class="btn-ghost" (click)="showEdit.set(false)">Odustani</button>
            </div>
            @if (editMsg()) {
              <div [style.color]="editErr() ? '#EF4444' : '#22C55E'" style="margin-top:8px;font-size:12px">{{ editMsg() }}</div>
            }
          </div>
        }
      </div>

      <!-- Two-column lower section -->
      <div class="budget-lower">
        <div class="card">
          <div class="card-title">Troškovi po kategoriji</div>
          <div class="card-sub">Ovaj mj.</div>
          @for (row of expByCat(); track row.name) {
            <div class="hbar-row">
              <div class="hbar-lbl">{{ row.name }}</div>
              <div class="hbar-track">
                <div class="hbar-fill" [style.width.%]="row.pct" [style.background]="row.color">{{ row.pct }}%</div>
              </div>
              <div class="hbar-amt">{{ fmt(row.amt) }}</div>
            </div>
          }
          @if (!expByCat().length) {
            <div style="color:#64748B;font-size:12px;padding:12px 0">Nema troškova.</div>
          }
        </div>
        <div class="card">
          <div class="card-title">Nedavni troškovi</div>
          <div class="card-sub">Zadnjih 5 ovaj mj.</div>
          @for (c of recentCosts(); track c.id) {
            <div class="txn-row">
              <div class="txn-icon" style="background:#3a0f0f;width:28px;height:28px;font-size:12px">💸</div>
              <div class="txn-info">
                <div class="txn-name">{{ c.notes || catNameFor(c) }}</div>
                <div class="txn-cat">{{ catNameFor(c) }}</div>
              </div>
              <div class="txn-amt" style="color:#EF4444">-{{ fmt(c.amount) }}</div>
            </div>
          }
          @if (!recentCosts().length) {
            <div style="color:#64748B;font-size:12px;padding:12px 0">Nema troškova.</div>
          }
        </div>
      </div>
    }
  `,
})
export class BudgetComponent {
  readonly state = inject(StateService);
  private api    = inject(ApiService);
  private auth   = inject(AuthService);
  private notif  = inject(NotificationService);
  readonly MONTHS_CAP = MONTHS_CAP;

  readonly budMonth = signal<number | null>(null);
  readonly budYear  = signal<number | null>(null);
  readonly month    = computed(() => this.budMonth() ?? (new Date().getMonth() + 1));
  readonly year     = computed(() => this.budYear()  ?? new Date().getFullYear());
  readonly groupName = computed(() => this.state.userGroups()[0]?.groupName ?? 'Vaša grupa');

  readonly familyGroup = computed(() => {
    const personalGroupId = this.state.personalGroupId();
    return this.state.userGroups().find(g => g.groupId !== personalGroupId) ?? null;
  });
  readonly activeUserGroup = computed(() => this.familyGroup() ?? this.state.userGroups().find(g => g.groupId === this.state.personalGroupId()) ?? null);
  readonly amIOwner = computed(() => this.activeUserGroup()?.role === ROLE_OWNER);

  // Combined costs of ALL members of the active group, so the shared budget
  // reflects every member's spending — not just the current user's.
  private readonly groupCosts = signal<Cost[]>([]);
  constructor() {
    effect(() => {
      const gid = this.activeUserGroup()?.groupId ?? this.state.personalGroupId();
      if (!gid) { this.groupCosts.set([]); return; }
      this.api.get<Cost[]>(`/Cost/GetAllCostsByGroup?groupId=${gid}`).subscribe({
        next: c => this.groupCosts.set(c ?? []),
        error: () => this.groupCosts.set([]),
      });
    });
  }

  readonly budget = computed(() => {
    const gid = this.activeUserGroup()?.groupId ?? this.state.personalGroupId();
    return this.state.budgets().find(b => b.groupId === gid && b.month === this.month() && b.year === this.year());
  });

  readonly mCosts = computed(() => {
    const m = this.month(), y = this.year();
    return this.groupCosts().filter(c => { const d = new Date(c.transactionDate); return d.getMonth()+1===m && d.getFullYear()===y; });
  });

  readonly limit     = computed(() => Number(this.budget()?.amount ?? 0));
  readonly spent     = computed(() => this.mCosts().reduce((s, c) => s + c.amount, 0));
  readonly remaining = computed(() => this.limit() - this.spent());
  readonly pct       = computed(() => Math.min(100, Math.round(this.limit() > 0 ? this.spent()/this.limit()*100 : 0)));
  readonly barColor  = computed(() => this.pct() < 60 ? '#22C55E' : this.pct() < 85 ? '#F59E0B' : '#EF4444');
  readonly daysLeft  = computed(() => {
    const m = this.month(), y = this.year(), now = new Date();
    const daysIn = new Date(y, m, 0).getDate();
    const today  = (y === now.getFullYear() && m === now.getMonth()+1) ? now.getDate() : daysIn;
    return daysIn - today;
  });
  readonly dailyAvg = computed(() => {
    const now = new Date();
    const today = (this.year() === now.getFullYear() && this.month() === now.getMonth()+1)
      ? now.getDate()
      : new Date(this.year(), this.month(), 0).getDate();
    return today > 0 ? this.spent() / today : 0;
  });

  readonly expByCat = computed(() => {
    const byName: Record<string,number> = {};
    this.mCosts().forEach(c => { const n = this.catNameFor(c); byName[n] = (byName[n] ?? 0) + c.amount; });
    const total = this.spent() || 1;
    return Object.entries(byName)
      .map(([name, amt], i) => ({ name, amt, pct: Math.max(8, Math.round(amt/total*100)), color: COLORS[i%COLORS.length] }))
      .sort((a,b) => b.amt - a.amt).slice(0,5);
  });

  readonly recentCosts = computed(() =>
    this.mCosts()
      .slice()
      .sort((a,b) => new Date(b.transactionDate).getTime() - new Date(a.transactionDate).getTime())
      .slice(0, 5)
  );

  catNameFor(c: Cost): string {
    return c.costTypeName ?? this.state.costTypes().find(x => x.id === c.costTypeId)?.name ?? '—';
  }

  readonly showEdit  = signal(false);
  readonly saving    = signal(false);
  readonly createMsg = signal(''); readonly createErr = signal(false);
  readonly editMsg   = signal(''); readonly editErr   = signal(false);
  newAmount  = '';
  editAmount = '';

  fmt(n: number) { return this.state.fmt(n); }

  changeMonth(delta: number) {
    let m = this.month() + delta, y = this.year();
    if (m > 12) { m = 1; y++; }
    if (m < 1)  { m = 12; y--; }
    this.budMonth.set(m); this.budYear.set(y);
  }

  createBudget() {
    const amount = parseFloat(this.newAmount);
    if (!amount || amount <= 0) { this.createMsg.set('Unesite ispravan iznos.'); this.createErr.set(true); return; }
    const ugId = this.activeUserGroup()?.id ?? this.state.personalUserGroupId();
    if (!ugId) { this.createMsg.set('Greška: nema korisničke grupe.'); this.createErr.set(true); return; }
    this.saving.set(true);
    this.api.post<Budget>(`/Budget/CreateBudget/${ugId}`, { amount, year: this.year(), month: this.month(), currency: 0 }).subscribe({
      next: b => { if (b) this.state.budgets.update(list => [...list, b]); this.saving.set(false); this.newAmount = ''; this.notif.checkBudgetAlerts(); },
      error: (e: Error) => { this.createMsg.set('Greška: ' + this.auth.friendlyErr(e)); this.createErr.set(true); this.saving.set(false); },
    });
  }

  updateBudget() {
    const amount = parseFloat(this.editAmount);
    if (!amount || amount <= 0) { this.editMsg.set('Unesite ispravan iznos.'); this.editErr.set(true); return; }
    const id = this.budget()!.id;
    this.saving.set(true);
    this.api.put(`/Budget/UpdateBudget/${id}`, { amount, year: this.year(), month: this.month(), currency: 0 }).subscribe({
      next: () => {
        this.state.budgets.update(list => list.map(b => b.id === id ? { ...b, amount } : b));
        this.showEdit.set(false); this.saving.set(false);
        this.notif.prune(); this.notif.checkBudgetAlerts();
      },
      error: (e: Error) => { this.editMsg.set('Greška: ' + this.auth.friendlyErr(e)); this.editErr.set(true); this.saving.set(false); },
    });
  }

  deleteBudget() {
    if (!confirm('Obrisati budžet?')) return;
    const id = this.budget()!.id;
    this.api.delete(`/Budget/DeleteBudget/${id}`).subscribe({
      next: () => { this.state.budgets.update(list => list.filter(b => b.id !== id)); this.notif.prune(); },
      error: (e: Error) => alert('Greška: ' + this.auth.friendlyErr(e)),
    });
  }
}
