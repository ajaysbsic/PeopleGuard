import { Component, OnInit, ViewChild, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { EmployeesService, EmployeeDto } from '../../core/services/employees.service';
import { AuthService } from '../../core/services/auth.service';
import { EmployeeModalComponent } from '../../shared/components/employee-modal.component';

@Component({
  selector: 'pg-employees-list',
  standalone: true,
  imports: [CommonModule, RouterLink, EmployeeModalComponent],
  templateUrl: './employees-list.component.html',
  styleUrls: ['./employees-list.component.scss']
})
export class EmployeesListComponent implements OnInit {
  @ViewChild(EmployeeModalComponent) modal!: EmployeeModalComponent;
  
  private svc = inject(EmployeesService);
  private auth = inject(AuthService);
  private cdr = inject(ChangeDetectorRef);
  
  items: EmployeeDto[] = [];
  loading = true;
  modalOpen = false;
  canEditEmployee = false;

  ngOnInit() {
    this.loadEmployees();
    this.canEditEmployee = this.auth.user()?.roles?.includes('Admin') || this.auth.user()?.roles?.includes('HR') ? true : false;
  }

  loadEmployees() {
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
        alert('Failed to load employees'); 
      }
    });
  }

  openAddModal() {
    this.modalOpen = true;
    setTimeout(() => this.modal?.setData({}, false), 0);
  }

  openEditModal(employee: EmployeeDto) {
    this.modalOpen = true;
    setTimeout(() => this.modal?.setData(employee as any, true), 0);
  }

  closeModal() {
    this.modalOpen = false;
  }

  onSaveEmployee(data: any) {
    if (this.modal.isEditMode) {
      this.svc.update(data.id, data).subscribe({
        next: () => {
          alert('Employee updated successfully');
          this.closeModal();
          this.loadEmployees();
        },
        error: () => alert('Failed to update employee')
      });
    } else {
      this.svc.create(data).subscribe({
        next: () => {
          alert('Employee added successfully');
          this.closeModal();
          this.loadEmployees();
        },
        error: () => alert('Failed to add employee')
      });
    }
  }

  deleteEmployee(id: string) {
    if (confirm('Are you sure you want to delete this employee?')) {
      this.svc.delete(id).subscribe({
        next: () => {
          alert('Employee deleted successfully');
          this.loadEmployees();
        },
        error: () => alert('Failed to delete employee')
      });
    }
  }
}
