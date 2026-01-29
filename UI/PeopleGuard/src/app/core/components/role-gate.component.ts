import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService, RoleType } from '../services/auth.service';

/**
 * RoleGate Component - Show/hide content based on user roles
 * 
 * Usage:
 * <role-gate [roles]="['Admin', 'ER']">
 *   <button>Admin/ER Only Action</button>
 * </role-gate>
 * 
 * Use requireAll for AND logic:
 * <role-gate [roles]="['Admin', 'ER']" [requireAll]="true">
 *   <span>Must have BOTH Admin AND ER roles</span>
 * </role-gate>
 */
@Component({
  selector: 'role-gate',
  standalone: true,
  imports: [CommonModule],
  template: `<ng-content *ngIf="hasAccess()"></ng-content>`
})
export class RoleGateComponent {
  /** Roles that grant access (OR logic by default) */
  @Input() roles: RoleType[] = [];
  
  /** If true, user must have ALL specified roles (AND logic) */
  @Input() requireAll = false;

  constructor(private auth: AuthService) {}

  hasAccess(): boolean {
    if (!this.roles || this.roles.length === 0) {
      return true;
    }
    
    if (this.requireAll) {
      return this.auth.hasAllRoles(...this.roles);
    }
    
    return this.auth.hasRole(...this.roles);
  }
}
