import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { StateService } from '../services/state.service';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = () => {
  const state  = inject(StateService);
  const auth   = inject(AuthService);
  const router = inject(Router);

  // Restore token from localStorage if not in state yet
  if (!state.token()) {
    auth.tryAutoLogin();
  }

  if (state.token()) return true;
  router.navigate(['/login']);
  return false;
};
