import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export type Language = 'en' | 'ar';

@Injectable({ providedIn: 'root' })
export class TranslationService {
  private translations: Record<string, any> = {};
  readonly currentLang = signal<Language>('en');
  readonly isRtl = signal(false);

  constructor(private http: HttpClient) {
    this.loadLanguage('en');
  }

  loadLanguage(lang: Language): Promise<void> {
    return new Promise((resolve) => {
      this.http.get<Record<string, any>>(`/assets/i18n/${lang}.json`).subscribe({
        next: (data) => {
          this.translations = data;
          this.currentLang.set(lang);
          this.isRtl.set(lang === 'ar');
          document.documentElement.setAttribute('lang', lang);
          document.documentElement.setAttribute('dir', lang === 'ar' ? 'rtl' : 'ltr');
          resolve();
        },
        error: () => {
          console.error(`Failed to load ${lang} translations`);
          resolve();
        }
      });
    });
  }

  switchLanguage(lang: Language): Promise<void> {
    return this.loadLanguage(lang);
  }

  /**
   * Get translated string by key
   * Supports nested keys like "CASES.CREATE_TITLE"
   * Supports interpolation like "Hello {name}"
   */
  translate(key: string, params?: Record<string, any>): string {
    const keys = key.split('.');
    let value: any = this.translations;

    for (const k of keys) {
      if (value && typeof value === 'object' && k in value) {
        value = value[k];
      } else {
        return key; // Return key if not found
      }
    }

    if (typeof value !== 'string') {
      return key;
    }

    // Handle interpolation
    if (params) {
      return value.replace(/\{(\w+)\}/g, (_, paramKey) => {
        return params[paramKey]?.toString() ?? `{${paramKey}}`;
      });
    }

    return value;
  }

  /**
   * Instant translation (alias for translate)
   */
  instant(key: string, params?: Record<string, any>): string {
    return this.translate(key, params);
  }
}
