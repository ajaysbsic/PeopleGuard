import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import {
  EmployeesService,
  EmployeeDto,
  EmployeeHistoryItem,
  EmployeeStatsDto,
  PagedResponse
} from '../../core/services/employees.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'pg-employee-detail',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './employee-detail.component.html',
  styleUrls: ['./employee-detail.component.scss']
})
export class EmployeeDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private employeesSvc = inject(EmployeesService);
  private cdr = inject(ChangeDetectorRef);
  Math = Math;

  employee: EmployeeDto | null = null;
  stats: EmployeeStatsDto | null = null;
  history: PagedResponse<EmployeeHistoryItem> | null = null;

  loading = true;
  loadingHistory = false;
  error: string | null = null;

  filters = {
    type: 'all',
    from: '',
    to: ''
  };

  page = 1;
  size = 10;

  ngOnInit() {
    this.loadEmployee();
  }

  loadEmployee() {
    this.loading = true;
    const id = this.route.snapshot.paramMap.get('id');
    
    if (!id) {
      this.router.navigateByUrl('/employees');
      return;
    }

    this.employeesSvc.getById(id).subscribe({
      next: (emp) => {
        this.employee = emp;
        this.loading = false;
        this.loadStats();
        this.loadHistory();
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.message ?? 'Failed to load employee';
        this.cdr.detectChanges();
      }
    });
  }

  loadStats() {
    if (!this.employee) return;
    this.employeesSvc.getStats(this.employee.id).subscribe({
      next: s => {
        this.stats = s;
        this.cdr.detectChanges();
      },
      error: () => { /* ignore for now */ }
    });
  }

  loadHistory() {
    if (!this.employee) return;
    this.loadingHistory = true;
    this.employeesSvc.getHistory(this.employee.id, {
      type: this.filters.type === 'all' ? undefined : this.filters.type,
      from: this.filters.from || undefined,
      to: this.filters.to || undefined,
      page: this.page,
      size: this.size
    }).subscribe({
      next: (res) => {
        this.history = res;
        this.loadingHistory = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.error = err?.error?.message ?? 'Failed to load history';
        this.loadingHistory = false;
        this.cdr.detectChanges();
      }
    });
  }

  onFilterChange() {
    this.page = 1;
    this.loadHistory();
  }

  onPageChange(delta: number) {
    const next = this.page + delta;
    if (next < 1) return;
    if (this.history && next > Math.max(1, Math.ceil(this.history.total / this.size))) return;
    this.page = next;
    this.loadHistory();
  }

  exportCsv() {
    if (!this.history?.data) return;
    const rows = this.history.data;
    const header = ['Type','Title','CaseType','Status','Outcome','Date','Description'];
    const csv = [header.join(',')].concat(
      rows.map(r => [r.kind, r.title, r.caseType, r.status, r.outcome ?? '', r.date, (r.description ?? '').replace(/,/g, ' ')]
        .map(v => `"${v ?? ''}"`).join(','))
    ).join('\n');

    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `employee-history-${this.employee?.employeeId}.csv`;
    link.click();
    URL.revokeObjectURL(url);
  }

  goBack() {
    window.history.back();
  }

  navigateToInvestigation(id: string | undefined) {
    if (!id) return;
    this.router.navigateByUrl(`/cases/${id}`);
  }
}
