import { Component, inject, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { StateService, CURRENCIES } from '../../core/services/state.service';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { NotifPrefs } from '../../models';

@Component({
  selector: 'app-settings',
  imports: [FormsModule],
  template: `
    <div class="page-hdr">
      <div><div class="page-title">Postavke</div><div class="page-sub">Upravljajte računom i preferencijama</div></div>
    </div>

    <div class="settings-wrap">
      <div class="s-nav">
        <div class="s-nav-item" [class.active]="tab() === 'profile'"   (click)="tab.set('profile')">👤 &nbsp;Profil</div>
        <div class="s-nav-item" [class.active]="tab() === 'security'"  (click)="tab.set('security')">🔒 &nbsp;Sigurnost</div>
        <div class="s-nav-item" [class.active]="tab() === 'notif'"     (click)="tab.set('notif')">🔔 &nbsp;Obavijesti</div>
        <div class="s-nav-item" [class.active]="tab() === 'currency'"  (click)="tab.set('currency')">💱 &nbsp;Valuta</div>
        <div class="s-nav-item danger" style="margin-top:12px" (click)="auth.logout()">🚪 &nbsp;Odjava</div>
      </div>

      <div class="s-content">
        <!-- Profile -->
        @if (tab() === 'profile') {
          <div id="s-profile">
            <div style="display:flex;align-items:center;gap:14px;margin-bottom:20px">
              <div class="settings-avatar">{{ initial() }}</div>
              <div>
                <div style="font-weight:700;font-size:15px">{{ fullName() }}</div>
                <div style="font-size:12px;color:#64748B">{{ state.user()?.email || state.user()?.username }}</div>
              </div>
            </div>
            <label class="modal-label">Ime</label>
            <input class="modal-fi" type="text" [(ngModel)]="pFirstName" />
            <label class="modal-label">Prezime</label>
            <input class="modal-fi" type="text" [(ngModel)]="pLastName" />
            <label class="modal-label">E-mail adresa</label>
            <input class="modal-fi" readonly style="opacity:.6" [value]="state.user()?.email || ''" />
            @if (profileMsg()) { <div [style.color]="profileErr() ? '#EF4444' : '#22C55E'" style="font-size:12px;margin-bottom:10px">{{ profileMsg() }}</div> }
            <button class="btn-sm" [disabled]="saving()" (click)="saveProfile()">{{ saving() ? 'Sprema...' : 'Spremi promjene' }}</button>
            <div style="font-size:11px;color:#64748B;margin-top:10px">Uređivanje profila još nije dostupno.</div>
          </div>
        }

        <!-- Security -->
        @if (tab() === 'security') {
          <div id="s-security">
            <div class="card-title" style="margin-bottom:14px">Promjena lozinke</div>
            <label class="modal-label">Trenutna lozinka</label>
            <input class="modal-fi" type="password" [(ngModel)]="currPw" />
            <label class="modal-label">Nova lozinka</label>
            <input class="modal-fi" type="password" [(ngModel)]="newPw" />
            <label class="modal-label">Potvrdi novu lozinku</label>
            <input class="modal-fi" type="password" [(ngModel)]="confirmPw" />
            @if (pwMsg()) { <div [style.color]="pwErr() ? '#EF4444' : '#22C55E'" style="font-size:12px;margin-bottom:10px">{{ pwMsg() }}</div> }
            <button class="btn-sm" [disabled]="saving()" (click)="changePassword()">{{ saving() ? 'Mijenja...' : 'Promijeni lozinku' }}</button>
          </div>
        }

        <!-- Notifications -->
        @if (tab() === 'notif') {
          <div id="s-notifpref">
            <div class="card-title" style="margin-bottom:14px">Postavke obavijesti</div>
            <div class="toggle-row">
              <div class="tgl-info">
                <div class="tgl-label">Upozorenja o limitu budžeta</div>
                <div class="tgl-desc">Obavijesti kada zajednički budžet dosegne 80% ili 100%</div>
              </div>
              <div class="toggle" [class.on]="prefs().budget_alert" [class.off]="!prefs().budget_alert"
                   [style.justifyContent]="prefs().budget_alert ? 'flex-end' : 'flex-start'"
                   (click)="togglePref('budget_alert')"><div class="toggle-thumb"></div></div>
            </div>
            <div class="toggle-row">
              <div class="tgl-info">
                <div class="tgl-label">Novi zajednički troškovi</div>
                <div class="tgl-desc">Obavijesti kada član obitelji doda zajednički trošak</div>
              </div>
              <div class="toggle" [class.on]="prefs().shared_costs" [class.off]="!prefs().shared_costs"
                   [style.justifyContent]="prefs().shared_costs ? 'flex-end' : 'flex-start'"
                   (click)="togglePref('shared_costs')"><div class="toggle-thumb"></div></div>
            </div>
            <div class="toggle-row">
              <div class="tgl-info">
                <div class="tgl-label">Evidencija aktivnosti članova</div>
                <div class="tgl-desc">Obavijesti o promjenama u obiteljskoj grupi</div>
              </div>
              <div class="toggle" [class.on]="prefs().member_activity" [class.off]="!prefs().member_activity"
                   [style.justifyContent]="prefs().member_activity ? 'flex-end' : 'flex-start'"
                   (click)="togglePref('member_activity')"><div class="toggle-thumb"></div></div>
            </div>
          </div>
        }

        <!-- Currency -->
        @if (tab() === 'currency') {
          <div id="s-currency">
            <div class="card-title" style="margin-bottom:14px">Valuta prikaza</div>
            <label class="modal-label">Odaberite valutu</label>
            <select class="modal-fi" [(ngModel)]="selectedCurrency">
              @for (c of CURRENCIES; track c.code) {
                <option [value]="c.code">{{ c.symbol }} {{ c.name }} ({{ c.code }})</option>
              }
            </select>
            @if (currencyMsg()) { <div style="color:#22C55E;font-size:12px;margin-top:6px">{{ currencyMsg() }}</div> }
            <button class="btn-sm" style="margin-top:10px" (click)="saveCurrency()">Spremi valutu</button>
          </div>
        }
      </div>
    </div>
  `,
})
export class SettingsComponent {
  readonly state = inject(StateService);
  private api    = inject(ApiService);
  readonly auth  = inject(AuthService);
  private notifSvc = inject(NotificationService);
  readonly CURRENCIES = CURRENCIES;

  readonly tab = signal<'profile' | 'security' | 'notif' | 'currency'>('profile');

  readonly initial  = computed(() => this.state.user()?.firstName?.charAt(0).toUpperCase() ?? '?');
  readonly fullName = computed(() => { const u = this.state.user(); return u ? u.firstName + ' ' + u.lastName : ''; });

  pFirstName = this.state.user()?.firstName ?? '';
  pLastName  = this.state.user()?.lastName  ?? '';
  currPw = ''; newPw = ''; confirmPw = '';
  selectedCurrency = this.state.currency().code;

  readonly saving     = signal(false);
  readonly profileMsg = signal(''); readonly profileErr = signal(false);
  readonly pwMsg      = signal(''); readonly pwErr      = signal(false);
  readonly currencyMsg = signal('');
  readonly prefs      = signal<NotifPrefs>(this.notifSvc.getPrefs());

  togglePref(key: keyof NotifPrefs): void {
    const updated = { ...this.prefs(), [key]: !this.prefs()[key] };
    this.prefs.set(updated);
    this.notifSvc.savePrefs(updated);
  }

  saveProfile(): void {
    const firstName = this.pFirstName.trim(), lastName = this.pLastName.trim();
    if (!firstName || !lastName) { this.profileMsg.set('Ime i prezime su obavezni.'); this.profileErr.set(true); return; }
    this.saving.set(true);
    this.api.put('/User/EditMyProfile', { firstName, lastName }).subscribe({
      next: () => {
        const u = this.state.user()!;
        this.state.user.set({ ...u, firstName, lastName });
        localStorage.setItem('spendly_user', JSON.stringify(this.state.user()));
        this.profileMsg.set('✓ Profil uspješno ažuriran!'); this.profileErr.set(false); this.saving.set(false);
      },
      error: (e: Error) => {
        const isNotFound = e.message.includes('Not Found') || e.message.includes('404') || e.message.includes('not found');
        this.profileMsg.set(isNotFound ? 'Uređivanje profila još nije dostupno.' : 'Greška: ' + this.auth.friendlyErr(e));
        this.profileErr.set(true); this.saving.set(false);
      },
    });
  }

  changePassword(): void {
    if (!this.currPw || !this.newPw || !this.confirmPw) { this.pwMsg.set('Sva polja su obavezna.'); this.pwErr.set(true); return; }
    if (this.newPw !== this.confirmPw) { this.pwMsg.set('Nove lozinke se ne podudaraju.'); this.pwErr.set(true); return; }
    if (this.newPw.length < 8) { this.pwMsg.set('Lozinka mora imati najmanje 8 znakova.'); this.pwErr.set(true); return; }
    this.saving.set(true);
    this.api.put('/User/ChangePassword', { currentPassword: this.currPw, password: this.newPw }).subscribe({
      next: () => { this.pwMsg.set('✓ Lozinka uspješno promijenjena!'); this.pwErr.set(false); this.currPw = this.newPw = this.confirmPw = ''; this.saving.set(false); },
      error: (e: Error) => { this.pwMsg.set('Greška: ' + this.auth.friendlyErr(e)); this.pwErr.set(true); this.saving.set(false); },
    });
  }

  saveCurrency(): void {
    const c = CURRENCIES.find(x => x.code === this.selectedCurrency) ?? CURRENCIES[0];
    this.state.setCurrency(c);
    this.currencyMsg.set('✓ Valuta postavljena na ' + c.symbol + ' ' + c.name);
  }
}
