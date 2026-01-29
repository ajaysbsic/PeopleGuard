import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

interface MenuItem {
  label: string;
  path: string;
  roles: string[];
  icon: string;
}

@Component({
  selector: 'pg-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  template: `
    <aside class="sidebar">
      <nav class="menu">
        <div *ngFor="let item of visibleMenuItems" class="menu-item">
          <a [routerLink]="item.path" routerLinkActive="active" class="menu-link">
            <span class="menu-icon">{{ item.icon }}</span>
            <span class="menu-label">{{ item.label }}</span>
          </a>
        </div>
      </nav>
    </aside>
  `,
  styles: [`
    .sidebar {
      width: 260px;
      background: var(--bg-secondary);
      border-right: 1px solid var(--border-color);
      padding: 16px 0;
      height: calc(100vh - 60px);
      overflow-y: auto;
      position: fixed;
      left: 0;
      top: 60px;
      z-index: 999;
    }

    .menu {
      display: flex;
      flex-direction: column;
    }

    .menu-item {
      margin: 0;
    }

    .menu-link {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 12px 16px;
      color: var(--text-primary);
      text-decoration: none;
      transition: all 0.2s;
      border-left: 4px solid transparent;

      &:hover {
        background: var(--bg-tertiary);
        color: var(--color-secondary);
      }

      &.active {
        background: var(--bg-tertiary);
        border-left-color: var(--color-secondary);
        color: var(--color-secondary);
        font-weight: 500;
      }
    }

    .menu-icon {
      font-size: 18px;
      width: 24px;
      text-align: center;
    }

    .menu-label {
      flex: 1;
    }
  `]
})
export class SidebarComponent {
  private auth = inject(AuthService);

  private allMenuItems: MenuItem[] = [
    { label: 'Dashboard', path: '/dashboard', roles: ['Admin', 'Manager', 'ER', 'HR', 'Management', 'Business'], icon: '📊' },
    { label: 'Employees', path: '/employees', roles: ['Admin', 'Manager', 'ER', 'HR'], icon: '👥' },
    { label: 'Cases', path: '/cases', roles: ['Admin', 'Manager', 'ER', 'HR', 'Management'], icon: '📋' },
    { label: 'New Case', path: '/cases/new', roles: ['Admin', 'Business', 'ER', 'HR'], icon: '➕' },
    { label: 'Leave Requests', path: '/leaves', roles: ['Admin', 'ER', 'Management'], icon: '🧾' },
    { label: 'New Leave', path: '/leaves/new', roles: ['Admin'], icon: '⏱️' },
    { label: 'QR Codes', path: '/admin/qr-tokens', roles: ['Admin', 'ER', 'HR'], icon: '📱' },
    { label: 'Warning Letters', path: '/warning-letters', roles: ['Admin', 'ER', 'HR'], icon: '⚠️' },
    { label: 'Audit Logs', path: '/audit-logs', roles: ['Admin', 'ITAdmin'], icon: '📝' },
    { label: 'Audit Viewer', path: '/admin/audit', roles: ['Admin', 'ITAdmin'], icon: '🔍' },
  ];

  get visibleMenuItems(): MenuItem[] {
    const userRoles = this.auth.user()?.roles ?? [];
    return this.allMenuItems.filter(item =>
      item.roles.some(role => userRoles.includes(role))
    );
  }
}
