import { Pipe, PipeTransform, inject } from '@angular/core';
import { TranslationService } from '../services/translation.service';

/**
 * Translate pipe for i18n
 * Usage: {{ 'CASES.CREATE_TITLE' | translate }}
 * With params: {{ 'CASES.SUMMARY_HELP' | translate: { min: 20, max: 5000 } }}
 */
@Pipe({
  name: 'translate',
  standalone: true,
  pure: false // Required to update when language changes
})
export class TranslatePipe implements PipeTransform {
  private translationService = inject(TranslationService);

  transform(key: string, params?: Record<string, any>): string {
    return this.translationService.translate(key, params);
  }
}
