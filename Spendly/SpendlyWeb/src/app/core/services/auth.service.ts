import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { forkJoin, map, of, switchMap, tap } from 'rxjs';
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

  friendlyErr(e: Error): string {
    if (e.message?.toLowerCase().includes('failed to fetch') || e.message?.toLowerCase().includes('unknown error'))
      return 'Ne mogu se spojiti na server. Provjerite je li backend pokrenut.';
    try {
      const obj = JSON.parse(e.message);
      if (obj?.errors) return Object.values(obj.errors).flat().join(' ');
      if (typeof obj === 'string') return this.stripUrls(obj);
    } catch {}
    return this.stripUrls(e.message) || 'Nepoznata greška.';
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
        });
      }),
      tap(({ costs, revenues, costTypes, revenueTypes, budgets }) => {
        this.state.costs.set(costs ?? []);
        this.state.revenues.set(revenues ?? []);
        this.state.costTypes.set(costTypes ?? []);
        this.state.revenueTypes.set(revenueTypes ?? []);
        this.state.budgets.set(budgets ?? []);
        this.notif.checkBudgetAlerts();
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
