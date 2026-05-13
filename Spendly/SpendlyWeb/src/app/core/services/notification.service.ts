import { inject, Injectable, signal, computed } from '@angular/core';
import { Notification, NotifPrefs } from '../../models';
import { StateService } from './state.service';

const STORE_KEY      = 'spendly_notifications';
const PREFS_KEY      = 'spendly_notif_prefs';
const PREFS_DEFAULTS: NotifPrefs = { budget_alert: true, shared_costs: true, member_activity: false };

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private state = inject(StateService);

  readonly notifications = signal<Notification[]>(this.load());
  readonly unreadCount   = computed(() => this.notifications().filter(n => !n.read).length);

  private load(): Notification[] {
    try { return JSON.parse(localStorage.getItem(STORE_KEY) || '[]'); }
    catch { return []; }
  }

  private save(): void {
    localStorage.setItem(STORE_KEY, JSON.stringify(this.notifications()));
  }

  getPrefs(): NotifPrefs {
    try { return { ...PREFS_DEFAULTS, ...(JSON.parse(localStorage.getItem(PREFS_KEY) || 'null') ?? {}) }; }
    catch { return { ...PREFS_DEFAULTS }; }
  }

  savePrefs(prefs: NotifPrefs): void {
    localStorage.setItem(PREFS_KEY, JSON.stringify(prefs));
  }

  add(notif: Notification): void {
    if (this.notifications().find(n => n.id === notif.id)) return;
    this.notifications.update(list => [notif, ...list]);
    this.save();
  }

  markRead(id: string): void {
    this.notifications.update(list =>
      list.map(n => n.id === id && !n.read ? { ...n, read: true } : n)
    );
    this.save();
  }

  markAllRead(): void {
    this.notifications.update(list => list.map(n => ({ ...n, read: true })));
    this.save();
  }

  prune(): void {
    const now = new Date();
    const m = now.getMonth() + 1, y = now.getFullYear();
    const ugId = this.state.personalUserGroupId();
    if (!ugId) return;

    const id80  = `budget-80-${ugId}-${m}-${y}`;
    const id100 = `budget-100-${ugId}-${m}-${y}`;
    const budget = this.state.budgets().find(b => b.userGroupId === ugId && b.month === m && b.year === y);

    let list = this.notifications();
    const before = list.length;

    if (!budget) {
      list = list.filter(n => n.id !== id80 && n.id !== id100);
    } else {
      const spent = this.state.costs()
        .filter(c => { const d = new Date(c.transactionDate); return d.getMonth()+1===m && d.getFullYear()===y; })
        .reduce((s, c) => s + c.amount, 0);
      const pct = Number(budget.amount) > 0 ? (spent / Number(budget.amount) * 100) : 0;
      if (pct < 100) list = list.filter(n => n.id !== id100);
      if (pct < 80)  list = list.filter(n => n.id !== id80);
    }

    if (list.length !== before) {
      this.notifications.set(list);
      this.save();
    }
  }

  checkBudgetAlerts(): void {
    const prefs = this.getPrefs();
    if (!prefs.budget_alert) return;

    const now = new Date();
    const m = now.getMonth() + 1, y = now.getFullYear();
    const ugId = this.state.personalUserGroupId();
    if (!ugId) return;

    const budget = this.state.budgets().find(b => b.userGroupId === ugId && b.month === m && b.year === y);
    if (!budget) return;

    const spent = this.state.costs()
      .filter(c => { const d = new Date(c.transactionDate); return d.getMonth()+1===m && d.getFullYear()===y; })
      .reduce((s, c) => s + c.amount, 0);

    const limit = Number(budget.amount);
    const pct   = limit > 0 ? (spent / limit * 100) : 0;
    const fmt   = (n: number) => this.state.fmt(n);

    const MONTHS_CAP = ['Siječanj','Veljača','Ožujak','Travanj','Svibanj','Lipanj','Srpanj','Kolovoz','Rujan','Listopad','Studeni','Prosinac'];

    if (pct >= 100) {
      this.add({
        id: `budget-100-${ugId}-${m}-${y}`,
        type: 'budget_100',
        title: 'Budžet prekoračen!',
        desc: `Potrošnja za ${MONTHS_CAP[m-1]} ${y}. premašila je postavljeni limit. Potrošeno ${fmt(spent)} od ${fmt(limit)}.`,
        timestamp: new Date().toISOString(),
        read: false,
      });
    } else if (pct >= 80) {
      this.add({
        id: `budget-80-${ugId}-${m}-${y}`,
        type: 'budget_80',
        title: 'Limit budžeta se približava!',
        desc: `Budžet za ${MONTHS_CAP[m-1]} ${y}. dostigao je ${Math.round(pct)}% — potrošeno ${fmt(spent)} od ${fmt(limit)}.`,
        timestamp: new Date().toISOString(),
        read: false,
      });
    }
  }

  formatTime(iso: string): string {
    if (!iso) return '';
    const d       = new Date(iso);
    const diffMs  = Date.now() - d.getTime();
    const diffMin = Math.floor(diffMs / 60000);
    const diffHr  = Math.floor(diffMs / 3600000);
    const diffDay = Math.floor(diffMs / 86400000);
    if (diffMin < 1)   return 'Upravo';
    if (diffMin < 60)  return diffMin + 'm';
    if (diffHr  < 24)  return diffHr + 'h';
    if (diffDay === 1) return 'Jučer';
    return d.toLocaleDateString('hr-HR', { day: 'numeric', month: 'short' });
  }
}
