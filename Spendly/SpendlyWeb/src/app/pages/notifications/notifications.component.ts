import { Component, inject, computed } from '@angular/core';
import { NotificationService } from '../../core/services/notification.service';

const NOTIF_ICONS: Record<string, { bg: string; icon: string }> = {
  budget_80:  { bg: '#3a1200', icon: '⚠️' },
  budget_100: { bg: '#3a0000', icon: '🚨' },
};

@Component({
  selector: 'app-notifications',
  template: `
    <div class="page-hdr">
      <div>
        <div class="page-title">Obavijesti</div>
        <div class="page-sub">{{ unread() > 0 ? unread() + ' nepročitanih obavijesti' : 'Sve obavijesti su pročitane' }}</div>
      </div>
      <button class="btn-ghost" (click)="notif.markAllRead()">Označi sve kao pročitano</button>
    </div>

    <div class="card" style="padding:14px">
      <div id="notif-list">
        @if (!notif.notifications().length) {
          <div style="text-align:center;padding:32px;color:#64748B;font-size:13px">Nema obavijesti.</div>
        }
        @for (n of notif.notifications(); track n.id) {
          <div class="notif-item" [class.unread]="!n.read" [class.read]="n.read" (click)="notif.markRead(n.id)">
            <div style="display:flex;gap:9px;align-items:flex-start;width:100%">
              <div class="n-icon" [style.background]="icon(n.type).bg">{{ icon(n.type).icon }}</div>
              <div class="n-body">
                <div class="n-title">{{ n.title }}</div>
                <div class="n-desc">{{ n.desc }}</div>
              </div>
              <div style="display:flex;flex-direction:column;align-items:flex-end;gap:5px">
                <div class="n-time">{{ notif.formatTime(n.timestamp) }}</div>
                @if (!n.read) { <div class="n-dot"></div> }
              </div>
            </div>
          </div>
        }
      </div>
    </div>
  `,
})
export class NotificationsComponent {
  readonly notif  = inject(NotificationService);
  readonly unread = this.notif.unreadCount;

  icon(type: string) { return NOTIF_ICONS[type] ?? { bg: '#1e3a5f', icon: '🔔' }; }
}
