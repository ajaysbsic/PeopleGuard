import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';
import { roleGuard } from './core/role.guard';

/**
 * Route Configuration with Role-Based Access Control
 * 
 * Role Matrix:
 * - Admin: Full access to all features
 * - Business: Can create investigation requests only
 * - ER: Can manage investigations and outcomes
 * - HR: Can manage investigations and outcomes
 * - ITAdmin: Can access audit logs and system settings
 * - Management: Read-only access to reports/dashboard
 * - Manager: Can view investigations for their team
 */
export const routes: Routes = [
	{ path: 'login', loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent) },
	
	// Public routes (no authentication required)
	{ 
		path: 'qr/:token', 
		loadComponent: () => import('./pages/public/qr-submit.component').then(m => m.QrSubmitComponent) 
	},
	
	{
		path: '',
		loadComponent: () => import('./pages/layout/layout.component').then(m => m.LayoutComponent),
		canActivate: [authGuard],
		children: [
			{ path: '', pathMatch: 'full', redirectTo: 'dashboard' },
			
			// Dashboard - accessible to all authenticated users
			{ 
				path: 'dashboard', 
				loadComponent: () => import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent) 
			},
			
			// Employees - Admin and HR only
			{ 
				path: 'employees', 
				loadComponent: () => import('./pages/employees/employees-list.component').then(m => m.EmployeesListComponent),
				canActivate: [roleGuard],
				data: { roles: ['Admin', 'HR'] }
			},
			{ 
				path: 'employees/:id', 
				loadComponent: () => import('./pages/employees/employee-detail.component').then(m => m.EmployeeDetailComponent),
				canActivate: [roleGuard],
				data: { roles: ['Admin', 'HR', 'ER', 'Manager'] }
			},
			
			// Investigations - Various access levels
			{ 
				path: 'investigations', 
				loadComponent: () => import('./pages/investigations/investigations-list.component').then(m => m.InvestigationsListComponent),
				canActivate: [roleGuard],
				data: { roles: ['Admin', 'ER', 'HR', 'Manager', 'Management'] }
			},
			{ 
				path: 'investigations/create', 
				loadComponent: () => import('./pages/investigations/investigation-create.component').then(m => m.InvestigationCreateComponent),
				canActivate: [roleGuard],
				data: { roles: ['Admin', 'Business', 'ER', 'HR'] }
			},
			{ 
				path: 'investigations/:id', 
				loadComponent: () => import('./pages/investigations/investigation-detail.component').then(m => m.InvestigationDetailComponent),
				canActivate: [roleGuard],
				data: { roles: ['Admin', 'ER', 'HR', 'Manager', 'Management'] }
			},
			
			// Cases - Case list and management
			{ 
				path: 'cases', 
				loadComponent: () => import('./pages/cases/case-list.component').then(m => m.CaseListComponent),
				canActivate: [roleGuard],
				data: { roles: ['Admin', 'ER', 'HR', 'Manager', 'Management'] }
			},
			{ 
				path: 'cases/new', 
				loadComponent: () => import('./pages/cases/case-create.component').then(m => m.CaseCreateComponent),
				canActivate: [roleGuard],
				data: { roles: ['Admin', 'Business', 'ER', 'HR'] }
			},
			{ 
				path: 'cases/:id', 
				loadComponent: () => import('./pages/cases/case-detail.component').then(m => m.CaseDetailComponent),
				canActivate: [roleGuard],
				data: { roles: ['Admin', 'ER', 'HR', 'Manager', 'Management'] }
			},

			// Emergency Leave Requests
			{ 
				path: 'leaves',
				loadComponent: () => import('./pages/leaves/leaves-list.component').then(m => m.LeavesListComponent),
				canActivate: [roleGuard],
				data: { roles: ['Admin', 'ER', 'Management'] }
			},
			{ 
				path: 'leaves/new',
				loadComponent: () => import('./pages/leaves/leave-create.component').then(m => m.LeaveCreateComponent),
				canActivate: [roleGuard],
				data: { roles: ['Admin'] }
			},
			{ 
				path: 'leaves/:id',
				loadComponent: () => import('./pages/leaves/leave-detail.component').then(m => m.LeaveDetailComponent),
				canActivate: [roleGuard],
				data: { roles: ['Admin', 'ER', 'Management'] }
			},
			
			// Warning Letters - Admin and ER/HR only
			{ 
				path: 'warning-letters', 
				loadComponent: () => import('./pages/warnings/warning-letters-list.component').then(m => m.WarningLettersListComponent),
				canActivate: [roleGuard],
				data: { roles: ['Admin', 'ER', 'HR', 'Management', 'Manager', 'Business'] }
			},
			{ 
				path: 'warning-letters/create', 
				loadComponent: () => import('./pages/warnings/warning-letter-create.component').then(m => m.WarningLetterCreateComponent),
				canActivate: [roleGuard],
				data: { roles: ['Admin', 'ER', 'HR', 'Management', 'Manager', 'Business'] }
			},
			
			// Audit Logs - Admin and ITAdmin only
			{ 
				path: 'audit-logs', 
				loadComponent: () => import('./pages/audit-logs/audit-logs.component').then(m => m.AuditLogsComponent),
				canActivate: [roleGuard],
				data: { roles: ['Admin', 'ITAdmin'] }
			},
			
			// QR Token Management - Admin only
			{ 
				path: 'admin/qr-tokens', 
				loadComponent: () => import('./pages/admin/qr-generate.component').then(m => m.QrGenerateComponent),
				canActivate: [roleGuard],
				data: { roles: ['Admin', 'ER', 'HR'] }
			},
			
			// Audit Viewer - Admin and ITAdmin only
			{ 
				path: 'admin/audit', 
				loadComponent: () => import('./pages/admin/audit-viewer.component').then(m => m.AuditViewerComponent),
				canActivate: [roleGuard],
				data: { roles: ['Admin', 'ITAdmin'] }
			}
		]
	}
];
