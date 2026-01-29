import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { WarningLettersService, WarningLetterDto } from '../../core/services/warning-letters.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'pg-warning-letters-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="warnings-container">
      <div class="header">
        <h2>Warning Letters</h2>
        <button 
          *ngIf="canCreateWarning" 
          class="btn btn-primary"
          [routerLink]="['/warning-letters/create']">
          + Create Warning Letter
        </button>
      </div>

      <div *ngIf="loading" class="loading-state">
        <p>Loading warning letters...</p>
      </div>

      <div *ngIf="!loading && items.length === 0" class="empty-state">
        <p>No warning letters found</p>
        <p *ngIf="canCreateWarning" class="hint">Create a new warning letter from an investigation</p>
      </div>

      <div *ngIf="!loading && items.length > 0" class="table-wrapper">
        <table class="warnings-table">
          <thead>
            <tr>
              <th>Employee</th>
              <th>Type</th>
              <th>Reason</th>
              <th>Created Date</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let letter of items" class="warning-row">
              <td class="employee">{{ letter.employeeName || 'Unknown' }}</td>
              <td>
                <span class="warning-type" [ngClass]="'type-' + letter.outcome">
                  {{ getOutcomeLabel(letter.outcome) }}
                </span>
              </td>
              <td class="reason">{{ (letter.reason || '') | slice:0:50 }}{{ (letter.reason?.length || 0) > 50 ? '...' : '' }}</td>
              <td class="date">{{ letter.createdAt | date: 'MMM d, yyyy' }}</td>
              <td class="actions">
                <button class="btn btn-sm btn-info" (click)="viewLetter(letter)">View</button>
                <button class="btn btn-sm btn-secondary" (click)="downloadPdf(letter)">Download PDF</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  `,
  styles: [`
    .warnings-container {
      padding: 24px;
    }

    .header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 24px;

      h2 {
        margin: 0;
        color: var(--text-primary);
      }
    }

    .btn {
      padding: 10px 16px;
      border-radius: 4px;
      border: none;
      cursor: pointer;
      font-weight: 500;
      transition: all 0.2s;
    }

    .btn-primary {
      background: var(--color-secondary);
      color: #fff;

      &:hover {
        opacity: 0.9;
      }
    }

    .btn-sm {
      padding: 6px 12px;
      font-size: 13px;
    }

    .btn-info {
      background: var(--color-info, #17a2b8);
      color: #fff;
    }

    .btn-secondary {
      background: var(--bg-tertiary);
      color: var(--text-primary);
      border: 1px solid var(--border-color);
    }

    .loading-state, .empty-state {
      text-align: center;
      padding: 48px 24px;
      color: var(--text-secondary);

      .hint {
        font-size: 14px;
        margin-top: 8px;
      }
    }

    .table-wrapper {
      overflow-x: auto;
      border-radius: 8px;
      border: 1px solid var(--border-color);
    }

    .warnings-table {
      width: 100%;
      border-collapse: collapse;

      thead {
        background: var(--bg-secondary);

        th {
          padding: 14px 16px;
          text-align: left;
          font-weight: 600;
          color: var(--text-primary);
          border-bottom: 2px solid var(--border-color);
        }
      }

      td {
        padding: 14px 16px;
        border-bottom: 1px solid var(--border-color);
        color: var(--text-primary);
      }

      .warning-row:hover {
        background: var(--bg-tertiary);
      }
    }

    .warning-type {
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 12px;
      font-weight: 500;

      &.type-1 {
        background: #d4edda;
        color: #155724;
      }

      &.type-2 {
        background: #fff3cd;
        color: #856404;
      }

      &.type-3 {
        background: #f8d7da;
        color: #721c24;
      }
    }

    .reason {
      max-width: 250px;
    }

    .actions {
      display: flex;
      gap: 8px;
    }
  `]
})
export class WarningLettersListComponent implements OnInit {
  private warningsSvc = inject(WarningLettersService);
  private auth = inject(AuthService);
  private cdr = inject(ChangeDetectorRef);

  items: WarningLetterDto[] = [];
  loading = true;
  canCreateWarning = false;

  outcomeLabels: { [key: number]: string } = {
    0: 'No Action',
    1: 'No Action',
    2: 'Verbal Warning',
    3: 'Written Warning'
  };

  ngOnInit() {
    this.loadWarningLetters();
    this.canCreateWarning = this.auth.user()?.roles?.includes('Admin') || 
                            this.auth.user()?.roles?.includes('ER') || 
                            this.auth.user()?.roles?.includes('HR') || 
                            this.auth.user()?.roles?.includes('Management') || 
                            this.auth.user()?.roles?.includes('Manager') || 
                            this.auth.user()?.roles?.includes('Business') || false;
  }

  loadWarningLetters() {
    this.loading = true;
    this.warningsSvc.getAll().subscribe({
      next: (data) => {
        this.items = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  getOutcomeLabel(outcome: number): string {
    return this.outcomeLabels[outcome] || 'Unknown';
  }

  viewLetter(letter: WarningLetterDto) {
    const win = window.open('', '_blank');
    if (!win) return;

    const content = letter.letterContent || `<p>${letter.reason || ''}</p>`;
    win.document.write(`
      <html>
        <head>
          <title>Warning Letter</title>
          <style>
            body { font-family: Arial, sans-serif; margin: 24px; color: #1f2937; }
            .header { margin-bottom: 16px; }
            .meta { color: #6b7280; margin-bottom: 8px; }
            .content { margin-top: 16px; line-height: 1.6; }
          </style>
        </head>
        <body>
          <div class="header">
            <h2>Warning Letter</h2>
            <div class="meta">Employee: ${letter.employeeName || 'Unknown'}</div>
            <div class="meta">Type: ${this.getOutcomeLabel(letter.outcome)}</div>
            <div class="meta">Issued: ${new Date(letter.issuedAt || letter.createdAt).toLocaleDateString()}</div>
          </div>
          <div class="content">${content}</div>
        </body>
      </html>
    `);
    win.document.close();
  }

  downloadPdf(letter: WarningLetterDto) {
    this.warningsSvc.downloadPdf(letter.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `warning-letter-${letter.id}.pdf`;
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: () => alert('Failed to download PDF')
    });
  }
}
