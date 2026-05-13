import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-bottom-nav',
  imports: [RouterLink, RouterLinkActive],
  template: `
    <nav class="bottom-nav">
      <a class="bn-item" routerLink="/dashboard"    routerLinkActive="active">
        <span>📊</span><span>Ploča</span>
      </a>
      <a class="bn-item" routerLink="/transactions" routerLinkActive="active">
        <span>💳</span><span>Transakcije</span>
      </a>
      <a class="bn-item" routerLink="/reports"      routerLinkActive="active">
        <span>📈</span><span>Izvještaji</span>
      </a>
      <a class="bn-item" routerLink="/family"       routerLinkActive="active">
        <span>👥</span><span>Obitelj</span>
      </a>
      <a class="bn-item" routerLink="/settings"     routerLinkActive="active" style="position:relative">
        <span>⚙️</span><span>Postavke</span>
        @if (unread() > 0) {
          <span class="notif-badge" style="position:absolute;top:2px;right:10px;font-size:9px">{{ unread() }}</span>
        }
      </a>
    </nav>
  `,
})
export class BottomNavComponent {
  private notif = inject(NotificationService);
  readonly unread = this.notif.unreadCount;
}
