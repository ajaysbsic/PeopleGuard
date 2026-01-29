import { Component, Injector, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

export interface EmployeeFormData {
  id?: string;
  employeeId: string;
  name: string;
  email: string;
  phone: string;
  department: string;
  factory: string;
  designation: string;
}

@Component({
  selector: 'pg-employee-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div *ngIf="isOpen()" class="modal-overlay" (click)="closeModal()">
      <div class="modal" (click)="$event.stopPropagation()">
        <div class="modal-header">
          <h2>{{ isEditMode ? 'Edit Employee' : 'Add Employee' }}</h2>
          <button class="close-btn" (click)="closeModal()">✕</button>
        </div>
        
        <form [formGroup]="form" (ngSubmit)="submit()" class="modal-body">
          <div class="form-group">
            <label>Employee ID</label>
            <input type="text" formControlName="employeeId" [readonly]="isEditMode" [class.readonly]="isEditMode" />
          </div>
          
          <div class="form-group">
            <label>Name</label>
            <input type="text" formControlName="name" />
          </div>
          
          <div class="form-group">
            <label>Email</label>
            <input type="email" formControlName="email" />
          </div>
          
          <div class="form-group">
            <label>Phone</label>
            <input type="tel" formControlName="phone" />
          </div>
          
          <div class="form-row">
            <div class="form-group">
              <label>Department</label>
              <input type="text" formControlName="department" />
            </div>
            <div class="form-group">
              <label>Factory</label>
              <input type="text" formControlName="factory" />
            </div>
          </div>
          
          <div class="form-group">
            <label>Position</label>
            <input type="text" formControlName="designation" />
          </div>
        </form>
        
        <div class="modal-footer">
          <button class="btn-cancel" (click)="closeModal()">Cancel</button>
          <button class="btn-submit" (click)="submit()" [disabled]="!form.valid">
            {{ isEditMode ? 'Update' : 'Add' }} Employee
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .modal-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0, 0, 0, 0.5);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1000;
    }

    .modal {
      background: var(--bg-primary);
      border-radius: 8px;
      box-shadow: var(--shadow-dark);
      width: 90%;
      max-width: 500px;
      display: flex;
      flex-direction: column;
    }

    .modal-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 20px;
      border-bottom: 1px solid var(--border-color);
      
      h2 {
        margin: 0;
        font-size: 18px;
        color: var(--text-primary);
      }
    }

    .close-btn {
      background: transparent;
      border: none;
      font-size: 24px;
      cursor: pointer;
      color: var(--text-secondary);
      
      &:hover {
        color: var(--text-primary);
      }
    }

    .modal-body {
      padding: 20px;
      overflow-y: auto;
      max-height: 60vh;
    }

    .form-group {
      margin-bottom: 16px;
      
      label {
        display: block;
        font-size: 14px;
        font-weight: 500;
        margin-bottom: 6px;
        color: var(--text-primary);
      }
      
      input {
        width: 100%;
        padding: 10px;
        border: 1px solid var(--border-color);
        border-radius: 4px;
        background: var(--bg-secondary);
        color: var(--text-primary);
        font-size: 14px;
        transition: border-color 0.2s;
        
        &:focus {
          outline: none;
          border-color: var(--color-secondary);
        }
        
        &.readonly {
          opacity: 0.6;
          cursor: not-allowed;
          background: var(--bg-tertiary);
        }
      }
    }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 12px;
    }

    .modal-footer {
      display: flex;
      gap: 12px;
      padding: 20px;
      border-top: 1px solid var(--border-color);
      justify-content: flex-end;
    }

    .btn-cancel,
    .btn-submit {
      padding: 10px 20px;
      border-radius: 4px;
      border: none;
      cursor: pointer;
      font-size: 14px;
      font-weight: 500;
      transition: all 0.2s;
    }

    .btn-cancel {
      background: var(--bg-secondary);
      color: var(--text-primary);
      border: 1px solid var(--border-color);
      
      &:hover {
        background: var(--bg-tertiary);
      }
    }

    .btn-submit {
      background: var(--color-secondary);
      color: #fff;
      
      &:hover:not(:disabled) {
        opacity: 0.9;
      }
      
      &:disabled {
        opacity: 0.5;
        cursor: not-allowed;
      }
    }
  `]
})
export class EmployeeModalComponent {
  isOpen = input(false);
  onClose = output<void>();
  onSubmit = output<EmployeeFormData>();

  form!: FormGroup;
  isEditMode = false;

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      id: [''],
      employeeId: ['', Validators.required],
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', Validators.required],
      department: ['', Validators.required],
      factory: ['', Validators.required],
      designation: ['', Validators.required]
    });
  }

  closeModal() {
    this.onClose.emit();
  }

  submit() {
    if (this.form.valid) {
      this.onSubmit.emit(this.form.value);
    }
  }

  setData(data: Partial<EmployeeFormData>, editMode: boolean = false) {
    this.isEditMode = editMode;
    if (editMode) {
      const patch = {
        ...data,
        designation: (data as any).designation ?? (data as any).position
      };
      this.form.patchValue(patch);
    } else {
      this.form.reset();
    }
  }
}
