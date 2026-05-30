import { Component, inject, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { StateService, MONTHS_CAP } from '../../core/services/state.service';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { Cost, Revenue } from '../../models';

type TxnFilter = 'all' | 'inc' | 'exp';

@Component({
  selector: 'app-transactions',
  imports: [FormsModule],
  template: `
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
        <input class="txn-search" type="text" placeholder="Pretraži..."
          [ngModel]="searchFilter()" (ngModelChange)="searchFilter.set($event)" />
        <select class="modal-fi" style="margin:0;flex:1;min-width:120px;max-width:180px"
          [ngModel]="monthFilter()" (ngModelChange)="monthFilter.set($event)">
          <option value="">Svi mjeseci</option>
          @for (m of availableMonths(); track m.value) {
            <option [value]="m.value">{{ m.label }}</option>
          }
        </select>
        <select class="modal-fi" style="margin:0;flex:1;min-width:130px;max-width:200px"
          [ngModel]="catFilter()" (ngModelChange)="catFilter.set($event)">
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
                  <div class="txn-icon"
                    [style.background]="txn.isIncome ? '#0a3320' : '#3a0f0f'"
                    style="width:28px;height:28px;font-size:12px;flex-shrink:0">
                    {{ txn.isIncome ? '📥' : '💸' }}
                  </div>
                  <span style="white-space:nowrap;overflow:hidden;text-overflow:ellipsis">
                    {{ txn.notes || txn.cat }}
                  </span>
                </div>
              </td>
              <td class="txn-td">{{ txn.cat }}</td>
              <td class="txn-td" style="white-space:nowrap">{{ txn.dateLabel }}</td>
              <td class="txn-td">
                <span class="txn-type-badge" [class]="txn.isIncome ? 'txn-type-inc' : 'txn-type-exp'">
                  {{ txn.isIncome ? 'Prihod' : 'Trošak' }}
                </span>
              </td>
              <td class="txn-td" style="font-weight:700;white-space:nowrap"
                [style.color]="txn.isIncome ? '#22C55E' : '#EF4444'">
                {{ txn.isIncome ? '+' : '-' }}{{ fmt(txn.amount) }}
              </td>
              <td class="txn-td">
                <div style="display:flex;gap:4px;justify-content:flex-end">
                  <button class="btn-ghost" style="padding:5px 10px;font-size:11px"
                    (click)="editTxn(txn, $event)">Uredi</button>
                  <button class="btn-ghost" style="padding:5px 10px;font-size:11px;color:#EF4444"
                    (click)="deleteTxn(txn, $event)">Obriši</button>
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

    <!-- Modal -->
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
          <button class="btn-primary" style="width:100%;margin-top:12px"
            [disabled]="saving()" (click)="save()">
            {{ saving() ? 'Sprema...' : 'Spremi' }}
          </button>
        </div>
      </div>
    }
  `,
})
export class TransactionsComponent {
  readonly state = inject(StateService);
  private api    = inject(ApiService);
  private auth   = inject(AuthService);
  private notif  = inject(NotificationService);

  readonly filter      = signal<TxnFilter>('all');
  readonly catFilter   = signal('');
  readonly monthFilter = signal('');
  readonly searchFilter = signal('');
  readonly modalOpen   = signal(false);
  readonly modalType   = signal<'exp' | 'inc'>('exp');
  readonly editId      = signal<number | null>(null);
  readonly saving      = signal(false);
  readonly modalErr    = signal('');

  mAmount = '';
  mCatId  = '';
  mDate   = new Date().toISOString().slice(0, 10);
  mNotes  = '';

  readonly availableMonths = computed(() => {
    const seen = new Set<string>();
    [...this.state.costs(), ...this.state.revenues()].forEach(t => {
      const d = new Date(t.transactionDate);
      seen.add(`${d.getFullYear()}-${String(d.getMonth()+1).padStart(2,'0')}`);
    });
    return [...seen].sort((a,b) => b.localeCompare(a)).map(v => {
      const [y, m] = v.split('-');
      return { value: v, label: `${MONTHS_CAP[+m-1]} ${y}.` };
    });
  });

  readonly filtered = computed(() => {
    const f  = this.filter();
    const cf = this.catFilter();
    const mf = this.monthFilter();
    const q  = this.searchFilter().toLowerCase().trim();

    const costs = this.state.costs().map(c => ({
      key: 'c' + c.id, id: c.id, isIncome: false, amount: c.amount, notes: c.notes,
      cat: this.state.costTypes().find(x => x.id === c.costTypeId)?.name ?? '—',
      catKey: 'c' + c.costTypeId,
      date: new Date(c.transactionDate),
      dateLabel: new Date(c.transactionDate).toLocaleDateString('hr-HR', { day:'numeric', month:'short', year:'numeric' }),
      monthKey: c.transactionDate.slice(0,7),
    }));
    const revs = this.state.revenues().map(r => ({
      key: 'r' + r.id, id: r.id, isIncome: true, amount: r.amount, notes: r.notes,
      cat: this.state.revenueTypes().find(x => x.id === r.revenueTypeId)?.name ?? '—',
      catKey: 'r' + r.revenueTypeId,
      date: new Date(r.transactionDate),
      dateLabel: new Date(r.transactionDate).toLocaleDateString('hr-HR', { day:'numeric', month:'short', year:'numeric' }),
      monthKey: r.transactionDate.slice(0,7),
    }));

    let all = [...costs, ...revs];
    if (f === 'exp') all = all.filter(t => !t.isIncome);
    if (f === 'inc') all = all.filter(t =>  t.isIncome);
    if (cf) all = all.filter(t => t.catKey === cf);
    if (mf) all = all.filter(t => t.monthKey === mf);
    if (q)  all = all.filter(t => t.cat.toLowerCase().includes(q) || (t.notes ?? '').toLowerCase().includes(q));
    return all.sort((a, b) => b.date.getTime() - a.date.getTime());
  });

  setFilter(f: TxnFilter) { this.filter.set(f); this.catFilter.set(''); }
  fmt(n: number) { return this.state.fmt(n); }

  openModal() {
    this.editId.set(null); this.modalType.set('exp');
    this.mAmount = ''; this.mCatId = ''; this.mDate = new Date().toISOString().slice(0,10); this.mNotes = '';
    this.modalErr.set(''); this.modalOpen.set(true);
  }

  editTxn(txn: ReturnType<typeof this.filtered>[0], e: Event) {
    e.stopPropagation();
    this.modalType.set(txn.isIncome ? 'inc' : 'exp');
    this.editId.set(txn.id);
    this.mAmount = String(txn.amount);
    this.mDate   = txn.date.toISOString().slice(0,10);
    this.mNotes  = txn.notes ?? '';
    this.modalErr.set(''); this.modalOpen.set(true);
    if (!txn.isIncome) {
      const cost = this.state.costs().find(c => c.id === txn.id);
      if (cost) this.mCatId = String(cost.costTypeId);
    } else {
      const rev = this.state.revenues().find(r => r.id === txn.id);
      if (rev) this.mCatId = String(rev.revenueTypeId);
    }
  }

  closeModal(e?: MouseEvent) {
    if (!e || (e.target as HTMLElement).classList.contains('modal-overlay')) this.modalOpen.set(false);
  }

  save() {
    const amount = parseFloat(this.mAmount);
    if (!amount || amount <= 0) { this.modalErr.set('Unesite ispravan iznos.'); return; }
    if (!this.mCatId)           { this.modalErr.set('Odaberite kategoriju.'); return; }
    if (!this.mDate)            { this.modalErr.set('Odaberite datum.'); return; }
    const groupId = this.state.personalGroupId();
    if (!groupId) { this.modalErr.set('Greška: nema grupe.'); return; }

    this.saving.set(true); this.modalErr.set('');
    const isExp = this.modalType() === 'exp';
    const body = isExp
      ? { amount, notes: this.mNotes, transactionDate: this.mDate, costTypeId: +this.mCatId, currency: 0 }
      : { amount, notes: this.mNotes, transactionDate: this.mDate, revenueTypeId: +this.mCatId, currency: 0 };

    const editId = this.editId();
    const req = editId
      ? (isExp ? this.api.put(`/Cost/EditCostType/${editId}`, body) : this.api.put(`/Revenue/EditRevenueType/${editId}`, body))
      : (isExp ? this.api.post(`/Cost/CreateNewCost/${groupId}`, body) : this.api.post(`/Revenue/CreateNewRevenue/${groupId}`, body));

    req.subscribe({
      next: () => {
        this.api.get<Cost[]>('/Cost/GetAllCosts').subscribe(c => { if (c) this.state.costs.set(c); });
        this.api.get<Revenue[]>('/Revenue/GetAllRevenues').subscribe(r => { if (r) this.state.revenues.set(r); });
        this.modalOpen.set(false); this.saving.set(false);
        this.notif.prune(); this.notif.checkBudgetAlerts();
      },
      error: (e: Error) => { this.modalErr.set('Greška: ' + this.auth.friendlyErr(e)); this.saving.set(false); },
    });
  }

  deleteTxn(txn: ReturnType<typeof this.filtered>[0], e: Event) {
    e.stopPropagation();
    if (!confirm('Obrisati ovu transakciju?')) return;
    const req = txn.isIncome
      ? this.api.delete(`/Revenue/DeleteRevenue/${txn.id}`)
      : this.api.delete(`/Cost/${txn.id}`);
    req.subscribe({
      next: () => {
        if (txn.isIncome) this.state.revenues.update(list => list.filter(r => r.id !== txn.id));
        else              this.state.costs.update(list => list.filter(c => c.id !== txn.id));
        this.notif.prune(); this.notif.checkBudgetAlerts();
      },
      error: (e: Error) => alert('Greška: ' + this.auth.friendlyErr(e)),
    });
  }
}
