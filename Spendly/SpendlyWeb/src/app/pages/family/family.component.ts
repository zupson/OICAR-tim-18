import { Component, inject, computed, signal, effect } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { StateService, MONTHS_CAP } from '../../core/services/state.service';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { ROLE_OWNER } from '../../models';

const AVATAR_COLORS = ['#3B82F6','#22C55E','#F59E0B','#8B5CF6','#06B6D4','#EF4444','#F97316','#EC4899'];

interface Member {
  id?: number;
  userId?: number;
  username?: string;
  firstName?: string;
  lastName?: string;
  email?: string;
  role?: number;
}

@Component({
  selector: 'app-family',
  imports: [FormsModule],
  template: `
    <div class="page-hdr">
      <div>
        <div class="page-title">Obiteljska grupa</div>
        <div class="page-sub">
          @if (hasFamily()) {
            {{ groupName() }} — {{ MONTHS_CAP[month()-1] }} {{ year() }}.
          } @else {
            Pozovite člana ili iskoristite pozivnicu da započnete obiteljsku grupu.
          }
        </div>
      </div>
    </div>

    <div class="family-cols">
      <div class="card">
        <div class="card-title">{{ hasFamily() ? 'Članovi' : 'Samo vi' }}</div>
        <div class="card-sub">
          @if (hasFamily()) {
            {{ membersOrSelf().length }} član(ova) u grupi
          } @else {
            Niste još u obiteljskoj grupi
          }
        </div>
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
              @if (isOwnerMember(mem)) {
                <span class="role-badge rb-owner">Vlasnik</span>
              } @else {
                <span class="role-badge" style="background:#1e3a5f;color:#94a3b8">Član</span>
              }
              @if (isMe(mem)) {
                <div class="m-spend">
                  <div class="m-spend-val">{{ fmt(myCosts()) }}</div>
                  <div class="m-spend-lbl">ovaj mj.</div>
                </div>
              }
              @if (canRemove(mem)) {
                <button class="btn-ghost" style="padding:4px 10px;font-size:11px;color:#EF4444;margin-left:8px"
                  [disabled]="removing()" (click)="removeMember(mem)">Ukloni</button>
              }
            </div>
          }
        </div>
        @if (removeMsg()) {
          <div [style.color]="removeErr() ? '#EF4444' : '#22C55E'"
            style="font-size:12px;margin-top:8px">{{ removeMsg() }}</div>
        }
      </div>

      <div>
        <div class="card" style="margin-bottom:14px">
          <div class="card-title">Uloge i ovlasti</div>
          <div class="card-sub">Pregled razina pristupa</div>
          <div style="display:flex;flex-direction:column;gap:8px">
            <div style="display:flex;align-items:center;gap:10px;padding:10px 12px;background:#0a1628;border-radius:9px">
              <div style="width:32px;height:32px;border-radius:50%;background:#1e3a5f;display:flex;align-items:center;justify-content:center;font-size:15px;flex-shrink:0">👑</div>
              <div>
                <div style="font-size:13px;font-weight:600;color:#3B82F6">Vlasnik</div>
                <div style="font-size:11px;color:#64748B;margin-top:2px">Postavlja budžet i uklanja članove</div>
              </div>
            </div>
            <div style="display:flex;align-items:center;gap:10px;padding:10px 12px;background:#0a1628;border-radius:9px">
              <div style="width:32px;height:32px;border-radius:50%;background:#1e3a5f;display:flex;align-items:center;justify-content:center;font-size:15px;flex-shrink:0">👤</div>
              <div>
                <div style="font-size:13px;font-weight:600;color:#94a3b8">Član</div>
                <div style="font-size:11px;color:#64748B;margin-top:2px">Pregled i dodavanje vlastitih transakcija</div>
              </div>
            </div>
          </div>
        </div>

        <div class="card" style="margin-bottom:14px">
          <div class="card-title">Pozovi člana</div>
          <div class="card-sub">Pošaljite pozivnicu e-mailom</div>
          <div style="display:flex;gap:10px;margin-top:10px;flex-wrap:wrap">
            <input class="modal-fi" style="flex:1;margin:0" type="email"
              placeholder="email@primjer.com" [(ngModel)]="inviteEmail" />
            <button class="btn-sm" [disabled]="inviting()" (click)="sendInvite()">
              {{ inviting() ? '...' : 'Pošalji' }}
            </button>
          </div>
          @if (inviteMsg()) {
            <div [style.color]="inviteErr() ? '#EF4444' : '#22C55E'"
              style="font-size:12px;margin-top:6px">{{ inviteMsg() }}</div>
          }
        </div>

        <div class="card">
          <div class="card-title">Iskoristi pozivnicu</div>
          <div class="card-sub">Unesite token koji ste primili</div>
          <div style="display:flex;gap:10px;margin-top:10px;flex-wrap:wrap">
            <input class="modal-fi" style="flex:1;margin:0" type="text"
              placeholder="Token pozivnice" [(ngModel)]="claimToken" />
            <button class="btn-sm" [disabled]="claiming()" (click)="claimInvite()">
              {{ claiming() ? '...' : 'Prihvati' }}
            </button>
          </div>
          @if (claimMsg()) {
            <div [style.color]="claimErr() ? '#EF4444' : '#22C55E'"
              style="font-size:12px;margin-top:6px">{{ claimMsg() }}</div>
          }
        </div>
      </div>
    </div>
  `,
})
export class FamilyComponent {
  readonly state = inject(StateService);
  private api    = inject(ApiService);
  private auth   = inject(AuthService);
  private http   = inject(HttpClient);
  readonly AVATAR_COLORS = AVATAR_COLORS;
  readonly MONTHS_CAP    = MONTHS_CAP;

  readonly members  = signal<Member[]>([]);
  inviteEmail = '';
  claimToken  = '';
  readonly inviting  = signal(false);
  readonly claiming  = signal(false);
  readonly removing  = signal(false);
  readonly inviteMsg = signal('');
  readonly inviteErr = signal(false);
  readonly claimMsg  = signal('');
  readonly claimErr  = signal(false);
  readonly removeMsg = signal('');
  readonly removeErr = signal(false);

  readonly familyGroup = computed(() => {
    const personalGroupId = this.state.personalGroupId();
    return this.state.userGroups().find(g => g.groupId !== personalGroupId) ?? null;
  });
  readonly hasFamily = computed(() => this.familyGroup() !== null);
  readonly groupName = computed(() => this.familyGroup()?.groupName ?? 'Niste u obiteljskoj grupi');
  readonly month     = computed(() => new Date().getMonth() + 1);
  readonly year      = computed(() => new Date().getFullYear());

  readonly activeGroupId = computed(() => this.familyGroup()?.groupId ?? this.state.personalGroupId());

  readonly myMembership = computed(() => {
    const gid = this.activeGroupId();
    return this.state.userGroups().find(g => g.groupId === gid) ?? null;
  });
  readonly amIOwner = computed(() => this.myMembership()?.role === ROLE_OWNER);

  readonly myCosts = computed(() => {
    const m = this.month(), y = this.year();
    return this.state.costs()
      .filter(c => { const d = new Date(c.transactionDate); return d.getMonth()+1===m && d.getFullYear()===y; })
      .reduce((s,c) => s+c.amount, 0);
  });

  readonly membersOrSelf = computed(() => {
    if (this.members().length) return this.members();
    const u = this.state.user();
    if (!u) return [];
    return [{
      userId: u.id, username: u.username, firstName: u.firstName, lastName: u.lastName, email: u.email,
      role: this.myMembership()?.role,
    } as Member];
  });

  isMe(mem: Member): boolean {
    const u = this.state.user();
    return mem.userId === u?.id || mem.username === u?.username;
  }

  isOwnerMember(mem: Member): boolean {
    return mem.role === ROLE_OWNER;
  }

  canRemove(mem: Member): boolean {
    return this.hasFamily()
        && this.amIOwner()
        && !this.isMe(mem)
        && !this.isOwnerMember(mem);
  }

  fmt(n: number) { return this.state.fmt(n); }

  constructor() {
    effect(() => {
      const gid = this.activeGroupId();
      if (!gid) { this.members.set([]); return; }
      this.http.get<Member[]>(`/api/UserGroup/GetMemebersByGroup/${gid}`).subscribe({
        next: members => this.members.set(members ?? []),
        error: () => this.members.set([]),
      });
    });
  }

  sendInvite(): void {
    const email = this.inviteEmail.trim();
    if (!email) { this.inviteMsg.set('Unesite e-mail adresu.'); this.inviteErr.set(true); return; }
    const gid = this.activeGroupId();
    if (!gid)  { this.inviteMsg.set('Greška: nema grupe.'); this.inviteErr.set(true); return; }
    this.inviting.set(true);
    this.api.post(`/Invitation/CreateNewInvitation/${gid}`, { email }).subscribe({
      next: () => {
        this.inviteMsg.set('✓ Pozivnica je poslana na ' + email);
        this.inviteErr.set(false); this.inviteEmail = ''; this.inviting.set(false);
      },
      error: (e: Error) => {
        this.inviteMsg.set('Greška: ' + this.auth.friendlyErr(e));
        this.inviteErr.set(true); this.inviting.set(false);
      },
    });
  }

  claimInvite(): void {
    const token = this.claimToken.trim();
    if (!token) { this.claimMsg.set('Unesite token pozivnice.'); this.claimErr.set(true); return; }
    this.claiming.set(true);
    this.api.post<{groupName?: string}>(`/Invitation/ClaimInvitation?token=${encodeURIComponent(token)}`).subscribe({
      next: (ug) => {
        this.claimMsg.set('✓ Uspješno ste se pridružili grupi: ' + (ug?.groupName ?? ''));
        this.claimErr.set(false); this.claimToken = '';
        this.auth.loadUserData().subscribe(); this.claiming.set(false);
      },
      error: (e: Error) => {
        this.claimMsg.set('Greška: ' + this.auth.friendlyErr(e));
        this.claimErr.set(true); this.claiming.set(false);
      },
    });
  }

  removeMember(mem: Member): void {
    if (!mem.id) { this.removeMsg.set('Greška: nepoznat član.'); this.removeErr.set(true); return; }
    if (!confirm(`Ukloniti ${mem.username ?? 'člana'} iz grupe?`)) return;
    this.removing.set(true);
    this.api.delete(`/UserGroup/RemoveMember/${mem.id}`).subscribe({
      next: () => {
        this.members.update(list => list.filter(m => m.id !== mem.id));
        this.removeMsg.set('✓ Član je uklonjen.');
        this.removeErr.set(false); this.removing.set(false);
      },
      error: (e: Error) => {
        this.removeMsg.set('Greška: ' + this.auth.friendlyErr(e));
        this.removeErr.set(true); this.removing.set(false);
      },
    });
  }
}
