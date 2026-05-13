import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-register',
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
        <div class="auth-title">Registracija</div>
        <div class="auth-sub">Kreirajte novi Spendly račun</div>

        <input class="auth-input" type="text"     placeholder="Ime"              [(ngModel)]="form.firstName" />
        <input class="auth-input" type="text"     placeholder="Prezime"          [(ngModel)]="form.lastName" />
        <input class="auth-input" type="email"    placeholder="E-mail"           [(ngModel)]="form.email" />
        <input class="auth-input" type="text"     placeholder="Korisničko ime"   [(ngModel)]="form.username" />
        <input class="auth-input" type="password" placeholder="Lozinka"          [(ngModel)]="form.password" />
        <input class="auth-input" type="password" placeholder="Potvrdi lozinku"  [(ngModel)]="confirm" />

        @if (error()) {
          <div class="auth-err">{{ error() }}</div>
        }

        <button class="auth-btn" [disabled]="loading()" (click)="register()">
          {{ loading() ? 'Registracija...' : 'Registriraj se' }}
        </button>

        <div class="auth-link">Već imate račun? <a routerLink="/login">Prijavite se</a></div>
      </div>
    </div>
  `,
})
export class RegisterComponent {
  private auth   = inject(AuthService);
  private router = inject(Router);

  form    = { firstName: '', lastName: '', email: '', username: '', password: '' };
  confirm = '';
  error   = signal('');
  loading = signal(false);

  register(): void {
    this.error.set('');
    const { firstName, lastName, email, username, password } = this.form;
    if (!firstName || !lastName || !email || !username || !password) { this.error.set('Sva polja su obavezna.'); return; }
    if (password !== this.confirm) { this.error.set('Lozinke se ne podudaraju.'); return; }
    if (password.length < 8)       { this.error.set('Lozinka mora imati najmanje 8 znakova.'); return; }
    this.loading.set(true);
    this.auth.register(this.form).subscribe({
      next: () => this.auth.loadUserData().subscribe({ next: () => this.router.navigate(['/']), error: () => this.router.navigate(['/']) }),
      error: (e: Error) => { this.error.set(this.auth.friendlyErr(e)); this.loading.set(false); },
    });
  }
}
