import { Injectable, signal } from '@angular/core';

export type Theme = 'light' | 'dark';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly THEME_KEY = 'pg_theme';
  private readonly themeSignal = signal<Theme>(this.getSavedTheme());

  readonly theme$ = this.themeSignal.asReadonly();

  constructor() {
    this.applyTheme(this.themeSignal());
  }

  private getSavedTheme(): Theme {
    if (typeof localStorage === 'undefined') return 'light';
    const saved = localStorage.getItem(this.THEME_KEY) as Theme | null;
    return saved ?? this.getSystemTheme();
  }

  private getSystemTheme(): Theme {
    if (typeof window === 'undefined') return 'light';
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  }

  toggleTheme(): void {
    const newTheme: Theme = this.themeSignal() === 'light' ? 'dark' : 'light';
    this.setTheme(newTheme);
  }

  setTheme(theme: Theme): void {
    this.themeSignal.set(theme);
    localStorage.setItem(this.THEME_KEY, theme);
    this.applyTheme(theme);
  }

  private applyTheme(theme: Theme): void {
    if (typeof document === 'undefined') return;
    document.documentElement.setAttribute('data-theme', theme);
  }
}
