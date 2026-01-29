import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ThemeService } from '../../core/services/theme.service';
import { SidebarComponent } from '../../shared/components/sidebar.component';
import { LanguageSwitcherComponent } from '../../shared/components/language-switcher.component';

@Component({
  selector: 'pg-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, SidebarComponent, LanguageSwitcherComponent],
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss']
})
export class LayoutComponent {
  auth = inject(AuthService);
  themeService = inject(ThemeService);
  router = inject(Router);
  
  theme$ = this.themeService.theme$;

  logout() {
    // auth.logout() already handles navigation to login page
    this.auth.logout();
  }

  toggleTheme() {
    this.themeService.toggleTheme();
  }
}
