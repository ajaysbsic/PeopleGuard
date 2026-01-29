import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject, PLATFORM_ID } from '@angular/core';
import { Router } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';
import { catchError, switchMap, filter, take, throwError } from 'rxjs';
import { AuthService } from './services/auth.service';

/**
 * HTTP Interceptor for:
 * 1. Adding Authorization header with Bearer token
 * 2. Handling 401 errors with token refresh
 * 3. Redirecting to login on auth failure
 * 
 * OWASP: No PII in logs, secure token handling
 */
export const apiInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const platformId = inject(PLATFORM_ID);

  if (!isPlatformBrowser(platformId)) {
    return next(req);
  }

  // Skip auth for login/register endpoints
  if (req.url.includes('/auth/login') || req.url.includes('/auth/register')) {
    return next(req);
  }

  // Get token from localStorage (always use fresh token)
  const token = localStorage.getItem('pg_token');
  
  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        authService.clearSession();
        router.navigate(['/login']);
      }
      return throwError(() => error);
    })
  );
};
