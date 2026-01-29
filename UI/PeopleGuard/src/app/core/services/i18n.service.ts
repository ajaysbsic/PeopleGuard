import { Injectable, signal, effect } from '@angular/core';
import { Directionality } from '@angular/cdk/bidi';

export type SupportedLanguage = 'en' | 'ar' | 'fr' | 'es';

interface LanguageConfig {
  code: SupportedLanguage;
  name: string;
  direction: 'ltr' | 'rtl';
  flag?: string;
}

@Injectable({
  providedIn: 'root'
})
export class I18nService {
  private readonly STORAGE_KEY = 'pg_language';
  private readonly STORAGE_KEY_RTL = 'pg_rtl';

  private languageConfigs: Record<SupportedLanguage, LanguageConfig> = {
    en: { code: 'en', name: 'English', direction: 'ltr' },
    ar: { code: 'ar', name: 'العربية', direction: 'rtl' },
    fr: { code: 'fr', name: 'Français', direction: 'ltr' },
    es: { code: 'es', name: 'Español', direction: 'ltr' }
  };

  private currentLanguage = signal<SupportedLanguage>('en');
  private rtlEnabled = signal<boolean>(false);

  // Translations dictionary (can be expanded with full i18n solution like ngx-translate)
  private translations: Record<SupportedLanguage, Record<string, string>> = {
    en: {
      'common.close': 'Close',
      'common.cancel': 'Cancel',
      'common.save': 'Save',
      'common.submit': 'Submit',
      'common.delete': 'Delete',
      'common.edit': 'Edit',
      'common.view': 'View',
      'common.loading': 'Loading...',
      'common.error': 'An error occurred',
      'common.success': 'Success',
      'qr.title': 'Submit Your Report',
      'qr.category': 'Report Type',
      'qr.message': 'Message',
      'qr.submit': 'Submit Report',
      'qr.anonymous': 'Submit Anonymously',
      'audit.title': 'Audit Trail Viewer',
      'audit.filters': 'Filters',
      'audit.export': 'Export CSV'
    },
    ar: {
      'common.close': 'إغلاق',
      'common.cancel': 'إلغاء',
      'common.save': 'حفظ',
      'common.submit': 'إرسال',
      'common.delete': 'حذف',
      'common.edit': 'تعديل',
      'common.view': 'عرض',
      'common.loading': 'جاري التحميل...',
      'common.error': 'حدث خطأ',
      'common.success': 'نجح',
      'qr.title': 'إرسال التقرير',
      'qr.category': 'نوع التقرير',
      'qr.message': 'الرسالة',
      'qr.submit': 'إرسال التقرير',
      'qr.anonymous': 'الإرسال بشكل مجهول',
      'audit.title': 'عارض سجل التدقيق',
      'audit.filters': 'المرشحات',
      'audit.export': 'تصدير CSV'
    },
    fr: {
      'common.close': 'Fermer',
      'common.cancel': 'Annuler',
      'common.save': 'Enregistrer',
      'common.submit': 'Soumettre',
      'common.delete': 'Supprimer',
      'common.edit': 'Modifier',
      'common.view': 'Afficher',
      'common.loading': 'Chargement...',
      'common.error': 'Une erreur s\'est produite',
      'common.success': 'Succès',
      'qr.title': 'Soumettre votre rapport',
      'qr.category': 'Type de rapport',
      'qr.message': 'Message',
      'qr.submit': 'Soumettre le rapport',
      'qr.anonymous': 'Soumettre anonymement',
      'audit.title': 'Visionneuse de journal d\'audit',
      'audit.filters': 'Filtres',
      'audit.export': 'Exporter CSV'
    },
    es: {
      'common.close': 'Cerrar',
      'common.cancel': 'Cancelar',
      'common.save': 'Guardar',
      'common.submit': 'Enviar',
      'common.delete': 'Eliminar',
      'common.edit': 'Editar',
      'common.view': 'Ver',
      'common.loading': 'Cargando...',
      'common.error': 'Ocurrió un error',
      'common.success': 'Éxito',
      'qr.title': 'Enviar su informe',
      'qr.category': 'Tipo de informe',
      'qr.message': 'Mensaje',
      'qr.submit': 'Enviar informe',
      'qr.anonymous': 'Enviar anónimamente',
      'audit.title': 'Visor de registro de auditoría',
      'audit.filters': 'Filtros',
      'audit.export': 'Exportar CSV'
    }
  };

  currentLanguage$ = this.currentLanguage.asReadonly();
  rtlEnabled$ = this.rtlEnabled.asReadonly();

  constructor(private directionality?: Directionality) {
    // Initialize stored values after construction
    const storedLang = this.getStoredLanguage();
    this.currentLanguage.set(storedLang);
    
    const storedRtl = this.getStoredRtl();
    this.rtlEnabled.set(storedRtl);

    // Update document direction when RTL changes
    effect(() => {
      const rtl = this.rtlEnabled();
      document.documentElement.dir = rtl ? 'rtl' : 'ltr';
      document.documentElement.lang = this.currentLanguage();
    });
  }

  setLanguage(language: SupportedLanguage) {
    this.currentLanguage.set(language);
    localStorage.setItem(this.STORAGE_KEY, language);
  }

  getLanguage(): SupportedLanguage {
    return this.currentLanguage();
  }

  setRtl(enabled: boolean) {
    this.rtlEnabled.set(enabled);
    localStorage.setItem(this.STORAGE_KEY_RTL, String(enabled));
  }

  isRtl(): boolean {
    return this.rtlEnabled();
  }

  toggleRtl() {
    this.setRtl(!this.rtlEnabled());
  }

  getLanguageName(lang: SupportedLanguage): string {
    return this.languageConfigs[lang]?.name || lang;
  }

  getDirection(lang: SupportedLanguage): 'ltr' | 'rtl' {
    return this.languageConfigs[lang]?.direction || 'ltr';
  }

  getSupportedLanguages(): LanguageConfig[] {
    return Object.values(this.languageConfigs);
  }

  translate(key: string, defaultValue?: string): string {
    const lang = this.currentLanguage();
    const translation = this.translations[lang]?.[key];
    return translation || defaultValue || key;
  }

  t(key: string): string {
    return this.translate(key);
  }

  private getStoredLanguage(): SupportedLanguage {
    const stored = localStorage.getItem(this.STORAGE_KEY);
    if (stored && this.isSupportedLanguage(stored)) {
      return stored;
    }
    // Default to browser language or English
    const browserLang = navigator.language.split('-')[0];
    return this.isSupportedLanguage(browserLang) ? (browserLang as SupportedLanguage) : 'en';
  }

  private getStoredRtl(): boolean {
    const stored = localStorage.getItem(this.STORAGE_KEY_RTL);
    if (stored !== null) {
      return stored === 'true';
    }
    // Default RTL based on current language
    return this.getDirection(this.currentLanguage()) === 'rtl';
  }

  private isSupportedLanguage(lang: any): lang is SupportedLanguage {
    return Object.keys(this.languageConfigs).includes(lang);
  }
}
