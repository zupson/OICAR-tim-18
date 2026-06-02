import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { catchError, forkJoin, map, of, switchMap, tap } from 'rxjs';
import { StateService } from './state.service';
import { NotificationService } from './notification.service';
import { Budget, Cost, CostType, Revenue, RevenueType, User, UserGroup } from '../../models';

interface LoginResponse { token: string; user: User; }

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http   = inject(HttpClient);
  private state  = inject(StateService);
  private notif  = inject(NotificationService);
  private router = inject(Router);

  friendlyErr(e: any): string {
    // No connection / server unreachable
    if (e?.status === 0 || /failed to fetch|unknown error/i.test(String(e?.message ?? '')))
      return 'Ne mogu se spojiti na server. Provjerite je li backend pokrenut.';

    // Actual server payload: HttpErrorResponse exposes it on .error; when the
    // body isn't JSON, Angular wraps the text as { text: '...' }.
    const body = e?.error ?? this.tryParse(e?.message);

    // ASP.NET model-validation errors -> { errors: { Field: [messages] } }
    if (body && typeof body === 'object' && body.errors && typeof body.errors === 'object') {
      const msg = this.translateValidation(body.errors as Record<string, string[]>);
      if (msg) return msg;
    }

    // Plain string message from the server (clean controller message or "Wrong password")
    const raw =
      typeof body === 'string'           ? body :
      typeof e?.error === 'string'       ? e.error :
      typeof e?.error?.text === 'string' ? e.error.text :
      typeof e?.message === 'string'     ? e.message : '';

    const known = this.mapKnownMessage(raw);
    if (known) return known;

    // A clean, human-readable server message — show it as-is
    if (raw && !this.isTechnical(raw)) return this.stripUrls(raw);

    // Fall back on the HTTP status code
    switch (e?.status) {
      case 400: return 'Neispravan zahtjev. Provjerite unesene podatke.';
      case 401:
      case 403: return 'Pogrešno korisničko ime ili lozinka.';
      case 404: return 'Traženi podatak nije pronađen.';
      case 409: return 'Korisničko ime ili e-mail već postoji.';
      case 500: return 'Greška na poslužitelju. Pokušajte ponovno kasnije.';
    }
    return 'Nepoznata greška. Pokušajte ponovno.';
  }

  private tryParse(s: unknown): any {
    if (typeof s !== 'string') return null;
    try { return JSON.parse(s); } catch { return null; }
  }

  private isTechnical(msg: string): boolean {
    return /dbupdate|sqlexception|inner exception|an error occurred while saving|stack trace|system\.|http failure|xhr|cannot read|undefined/i.test(msg);
  }

  private mapKnownMessage(raw: string): string | null {
    const m = (raw || '').toLowerCase();
    if (!m) return null;
    if (m.includes('wrong password') || m.includes('wrong username') || m.includes('invalid credentials') || m.includes('unauthorized'))
      return 'Pogrešno korisničko ime ili lozinka.';
    if (m.includes('uq_user_email')) return 'Ova e-mail adresa je već registrirana.';
    if (m.includes('uq_user_username')) return 'Ovo korisničko ime je već zauzeto.';
    if (m.includes('duplicate') || m.includes('unique') || m.includes('cannot insert duplicate'))
      return 'Korisničko ime ili e-mail već postoji.';
    return null;
  }

  private translateValidation(errors: Record<string, string[]>): string {
    const labels: Record<string, string> = {
      email: 'E-mail', password: 'Lozinka', username: 'Korisničko ime',
      firstname: 'Ime', lastname: 'Prezime', currentpassword: 'Trenutna lozinka',
    };
    const out: string[] = [];
    for (const [field, arr] of Object.entries(errors)) {
      const key   = field.toLowerCase();
      const label = labels[key] ?? field;
      const text  = (Array.isArray(arr) ? arr.join(' ') : String(arr ?? '')).toLowerCase();
      if (key === 'email' || text.includes('e-mail') || text.includes('email'))
        out.push('Unesite ispravnu e-mail adresu (npr. ime@primjer.com).');
      else if (text.includes('minimum length') || text.includes('at least') || text.includes('minlength'))
        out.push(key === 'password' ? 'Lozinka mora imati najmanje 8 znakova.' : `${label} mora imati najmanje 3 znaka.`);
      else if (text.includes('required') || text.includes('empty'))
        out.push(`${label} je obavezno polje.`);
      else
        out.push(`${label}: neispravan unos.`);
    }
    return out.join(' ');
  }

  private stripUrls(msg: string): string {
    if (!msg) return msg;
    return msg
      .replace(/https?:\/\/\S+/gi, '')
      .replace(/\/api\/\S+/gi, '')
      .replace(/\s{2,}/g, ' ')
      .trim();
  }

  login(username: string, password: string) {
    return this.http.post<LoginResponse>('/api/User/LoginUser', { username, password }).pipe(
      tap(data => {
        this.state.token.set(data.token);
        this.state.user.set(data.user);
        localStorage.setItem('spendly_token', data.token);
        localStorage.setItem('spendly_user', JSON.stringify(data.user));
      })
    );
  }

  register(payload: { firstName: string; lastName: string; email: string; username: string; password: string }) {
    return this.http.post<LoginResponse>('/api/User/RegisterUser', payload).pipe(
      tap(data => {
        this.state.token.set(data.token);
        this.state.user.set(data.user);
        localStorage.setItem('spendly_token', data.token);
        localStorage.setItem('spendly_user', JSON.stringify(data.user));
      })
    );
  }

  loadUserData() {
    const uid = this.state.user()?.id;
    if (uid != null) this.notif.syncUser(uid);
    return this.http.get<UserGroup[]>('/api/UserGroup/GetAllUserGroups').pipe(
      // Sort by UserGroup.id ascending so groups[0] is reliably the auto-created
      // personal group (created at registration, lowest id).
      map(groups => (groups ?? []).slice().sort((a, b) => a.id - b.id)),
      tap(groups => {
        this.state.userGroups.set(groups);
        if (groups.length) {
          this.state.personalGroupId.set(groups[0].groupId);
          this.state.personalUserGroupId.set(groups[0].id);
        }
      }),
      switchMap(groups => {
        const groupId = groups[0]?.groupId;
        // Active group = the shared/family group if you're in one, otherwise your own
        // group (which may itself be shared if others joined it — the owner's case).
        const familyGroupId = groups.find(g => g.groupId !== groupId)?.groupId ?? groupId ?? null;
        return forkJoin({
          costs:        this.http.get<Cost[]>('/api/Cost/GetAllCosts'),
          revenues:     this.http.get<Revenue[]>('/api/Revenue/GetAllRevenues'),
          budgets:      this.http.get<Budget[]>('/api/Budget/GetAllBudgets'),
          costTypes:    groupId
            ? this.http.get<CostType[]>(`/api/CostType/GetAllCostTypesByGroup?groupId=${groupId}`)
            : of<CostType[]>([]),
          revenueTypes: groupId
            ? this.http.get<RevenueType[]>(`/api/RevenueType/GetAllRevenueTypesByGroup?groupId=${groupId}`)
            : of<RevenueType[]>([]),
          sharedCosts:  familyGroupId
            ? this.http.get<Cost[]>(`/api/Cost/GetAllCostsByGroup?groupId=${familyGroupId}`).pipe(catchError(() => of<Cost[] | null>(null)))
            : of<Cost[]>([]),
          members:      familyGroupId
            ? this.http.get<any[]>(`/api/UserGroup/GetMemebersByGroup/${familyGroupId}`).pipe(catchError(() => of<any[] | null>(null)))
            : of<any[]>([]),
        }).pipe(map(data => ({ ...data, familyGroupId })));
      }),
      tap(({ costs, revenues, costTypes, revenueTypes, budgets, sharedCosts, members, familyGroupId }) => {
        this.state.costs.set(costs ?? []);
        this.state.revenues.set(revenues ?? []);
        this.state.costTypes.set(costTypes ?? []);
        this.state.revenueTypes.set(revenueTypes ?? []);
        this.state.budgets.set(budgets ?? []);
        this.notif.checkBudgetAlerts();
        if (familyGroupId) {
          const myId = this.state.user()?.id ?? -1;
          // Skip (don't seed) when a fetch failed, so we don't flood later.
          if (sharedCosts) this.notif.checkSharedCosts(sharedCosts, members ?? [], myId);
          if (members)     this.notif.checkMemberActivity(members, myId);
        }
      }),
    );
  }

  tryAutoLogin(): boolean {
    const tok = localStorage.getItem('spendly_token');
    const usr = localStorage.getItem('spendly_user');
    if (!tok || !usr) return false;
    this.state.token.set(tok);
    this.state.user.set(JSON.parse(usr));
    return true;
  }

  logout(): void {
    this.state.clearAll();
    localStorage.removeItem('spendly_token');
    localStorage.removeItem('spendly_user');
    this.router.navigate(['/login']);
  }
}
