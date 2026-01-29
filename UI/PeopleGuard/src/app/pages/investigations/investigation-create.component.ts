import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { InvestigationsService } from '../../core/services/investigations.service';
import { EmployeesService, EmployeeDto } from '../../core/services/employees.service';

@Component({
  selector: 'pg-investigation-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './investigation-create.component.html',
  styleUrls: ['./investigation-create.component.scss']
})
export class InvestigationCreateComponent implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private investigationsSvc = inject(InvestigationsService);
  private employeesSvc = inject(EmployeesService);

  form!: FormGroup;
  employees: EmployeeDto[] = [];
  loading = false;
  loadingEmployees = true;
  submitted = false;

  caseTypes = [
    { value: 1, label: 'Violation' },
    { value: 2, label: 'Safety Issue' },
    { value: 3, label: 'Misbehavior' },
    { value: 4, label: 'Investigation' }
  ];

  ngOnInit() {
    this.initForm();
    this.loadEmployees();
  }

  initForm() {
    this.form = this.fb.group({
      employeeId: ['', Validators.required],
      title: ['', Validators.required],
      description: ['', Validators.required],
      caseType: [1, Validators.required]
    });
  }

  loadEmployees() {
    this.loadingEmployees = true;
    this.employeesSvc.getAll().subscribe({
      next: (emp) => {
        this.employees = emp;
        this.loadingEmployees = false;
      },
      error: () => {
        this.loadingEmployees = false;
        alert('Failed to load employees');
      }
    });
  }

  onSubmit() {
    this.submitted = true;

    if (this.form.invalid) {
      return;
    }

    this.loading = true;
    const payload = {
      ...this.form.value,
      caseType: parseInt(this.form.value.caseType),
      status: 0 // Open
    };

    this.investigationsSvc.create(payload).subscribe({
      next: (result) => {
        this.loading = false;
        alert('Investigation created successfully');
        this.router.navigateByUrl(`/investigations/${result.id}`);
      },
      error: (err) => {
        this.loading = false;
        console.error(err);
        alert('Failed to create investigation');
      }
    });
  }

  onCancel() {
    this.router.navigateByUrl('/investigations');
  }

  getEmployeeLabel(empId: string): string {
    const emp = this.employees.find(e => e.id === empId);
    return emp ? `${emp.name} (ID: ${emp.employeeId})` : '';
  }
}
