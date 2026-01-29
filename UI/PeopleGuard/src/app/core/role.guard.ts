import { CanActivateFn, ActivatedRouteSnapshot } from '@angular/router';
import { inject, PLATFORM_ID } from '@angular/core';
import { Router } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';
import { AuthService, RoleType } from './services/auth.service';

/**
 * Role-based route guard
 * Usage in routes: canActivate: [roleGuard], data: { roles: ['Admin', 'ER'] }
 * 
 * Role Matrix:
 * - Admin: Full access to all features
 * - Business: Can create investigation requests only
 * - ER: Can manage investigations and outcomes
 * - HR: Can manage investigations and outcomes
 * - ITAdmin: Can access audit logs and system settings
 * - Management: Read-only access to reports/dashboard
 */
export const roleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const platformId = inject(PLATFORM_ID);
  if (!isPlatformBrowser(platformId)) {
    return true; // Skip on SSR
  }

  const router = inject(Router);
  const auth = inject(AuthService);
  
  // First check if authenticated
  if (!auth.isAuthenticated()) {
    router.navigateByUrl('/login');
    return false;
  }

  // Get required roles from route data
  const requiredRoles = route.data['roles'] as RoleType[] | undefined;
  
  // If no roles specified, allow access (just needs authentication)
  if (!requiredRoles || requiredRoles.length === 0) {
    return true;
  }

  // Check if user has at least one of the required roles
  if (auth.hasRole(...requiredRoles)) {
    return true;
  }

  // User doesn't have required role - redirect to dashboard
  // Don't log user info - OWASP best practice
  console.warn('Access denied: insufficient permissions for route');
  router.navigateByUrl('/dashboard');
  return false;
};
