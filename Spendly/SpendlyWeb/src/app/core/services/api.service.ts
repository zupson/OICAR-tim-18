import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);

  private handle(err: HttpErrorResponse) {
    const msg = typeof err.error === 'string' ? err.error : err.message;
    return throwError(() => new Error(msg || err.statusText));
  }

  get<T>(path: string) {
    return this.http.get<T>(`/api${path}`).pipe(catchError(e => this.handle(e)));
  }

  post<T>(path: string, body?: unknown) {
    return this.http.post<T>(`/api${path}`, body ?? null).pipe(catchError(e => this.handle(e)));
  }

  put<T>(path: string, body?: unknown) {
    return this.http.put<T>(`/api${path}`, body ?? null).pipe(catchError(e => this.handle(e)));
  }

  delete<T>(path: string) {
    return this.http.delete<T>(`/api${path}`).pipe(catchError(e => this.handle(e)));
  }
}
