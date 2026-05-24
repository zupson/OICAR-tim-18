import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { BottomNavComponent } from '../bottom-nav/bottom-nav.component';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, SidebarComponent, BottomNavComponent],
  template: `
    <div class="app-layout">
      <app-sidebar />
      <main class="main-content">
        <router-outlet />
      </main>
      <app-bottom-nav />
    </div>
  `,
})
export class ShellComponent implements OnInit {
  private auth   = inject(AuthService);
  private router = inject(Router);

  ngOnInit(): void {
    this.auth.loadUserData().subscribe({
      error: () => {
        this.auth.logout();
      }
    });
  }
}
