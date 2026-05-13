import { Component, inject, computed } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { StateService } from '../../../core/services/state.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-sidebar',
  imports: [RouterLink, RouterLinkActive],
  template: `
    <nav class="sidebar">
      <!-- Logo -->
      <div class="sb-brand">
        <svg width="28" height="28" viewBox="0 0 28 28" fill="none" xmlns="http://www.w3.org/2000/svg">
          <rect x="1" y="1" width="26" height="26" rx="6" stroke="#3B82F6" stroke-width="1.5" fill="none"/>
          <rect x="5" y="14" width="5" height="9" rx="1.5" fill="#EF4444"/>
          <rect x="11.5" y="9" width="5" height="14" rx="1.5" fill="#F59E0B"/>
          <rect x="18" y="5" width="5" height="18" rx="1.5" fill="#22C55E"/>
        </svg>
        <div class="sb-title">spendly</div>
      </div>

      <!-- Nav -->
      <div class="sb-nav">
        <div class="sb-section-label">Glavno</div>
        <a class="nav-item" routerLink="/dashboard" routerLinkActive="active">
          <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/></svg>
          Nadzorna ploča
        </a>
        <a class="nav-item" routerLink="/transactions" routerLinkActive="active">
          <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="1" x2="12" y2="23"/><path d="M17 5H9.5a3.5 3.5 0 000 7h5a3.5 3.5 0 010 7H6"/></svg>
          Transakcije
        </a>
        <a class="nav-item" routerLink="/categories" routerLinkActive="active">
          <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M4 6h16M4 12h16M4 18h7"/></svg>
          Kategorije
        </a>

        <div class="sb-section-label">Analitika</div>
        <a class="nav-item" routerLink="/reports" routerLinkActive="active">
          <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>
          Izvještaji
        </a>
        <a class="nav-item" routerLink="/budget" routerLinkActive="active">
          <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>
          Zajednički budžet
        </a>

        <div class="sb-section-label">Obitelj</div>
        <a class="nav-item" routerLink="/family" routerLinkActive="active">
          <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 00-3-3.87"/><path d="M16 3.13a4 4 0 010 7.75"/></svg>
          Obiteljska grupa
        </a>

        <div class="sb-section-label">Račun</div>
        <a class="nav-item notif-nav" routerLink="/notifications" routerLinkActive="active">
          <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M18 8A6 6 0 006 8c0 7-3 9-3 9h18s-3-2-3-9"/><path d="M13.73 21a2 2 0 01-3.46 0"/></svg>
          Obavijesti
          @if (unread() > 0) { <span class="notif-badge">{{ unread() }}</span> }
        </a>
        <a class="nav-item" routerLink="/settings" routerLinkActive="active">
          <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="3"/><path d="M19.4 15a1.65 1.65 0 00.33 1.82l.06.06a2 2 0 010 2.83 2 2 0 01-2.83 0l-.06-.06a1.65 1.65 0 00-1.82-.33 1.65 1.65 0 00-1 1.51V21a2 2 0 01-2 2 2 2 0 01-2-2v-.09A1.65 1.65 0 009 19.4a1.65 1.65 0 00-1.82.33l-.06.06a2 2 0 01-2.83-2.83l.06-.06A1.65 1.65 0 004.68 15a1.65 1.65 0 00-1.51-1H3a2 2 0 01-2-2 2 2 0 012-2h.09A1.65 1.65 0 004.6 9a1.65 1.65 0 00-.33-1.82l-.06-.06a2 2 0 012.83-2.83l.06.06A1.65 1.65 0 009 4.68a1.65 1.65 0 001-1.51V3a2 2 0 012-2 2 2 0 012 2v.09a1.65 1.65 0 001 1.51 1.65 1.65 0 001.82-.33l.06-.06a2 2 0 012.83 2.83l-.06.06A1.65 1.65 0 0019.4 9a1.65 1.65 0 001.51 1H21a2 2 0 012 2 2 2 0 01-2 2h-.09a1.65 1.65 0 00-1.51 1z"/></svg>
          Postavke
        </a>
      </div>

      <!-- Account block pinned to bottom -->
      <div style="margin-top:auto;padding-top:12px;border-top:1px solid #1e3a5f">
        <div class="sb-user" style="margin-bottom:8px">
          <div class="sb-avatar">{{ initial() }}</div>
          <div>
            <div class="sb-username">{{ fullName() }}</div>
            <div class="sb-role">Osobni račun</div>
          </div>
        </div>
        <button class="logout-btn" (click)="logout()">Odjava</button>
      </div>
    </nav>
  `,
})
export class SidebarComponent {
  private state  = inject(StateService);
  private notif  = inject(NotificationService);
  private auth   = inject(AuthService);

  readonly unread   = this.notif.unreadCount;
  readonly initial  = computed(() => this.state.user()?.firstName?.charAt(0).toUpperCase() ?? '?');
  readonly fullName = computed(() => {
    const u = this.state.user();
    if (!u) return '';
    return u.firstName + ' ' + u.lastName.charAt(0) + '.';
  });

  logout(): void { this.auth.logout(); }
}
