import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { StateService } from '../services/state.service';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const token = inject(StateService).token();
  if (token) {
    req = req.clone({ headers: req.headers.set('Authorization', `Bearer ${token}`) });
  }
  return next(req);
};
