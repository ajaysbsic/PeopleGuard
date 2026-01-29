import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { LeavesService, LeaveRequest, LEAVE_TYPES, LEAVE_STATUSES } from '../../core/services/leaves.service';
import { TranslatePipe } from '../../core/pipes/translate.pipe';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'pg-leaves-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, TranslatePipe],
  templateUrl: './leaves-list.component.html',
  styleUrls: ['./leaves-list.component.scss']
})
export class LeavesListComponent implements OnInit {
  private fb = inject(FormBuilder);
  leavesService = inject(LeavesService);
  private auth = inject(AuthService);

  filtersForm = this.fb.group({
    employeeId: [''],
    type: [''],
    status: [''],
    from: [''],
    to: ['']
  });

  leaves = signal<LeaveRequest[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  page = signal(1);
  size = signal(10);
  total = signal(0);

  readonly leaveTypes = LEAVE_TYPES;
  readonly leaveStatuses = LEAVE_STATUSES;

  totalPages = computed(() => {
    const pages = Math.ceil(this.total() / this.size());
    return Number.isFinite(pages) ? Math.max(pages, 1) : 1;
  });

  ngOnInit(): void {
    this.loadLeaves();
  }

  loadLeaves(): void {
    this.loading.set(true);
    this.error.set(null);

    const filters = this.filtersForm.value;
    this.leavesService.getLeaves({
      employeeId: filters.employeeId || undefined,
      type: filters.type ? Number(filters.type) as any : undefined,
      status: filters.status ? Number(filters.status) as any : undefined,
      from: filters.from || undefined,
      to: filters.to || undefined,
      page: this.page(),
      size: this.size()
    }).subscribe({
      next: (res) => {
        this.leaves.set(res.data || []);
        this.total.set(res.total || 0);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load leave requests');
        this.loading.set(false);
      }
    });
  }

  applyFilters(): void {
    this.page.set(1);
    this.loadLeaves();
  }

  clearFilters(): void {
    this.filtersForm.reset();
    this.applyFilters();
  }

  changePage(delta: number): void {
    const nextPage = this.page() + delta;
    if (nextPage < 1 || nextPage > this.totalPages()) return;
    this.page.set(nextPage);
    this.loadLeaves();
  }

  get canCreate(): boolean {
    return this.auth.hasRole('Admin');
  }

  get statusClass(): (status: number) => string {
    return (status) => this.leavesService.getStatusClass(status as any);
  }
}
