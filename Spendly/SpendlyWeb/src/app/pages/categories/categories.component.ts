import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { StateService, COLORS } from '../../core/services/state.service';
import { ApiService } from '../../core/services/api.service';
import { CostType, RevenueType } from '../../models';

@Component({
  selector: 'app-categories',
  imports: [FormsModule],
  template: `
    <div class="page-hdr">
      <div>
        <div class="page-title">Kategorije</div>
        <div class="page-sub">{{ state.costTypes().length + state.revenueTypes().length }} kategorija</div>
      </div>
    </div>

    <div class="cat-card-grid">
      @for (ct of state.costTypes(); track ct.id; let i = $index) {
        <div class="cat-card">
          <div class="cat-card-icon" [style.background]="catBg(i, 'exp')">💸</div>
          <div class="cat-card-name">{{ ct.name }}</div>
          <div class="cat-card-meta">{{ expCount(ct.id) }} transakcija · Trošak</div>
          <div class="cat-card-bar"
            [style.background]="catColor(i, 'exp')"
            [style.width.%]="expPct(ct.id)"></div>
          <div class="cat-card-footer">
            <div class="cat-card-amt">{{ fmt(expTotal(ct.id)) }}</div>
            <div class="cat-card-actions">
              <button class="btn-ghost" style="padding:4px 10px;font-size:11px"
                (click)="editCat(ct, 'exp')">Uredi</button>
              <button class="btn-ghost" style="padding:4px 10px;font-size:11px;color:#EF4444"
                (click)="deleteCat(ct.id, 'exp')">Obriši</button>
            </div>
          </div>
        </div>
      }

      @for (rt of state.revenueTypes(); track rt.id; let i = $index) {
        <div class="cat-card">
          <div class="cat-card-icon" [style.background]="catBg(i, 'inc')">💰</div>
          <div class="cat-card-name">{{ rt.name }}</div>
          <div class="cat-card-meta">{{ incCount(rt.id) }} transakcija · Prihod</div>
          <div class="cat-card-bar"
            [style.background]="catColor(i, 'inc')"
            [style.width.%]="incPct(rt.id)"></div>
          <div class="cat-card-footer">
            <div class="cat-card-amt" style="color:#22C55E">{{ fmt(incTotal(rt.id)) }}</div>
            <div class="cat-card-actions">
              <button class="btn-ghost" style="padding:4px 10px;font-size:11px"
                (click)="editCat(rt, 'inc')">Uredi</button>
              <button class="btn-ghost" style="padding:4px 10px;font-size:11px;color:#EF4444"
                (click)="deleteCat(rt.id, 'inc')">Obriši</button>
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
          <button class="btn-primary" style="width:100%;margin-top:12px"
            [disabled]="saving()" (click)="save()">
            {{ saving() ? 'Sprema...' : 'Spremi' }}
          </button>
        </div>
      </div>
    }
  `,
})
export class CategoriesComponent {
  readonly state = inject(StateService);
  private api    = inject(ApiService);

  readonly modalOpen = signal(false);
  readonly modalType = signal<'exp' | 'inc'>('exp');
  readonly editId    = signal<number | null>(null);
  readonly saving    = signal(false);
  readonly modalErr  = signal('');
  mName = '';

  // ── helpers ──────────────────────────────────────────────────────────────
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
    return this.catColor(i, type) + '33'; // 20% opacity
  }
  fmt(n: number) { return this.state.fmt(n); }

  // ── modal ─────────────────────────────────────────────────────────────────
  openModal() {
    this.editId.set(null); this.mName = ''; this.modalErr.set(''); this.modalOpen.set(true);
  }
  editCat(cat: CostType | RevenueType, type: 'exp' | 'inc') {
    this.editId.set(cat.id); this.modalType.set(type); this.mName = cat.name;
    this.modalErr.set(''); this.modalOpen.set(true);
  }
  setType(t: 'exp' | 'inc') { if (!this.editId()) this.modalType.set(t); }
  closeModal(e?: MouseEvent) {
    if (!e || (e.target as HTMLElement).classList.contains('modal-overlay')) this.modalOpen.set(false);
  }

  save() {
    const name = this.mName.trim();
    if (!name) { this.modalErr.set('Unesite naziv kategorije.'); return; }
    const ugId = this.state.personalUserGroupId();
    if (!ugId) { this.modalErr.set('Greška: nema grupe.'); return; }

    this.saving.set(true); this.modalErr.set('');
    const isExp  = this.modalType() === 'exp';
    const editId = this.editId();
    const req = editId
      ? (isExp ? this.api.put(`/CostType/UpdateCostType/${editId}`, { name, userGroupId: ugId })
               : this.api.put(`/RevenueType/UpdateRevenueType/${editId}`, { name, userGroupId: ugId }))
      : (isExp ? this.api.post('/CostType/CreateCostType', { name, userGroupId: ugId })
               : this.api.post('/RevenueType/CreateRevenueType', { name, userGroupId: ugId }));

    req.subscribe({
      next: () => {
        if (isExp) this.api.get<CostType[]>('/CostType/GetAllCostTypes').subscribe(r => { if (r) this.state.costTypes.set(r); });
        else       this.api.get<RevenueType[]>('/RevenueType/GetAllRevenueTypes').subscribe(r => { if (r) this.state.revenueTypes.set(r); });
        this.modalOpen.set(false); this.saving.set(false);
      },
      error: (e: Error) => { this.modalErr.set('Greška: ' + e.message); this.saving.set(false); },
    });
  }

  deleteCat(id: number, type: 'exp' | 'inc') {
    if (!confirm('Obrisati kategoriju?')) return;
    const req = type === 'exp'
      ? this.api.delete(`/CostType/DeleteCostType/${id}`)
      : this.api.delete(`/RevenueType/DeleteRevenueType/${id}`);
    req.subscribe({
      next: () => {
        if (type === 'exp') this.state.costTypes.update(list => list.filter(c => c.id !== id));
        else                this.state.revenueTypes.update(list => list.filter(r => r.id !== id));
      },
      error: (e: Error) => alert('Greška: ' + e.message),
    });
  }
}
