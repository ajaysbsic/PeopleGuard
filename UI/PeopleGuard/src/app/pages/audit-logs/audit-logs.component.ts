import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuditService, AuditLogEntry } from '../../core/services/audit.service';

interface AuditLog {
  id: string;
  action: string;
  userName: string;
  entityType: string;
  entityId: string;
  timestamp: string;
  description?: string;
}

@Component({
  selector: 'pg-audit-logs',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="audit-logs-container">
      <div class="header">
        <h2>Audit Logs</h2>
      </div>

      <div *ngIf="loading" class="loading-state">
        <p>Loading audit logs...</p>
      </div>

      <div *ngIf="!loading && items.length === 0" class="empty-state">
        <p>No audit logs found</p>
      </div>

      <div *ngIf="!loading && items.length > 0" class="table-wrapper">
        <table class="audit-logs-table">
          <thead>
            <tr>
              <th>Timestamp</th>
              <th>User</th>
              <th>Action</th>
              <th>Entity</th>
              <th>Description</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let log of items" class="log-row">
              <td class="timestamp">{{ log.timestamp | date: 'MMM d, yyyy HH:mm:ss' }}</td>
              <td class="user">{{ log.userName }}</td>
              <td class="action">{{ log.action }}</td>
              <td class="entity">{{ log.entityType }}</td>
              <td class="description">{{ log.description }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  `,
  styles: [`
    .audit-logs-container {
      padding: 20px;
    }

    .header {
      margin-bottom: 20px;
    }

    .header h2 {
      color: var(--primary-color);
      margin: 0;
    }

    .loading-state, .empty-state {
      text-align: center;
      padding: 40px 20px;
      color: var(--text-secondary);
    }

    .table-wrapper {
      overflow-x: auto;
    }

    .audit-logs-table {
      width: 100%;
      border-collapse: collapse;
      background: var(--bg-secondary);
      border: 1px solid var(--border-color);
    }

    .audit-logs-table thead {
      background: var(--bg-tertiary);
    }

    .audit-logs-table th {
      padding: 12px;
      text-align: left;
      font-weight: 600;
      color: var(--text-primary);
      border-bottom: 2px solid var(--border-color);
    }

    .audit-logs-table td {
      padding: 12px;
      border-bottom: 1px solid var(--border-color);
      color: var(--text-primary);
    }

    .log-row:hover {
      background: var(--bg-tertiary);
    }

    .timestamp {
      font-size: 0.9em;
      color: var(--text-secondary);
    }

    .action {
      font-weight: 500;
      color: var(--primary-color);
    }
  `]
})
export class AuditLogsComponent implements OnInit {
  private auditService = inject(AuditService);
  private cdr = inject(ChangeDetectorRef);

  items: AuditLog[] = [];
  loading = true;

  ngOnInit() {
    this.loadAuditLogs();
  }

  loadAuditLogs() {
    this.loading = true;
    this.auditService.getLogs({ page: 1, size: 50 }).subscribe({
      next: (res: any) => {
        const data = Array.isArray(res) ? res : (res?.data ?? []);
        this.items = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to load audit logs:', err);
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }
}
