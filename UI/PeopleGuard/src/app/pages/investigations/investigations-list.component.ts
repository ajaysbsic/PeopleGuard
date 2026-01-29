import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { InvestigationsService, InvestigationDto } from '../../core/services/investigations.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'pg-investigations-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './investigations-list.component.html',
  styleUrls: ['./investigations-list.component.scss']
})
export class InvestigationsListComponent implements OnInit {
  private svc = inject(InvestigationsService);
  private router = inject(Router);
  private auth = inject(AuthService);
  private cdr = inject(ChangeDetectorRef);

  items: InvestigationDto[] = [];
  loading = true;
  canCreateInvestigation = false;

  ngOnInit() {
    this.loadInvestigations();
    this.canCreateInvestigation = this.auth.user()?.roles?.includes('ER') || this.auth.user()?.roles?.includes('Admin') ? true : false;
  }

  loadInvestigations() {
    this.loading = true;
    this.svc.getAll().subscribe({
      next: r => { 
        this.items = r; 
        this.loading = false; 
        this.cdr.detectChanges();
      },
      error: _ => { 
        this.loading = false; 
        this.cdr.detectChanges();
        alert('Failed to load investigations'); 
      }
    });
  }

  createInvestigation() {
    this.router.navigateByUrl('/investigations/create');
  }

  deleteInvestigation(id: string) {
    if (!confirm('Are you sure you want to delete this investigation?')) {
      return;
    }
    this.svc.delete(id).subscribe({
      next: () => {
        this.loadInvestigations();
        alert('Investigation deleted successfully');
      },
      error: _ => alert('Failed to delete investigation')
    });
  }

  getStatusClass(status: string): string {
    return `status-${status.toLowerCase().replace(' ', '-')}`;
  }
}
