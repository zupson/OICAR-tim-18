import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  imports: [FormsModule, RouterLink],
  template: `
    <div class="auth-wrap">
      <div class="auth-box">
        <div class="auth-logo">
          <svg width="48" height="48" viewBox="0 0 28 28" fill="none" xmlns="http://www.w3.org/2000/svg">
            <rect x="1" y="1" width="26" height="26" rx="6" stroke="#3B82F6" stroke-width="1.5" fill="none"/>
            <rect x="5" y="14" width="5" height="9" rx="1.5" fill="#EF4444"/>
            <rect x="11.5" y="9" width="5" height="14" rx="1.5" fill="#F59E0B"/>
            <rect x="18" y="5" width="5" height="18" rx="1.5" fill="#22C55E"/>
          </svg>
        </div>
        <div class="auth-title">spendly</div>
        <div class="auth-sub">Upravljajte financijama pametno</div>

        <input class="auth-input" type="text"     placeholder="Korisničko ime" [(ngModel)]="username" (keyup.enter)="login()" />
        <input class="auth-input" type="password" placeholder="Lozinka"        [(ngModel)]="password" (keyup.enter)="login()" />

        @if (error()) {
          <div class="auth-err">{{ error() }}</div>
        }

        <button class="auth-btn" [disabled]="loading()" (click)="login()">
          {{ loading() ? 'Prijava...' : 'Prijava' }}
        </button>

        <div class="auth-link">Nemate račun? <a routerLink="/register">Registrirajte se</a></div>
      </div>
    </div>
  `,
})
export class LoginComponent {
  private auth   = inject(AuthService);
  private router = inject(Router);

  username = '';
  password = '';
  error    = signal('');
  loading  = signal(false);

  login(): void {
    this.error.set('');
    if (!this.username || !this.password) { this.error.set('Unesite korisničko ime i lozinku.'); return; }
    this.loading.set(true);
    this.auth.login(this.username, this.password).subscribe({
      next: () => this.auth.loadUserData().subscribe({ next: () => this.router.navigate(['/']), error: () => this.router.navigate(['/']) }),
      error: (e: Error) => { this.error.set(this.auth.friendlyErr(e)); this.loading.set(false); },
    });
  }
}
