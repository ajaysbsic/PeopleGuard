import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { BehaviorSubject, Observable, throwError, of } from 'rxjs';
import { catchError, tap, switchMap } from 'rxjs/operators';

export interface LoginRequestDto {
  email: string;
  password: string;
}

export interface AuthResponseDto {
  accessToken: string;
  userId: string;
  email: string;
  roles: string[];
  expiresAtUtc: string;
}

export interface RefreshResponseDto {
  accessToken: string;
  expiresAtUtc: string;
}

// Role constants matching the spec
export const ROLES = {
  ADMIN: 'Admin',
  BUSINESS: 'Business',
  ER: 'ER',
  HR: 'HR',
  IT_ADMIN: 'ITAdmin',
  MANAGEMENT: 'Management',
  MANAGER: 'Manager'
} as const;

export type RoleType = typeof ROLES[keyof typeof ROLES];

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly baseUrl = `${environment.apiBaseUrl}/Auth`;
  private router = inject(Router);
  private http = inject(HttpClient);
  
  readonly user = signal<AuthResponseDto | null>(null);
  private isRefreshing = false;
  private refreshTokenSubject = new BehaviorSubject<string | null>(null);

  constructor() {
    // Restore session on service init
    this.restoreSession();
  }

  /**
   * Restore user session from localStorage on app init
   */
  private restoreSession(): void {
    const token = localStorage.getItem('pg_token');
    const rolesJson = localStorage.getItem('pg_roles');
    const userId = localStorage.getItem('pg_userId');
    const email = localStorage.getItem('pg_email');
    const expiresAt = localStorage.getItem('pg_expiresAt');

    if (token && rolesJson && userId && email) {
      try {
        const roles = JSON.parse(rolesJson);
        // Check if token is expired
        if (expiresAt && new Date(expiresAt) > new Date()) {
          this.user.set({
            accessToken: token,
            userId,
            email,
            roles,
            expiresAtUtc: expiresAt
          });
        } else {
          // Token expired, try to refresh
          this.attemptTokenRefresh();
        }
      } catch {
        this.clearSession();
      }
    }
  }

  /**
   * Login with email and password
   */
  login(req: LoginRequestDto): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${this.baseUrl}/login`, req, { withCredentials: true }).pipe(
      tap(response => this.setSession(response)),
      catchError(err => {
        console.error('Login failed:', err);
        return throwError(() => err);
      })
    );
  }

  /**
   * Refresh the access token using refresh token (httpOnly cookie)
   */
  refreshToken(): Observable<RefreshResponseDto> {
    return this.http.post<RefreshResponseDto>(`${this.baseUrl}/refresh`, {}, { withCredentials: true }).pipe(
      tap(response => {
        localStorage.setItem('pg_token', response.accessToken);
        localStorage.setItem('pg_expiresAt', response.expiresAtUtc);
        
        const currentUser = this.user();
        if (currentUser) {
          this.user.set({
            ...currentUser,
            accessToken: response.accessToken,
            expiresAtUtc: response.expiresAtUtc
          });
        }
      }),
      catchError(err => {
        this.clearSession();
        this.router.navigateByUrl('/login');
        return throwError(() => err);
      })
    );
  }

  /**
   * Attempt silent token refresh
   */
  private attemptTokenRefresh(): void {
    if (this.isRefreshing) return;
    
    this.isRefreshing = true;
    this.refreshToken().subscribe({
      next: () => {
        this.isRefreshing = false;
      },
      error: () => {
        this.isRefreshing = false;
        this.clearSession();
      }
    });
  }

  /**
   * Set session data after successful login
   */
  setSession(auth: AuthResponseDto): void {
    localStorage.setItem('pg_token', auth.accessToken);
    localStorage.setItem('pg_roles', JSON.stringify(auth.roles ?? []));
    localStorage.setItem('pg_userId', auth.userId);
    localStorage.setItem('pg_email', auth.email);
    localStorage.setItem('pg_expiresAt', auth.expiresAtUtc);
    this.user.set(auth);
  }

  /**
   * Logout - call API and clear session
   */
  logout(): void {
    // Call logout endpoint to invalidate refresh token
    this.http.post(`${this.baseUrl}/logout`, {}, { withCredentials: true }).pipe(
      catchError(() => of(null))
    ).subscribe(() => {
      this.clearSession();
      this.router.navigateByUrl('/login');
    });
  }

  /**
   * Clear all session data
   */
  clearSession(): void {
    localStorage.removeItem('pg_token');
    localStorage.removeItem('pg_roles');
    localStorage.removeItem('pg_userId');
    localStorage.removeItem('pg_email');
    localStorage.removeItem('pg_expiresAt');
    this.user.set(null);
  }

  /**
   * Check if user is authenticated
   */
  isAuthenticated(): boolean {
    const token = localStorage.getItem('pg_token');
    const expiresAt = localStorage.getItem('pg_expiresAt');
    
    if (!token) return false;
    if (expiresAt && new Date(expiresAt) <= new Date()) return false;
    
    return true;
  }

  /**
   * Check if user has any of the specified roles
   */
  hasRole(...roles: RoleType[]): boolean {
    const userRoles = this.user()?.roles ?? [];
    return roles.some(role => userRoles.includes(role));
  }

  /**
   * Check if user has all of the specified roles
   */
  hasAllRoles(...roles: RoleType[]): boolean {
    const userRoles = this.user()?.roles ?? [];
    return roles.every(role => userRoles.includes(role));
  }

  /**
   * Get the refresh token subject for interceptor coordination
   */
  getRefreshTokenSubject(): BehaviorSubject<string | null> {
    return this.refreshTokenSubject;
  }

  /**
   * Get isRefreshing state
   */
  getIsRefreshing(): boolean {
    return this.isRefreshing;
  }

  /**
   * Set isRefreshing state
   */
  setIsRefreshing(value: boolean): void {
    this.isRefreshing = value;
  }
}
