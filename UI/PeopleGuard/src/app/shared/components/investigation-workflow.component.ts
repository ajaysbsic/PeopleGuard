import { Component, input, output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

export interface InvestigationStatus {
  status: 'Open' | 'Under Investigation' | 'Closed';
  outcome?: 'No Action' | 'Verbal Warning' | 'Written Warning';
}

@Component({
  selector: 'pg-investigation-workflow',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="workflow-container">
      <div class="workflow-header">
        <h3>Investigation Workflow</h3>
      </div>

      <div class="status-section">
        <label>Current Status</label>
        <div class="status-badge" [class]="'status-' + (investigation()?.status || 'open').toLowerCase()">
          {{ investigation()?.status || 'Open' }}
        </div>
      </div>

      <div class="action-buttons">
        <button 
          *ngIf="investigation()?.status === 'Open'"
          class="btn btn-primary"
          (click)="updateStatus('Under Investigation')"
        >
          Start Investigation
        </button>
        
        <button
          *ngIf="investigation()?.status === 'Under Investigation'"
          class="btn btn-warning"
          (click)="showOutcomeForm = true"
        >
          Close Investigation
        </button>

        <button
          *ngIf="investigation()?.status === 'Closed'"
          class="btn btn-info"
          (click)="createWarningLetter()"
        >
          Create Warning Letter
        </button>
      </div>

      <div *ngIf="showOutcomeForm" class="outcome-form">
        <div class="form-group">
          <label>Investigation Outcome</label>
          <select [(ngModel)]="selectedOutcome" class="form-control">
            <option value="">Select outcome...</option>
            <option value="No Action">No Action</option>
            <option value="Verbal Warning">Verbal Warning</option>
            <option value="Written Warning">Written Warning</option>
          </select>
        </div>
        <div class="form-actions">
          <button class="btn btn-primary" (click)="closeInvestigation()">Close Case</button>
          <button class="btn btn-secondary" (click)="showOutcomeForm = false">Cancel</button>
        </div>
      </div>

      <div class="remarks-section">
        <h4>Investigation Remarks</h4>
        <div class="remarks-list">
          <div *ngFor="let remark of remarks()" class="remark-item">
            <div class="remark-header">
              <span class="remark-author">{{ remark.createdBy }}</span>
              <span class="remark-date">{{ remark.createdAt | date:'short' }}</span>
            </div>
            <div class="remark-text">{{ remark.text }}</div>
          </div>
        </div>

        <div class="add-remark">
          <textarea 
            [(ngModel)]="newRemark"
            placeholder="Add a remark..."
            class="remark-input"
          ></textarea>
          <button class="btn btn-sm btn-primary" (click)="addRemark()">Add Remark</button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .workflow-container {
      background: var(--bg-secondary);
      border-radius: 8px;
      padding: 20px;
      display: flex;
      flex-direction: column;
      gap: 20px;
    }

    .workflow-header h3 {
      margin: 0;
      color: var(--text-primary);
      font-size: 16px;
    }

    .status-section {
      display: flex;
      align-items: center;
      gap: 12px;
      
      label {
        font-weight: 500;
        color: var(--text-primary);
      }
    }

    .status-badge {
      padding: 6px 12px;
      border-radius: 20px;
      font-size: 12px;
      font-weight: 500;
      color: #fff;
      
      &.status-open {
        background: var(--color-info);
      }
      
      &.status-under\ investigation {
        background: var(--color-warning);
      }
      
      &.status-closed {
        background: var(--color-success);
      }
    }

    .action-buttons {
      display: flex;
      gap: 12px;
      flex-wrap: wrap;
    }

    .btn {
      padding: 10px 16px;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-size: 14px;
      font-weight: 500;
      transition: all 0.2s;
      
      &.btn-primary {
        background: var(--color-secondary);
        color: #fff;
        
        &:hover {
          opacity: 0.9;
        }
      }
      
      &.btn-warning {
        background: var(--color-warning);
        color: #fff;
        
        &:hover {
          opacity: 0.9;
        }
      }

      &.btn-info {
        background: #17a2b8;
        color: #fff;
        
        &:hover {
          opacity: 0.9;
        }
      }

      &.btn-secondary {
        background: var(--bg-tertiary);
        color: var(--text-primary);
        border: 1px solid var(--border-color);
        
        &:hover {
          background: var(--border-color);
        }
      }
      
      &.btn-sm {
        padding: 6px 12px;
        font-size: 12px;
      }
    }

    .outcome-form {
      background: var(--bg-tertiary);
      padding: 16px;
      border-radius: 4px;
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .form-group {
      display: flex;
      flex-direction: column;
      gap: 6px;
      
      label {
        font-size: 14px;
        font-weight: 500;
        color: var(--text-primary);
      }
    }

    .form-control {
      padding: 10px;
      border: 1px solid var(--border-color);
      border-radius: 4px;
      background: var(--bg-secondary);
      color: var(--text-primary);
      font-size: 14px;
      
      &:focus {
        outline: none;
        border-color: var(--color-secondary);
      }
    }

    .form-actions {
      display: flex;
      gap: 8px;
    }

    .remarks-section h4 {
      margin: 0 0 12px 0;
      font-size: 14px;
      color: var(--text-primary);
    }

    .remarks-list {
      display: flex;
      flex-direction: column;
      gap: 12px;
      max-height: 300px;
      overflow-y: auto;
      margin-bottom: 16px;
    }

    .remark-item {
      background: var(--bg-primary);
      padding: 12px;
      border-radius: 4px;
      border-left: 3px solid var(--color-secondary);
    }

    .remark-header {
      display: flex;
      justify-content: space-between;
      margin-bottom: 8px;
      font-size: 12px;
    }

    .remark-author {
      font-weight: 500;
      color: var(--text-primary);
    }

    .remark-date {
      color: var(--text-secondary);
    }

    .remark-text {
      font-size: 13px;
      color: var(--text-primary);
      line-height: 1.4;
    }

    .add-remark {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .remark-input {
      padding: 10px;
      border: 1px solid var(--border-color);
      border-radius: 4px;
      background: var(--bg-primary);
      color: var(--text-primary);
      font-size: 13px;
      resize: vertical;
      min-height: 60px;
      font-family: inherit;
      
      &:focus {
        outline: none;
        border-color: var(--color-secondary);
      }
    }
  `]
})
export class InvestigationWorkflowComponent {
  private router = inject(Router);

  investigation = input<any>();
  remarks = input<any[]>([]);
  onStatusChange = output<string>();
  onAddRemark = output<string>();
  onCloseCase = output<string>();

  showOutcomeForm = false;
  selectedOutcome = '';
  newRemark = '';

  updateStatus(newStatus: string) {
    this.onStatusChange.emit(newStatus);
  }

  addRemark() {
    if (this.newRemark.trim()) {
      this.onAddRemark.emit(this.newRemark);
      this.newRemark = '';
    }
  }

  closeInvestigation() {
    if (this.selectedOutcome) {
      this.onCloseCase.emit(this.selectedOutcome);
      this.showOutcomeForm = false;
      this.selectedOutcome = '';
    }
  }

  createWarningLetter() {
    const investigationId = this.investigation()?.id;
    if (investigationId) {
      this.router.navigateByUrl(`/warning-letters/create?investigationId=${investigationId}`);
    }
  }
}
