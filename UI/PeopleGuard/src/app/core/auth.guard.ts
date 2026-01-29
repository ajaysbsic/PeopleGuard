import { CanActivateFn } from '@angular/router';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { AuthService } from './services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const platformId = inject(PLATFORM_ID);

  if (!isPlatformBrowser(platformId)) {
    return false;
  }

  // Check if user is authenticated
  const isAuth = authService.isAuthenticated();
  
  if (isAuth) {
    return true;
  }

  // Not authenticated, redirect to login
  router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
  return false;
};
