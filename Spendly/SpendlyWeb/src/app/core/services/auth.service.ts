import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { forkJoin, tap } from 'rxjs';
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
      return 'Ne mogu se spojiti na server. Provjerite je li backend pokrenut na http://localhost:5153.';
    try {
      const obj = JSON.parse(e.message);
      if (obj?.errors) return Object.values(obj.errors).flat().join(' ');
      if (typeof obj === 'string') return obj;
    } catch {}
    return e.message || 'Nepoznata greška.';
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
    return forkJoin({
      groups:       this.http.get<UserGroup[]>('/api/UserGroup/GetAllUserGroups'),
      costs:        this.http.get<Cost[]>('/api/Cost/GetAllCosts'),
      revenues:     this.http.get<Revenue[]>('/api/Revenue/GetAllRevenues'),
      costTypes:    this.http.get<CostType[]>('/api/CostType/GetAllCostTypes'),
      revenueTypes: this.http.get<RevenueType[]>('/api/RevenueType/GetAllRevenueTypes'),
      budgets:      this.http.get<Budget[]>('/api/Budget/GetAllBudgets'),
    }).pipe(
      tap(({ groups, costs, revenues, costTypes, revenueTypes, budgets }) => {
        this.state.userGroups.set(groups ?? []);
        this.state.costs.set(costs ?? []);
        this.state.revenues.set(revenues ?? []);
        this.state.costTypes.set(costTypes ?? []);
        this.state.revenueTypes.set(revenueTypes ?? []);
        this.state.budgets.set(budgets ?? []);
        if (groups?.length) {
          this.state.personalGroupId.set(groups[0].groupId);
          this.state.personalUserGroupId.set(groups[0].id);
        }
        this.notif.checkBudgetAlerts();
      })
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
