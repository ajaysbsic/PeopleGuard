import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslationService, Language } from '../../core/services/translation.service';

@Component({
  selector: 'pg-language-switcher',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="language-switcher" [dir]="translation.isRtl() ? 'rtl' : 'ltr'">
      <button
        (click)="toggleMenu()"
        class="switcher-button"
        [attr.aria-label]="'Current language: ' + translation.currentLang()"
        [attr.aria-expanded]="menuOpen()"
        aria-haspopup="menu"
      >
        <span class="flag">{{ getLanguageFlag() }}</span>
        <span class="label">{{ translation.currentLang() === 'ar' ? 'العربية' : 'English' }}</span>
        <span class="icon">▼</span>
      </button>

      @if (menuOpen()) {
        <div class="language-menu" role="menu">
          @for (lang of languages; track lang.code) {
            <button
              (click)="selectLanguage(lang.code)"
              class="menu-item"
              [class.active]="lang.code === translation.currentLang()"
              role="menuitem"
              [attr.aria-label]="'Switch to ' + lang.name"
            >
              <span class="flag">{{ getFlag(lang.code) }}</span>
              <span class="name">{{ lang.name }}</span>
            </button>
          }

          <div class="menu-divider"></div>

          <button
            (click)="toggleRtl()"
            class="menu-item rtl-toggle"
            role="menuitem"
            [attr.aria-label]="(translation.isRtl() ? 'Disable' : 'Enable') + ' RTL mode'"
          >
            <span class="icon">{{ translation.isRtl() ? '↔' : '↔' }}</span>
            <span class="name">{{ translation.isRtl() ? 'LTR Mode' : 'RTL Mode' }}</span>
          </button>
        </div>
      }
    </div>
  `,
  styles: [`
    .language-switcher {
      position: relative;
      display: inline-block;

      .switcher-button {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        padding: 0.5rem 1rem;
        background: white;
        border: 1px solid #ddd;
        border-radius: 4px;
        cursor: pointer;
        font-size: 0.9rem;
        font-weight: 500;
        color: #333;
        transition: all 0.2s;

        &:hover {
          background: #f5f5f5;
          border-color: #999;
        }

        &:focus {
          outline: none;
          border-color: #2196f3;
          box-shadow: 0 0 0 3px rgba(33, 150, 243, 0.2);
        }

        .flag {
          font-size: 1.2rem;
        }

        .label {
          min-width: 80px;
          text-align: left;
        }

        .icon {
          font-size: 0.7rem;
          transition: transform 0.2s;
        }

        &[aria-expanded="true"] .icon {
          transform: rotate(180deg);
        }
      }

      .language-menu {
        position: absolute;
        top: calc(100% + 0.5rem);
        left: 0;
        background: white;
        border: 1px solid #ddd;
        border-radius: 4px;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
        min-width: 180px;
        z-index: 100;
        animation: slideDown 0.2s ease-out;

        @keyframes slideDown {
          from {
            opacity: 0;
            transform: translateY(-8px);
          }
          to {
            opacity: 1;
            transform: translateY(0);
          }
        }

        .menu-item {
          display: flex;
          align-items: center;
          gap: 0.75rem;
          width: 100%;
          padding: 0.75rem 1rem;
          background: none;
          border: none;
          color: #333;
          cursor: pointer;
          text-align: left;
          font-size: 0.9rem;
          transition: background 0.2s;

          &:hover {
            background: #f5f5f5;
          }

          &:focus {
            outline: none;
            background: #f0f0f0;
            box-shadow: inset 0 0 0 2px #2196f3;
          }

          &.active {
            background: #e3f2fd;
            color: #2196f3;
            font-weight: 600;
          }

          .flag {
            font-size: 1.1rem;
            min-width: 24px;
            text-align: center;
          }

          .name {
            flex: 1;
          }

          &.rtl-toggle {
            color: #666;
            font-style: italic;
          }
        }

        .menu-divider {
          height: 1px;
          background: #eee;
          margin: 0.5rem 0;
        }
      }
    }

    @media (max-width: 600px) {
      .language-switcher {
        .switcher-button {
          padding: 0.4rem 0.8rem;
          font-size: 0.85rem;

          .label {
            display: none;
          }

          .flag {
            font-size: 1rem;
          }
        }

        .language-menu {
          right: 0;
          left: auto;
        }
      }
    }
  `]
})
export class LanguageSwitcherComponent {
  translation = inject(TranslationService);

  // Use signal so template can call menuOpen()
  menuOpen = signal(false);
  languages: { code: Language; name: string }[] = [
    { code: 'en', name: 'English' },
    { code: 'ar', name: 'العربية' }
  ];

  toggleMenu() {
    this.menuOpen.update((open) => !open);
  }

  async selectLanguage(lang: Language) {
    await this.translation.switchLanguage(lang);
    this.menuOpen.set(false);
  }

  toggleRtl() {
    // RTL follows language in translation service; toggling switches between ar/en for now
    const nextLang: Language = this.translation.currentLang() === 'ar' ? 'en' : 'ar';
    this.selectLanguage(nextLang);
  }

  getLanguageFlag(): string {
    const lang = this.translation.currentLang();
    return this.getFlag(lang);
  }

  getFlag(lang: Language): string {
    const flags: Record<Language, string> = {
      en: '🇺🇸',
      ar: '🇸🇦'
    };
    return flags[lang] || lang.toUpperCase();
  }
}
