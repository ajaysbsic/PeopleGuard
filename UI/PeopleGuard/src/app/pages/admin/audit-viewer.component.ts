import { Component, inject, signal, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuditService, AuditLogEntry, AuditResponse } from '../../core/services/audit.service';
import { JsonModalComponent } from '../../shared/components/json-modal.component';

export interface AuditFilterRequest {
  userName?: string;
  action?: string;
  entityType?: string;
  from?: string;
  to?: string;
  pageIndex?: number;
  pageSize?: number;
}

@Component({
  selector: 'pg-audit-viewer',
  standalone: true,
  imports: [CommonModule, FormsModule, JsonModalComponent],
  templateUrl: './audit-viewer.component.html',
  styleUrls: ['./audit-viewer.component.scss']
})
export class AuditViewerComponent implements OnInit {
  private auditService = inject(AuditService);
  private cdr = inject(ChangeDetectorRef);

  logs = signal<AuditLogEntry[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  
  // Pagination
  pageIndex = signal(0);
  pageSize = signal(10);
  totalRecords = signal(0);

  // Filters
  filters = signal({
    userName: '',
    action: '',
    entityType: '',
    startDate: '',
    endDate: ''
  });

  // Details modal
  selectedLog = signal<AuditLogEntry | null>(null);
  showDetailsModal = signal(false);

  // Action options (from backend)
  actionOptions = ['Create', 'Update', 'Delete', 'Export', 'Submit', 'Approve'];
  entityTypeOptions = ['Investigation', 'Employee', 'WarningLetter', 'QrToken', 'QrSubmission'];

  ngOnInit() {
    this.loadLogs();
  }

  loadLogs() {
    this.loading.set(true);
    this.error.set(null);

    const req: AuditFilterRequest = {
      ...this.filters(),
      pageIndex: this.pageIndex(),
      pageSize: this.pageSize(),
      from: this.filters().startDate || undefined,
      to: this.filters().endDate || undefined
    };

    this.auditService.getLogs(req).subscribe({
      next: (res: any) => {
        const data = Array.isArray(res) ? res : (res?.data ?? []);
        this.logs.set(data);
        this.totalRecords.set(Array.isArray(res) ? data.length : (res?.total ?? data.length));
        this.loading.set(false);
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.error.set('Failed to load audit logs');
        this.loading.set(false);
        this.cdr.detectChanges();
      }
    });
  }

  onFilterChange() {
    this.pageIndex.set(0); // Reset to first page
    this.loadLogs();
  }

  goToPage(page: number) {
    this.pageIndex.set(page);
    this.loadLogs();
  }

  onExport() {
    const req: AuditFilterRequest = {
      ...this.filters(),
      pageIndex: 0,
      pageSize: 50000 // Export all matching records (up to 50k)
    };

    this.auditService.export(req).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `audit-log-${new Date().toISOString()}.csv`;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        this.error.set('Failed to export logs');
      }
    });
  }

  showDetails(log: AuditLogEntry) {
    this.selectedLog.set(log);
    this.showDetailsModal.set(true);
  }

  closeDetails() {
    this.showDetailsModal.set(false);
    this.selectedLog.set(null);
  }

  get totalPages(): number {
    return Math.ceil(this.totalRecords() / this.pageSize());
  }

  get paginationPages(): number[] {
    const total = this.totalPages;
    const current = this.pageIndex();
    const pages: number[] = [];

    if (total <= 5) {
      for (let i = 0; i < total; i++) pages.push(i);
    } else {
      if (current > 2) pages.push(0);
      if (current > 3) pages.push(-1); // -1 indicates "..."

      const start = Math.max(0, current - 2);
      const end = Math.min(total, current + 3);
      for (let i = start; i < end; i++) pages.push(i);

      if (current < total - 4) pages.push(-1);
      if (current < total - 2) pages.push(total - 1);
    }

    return pages;
  }
}
