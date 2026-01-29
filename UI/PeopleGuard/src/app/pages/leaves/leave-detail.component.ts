import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { LeavesService, LeaveRequest, LEAVE_STATUSES } from '../../core/services/leaves.service';
import { TranslatePipe } from '../../core/pipes/translate.pipe';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'pg-leave-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './leave-detail.component.html',
  styleUrls: ['./leave-detail.component.scss']
})
export class LeaveDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  leavesService = inject(LeavesService);
  private auth = inject(AuthService);

  leave = signal<LeaveRequest | null>(null);
  loading = signal(false);
  error = signal<string | null>(null);
  reviewRemark = '';

  readonly statuses = LEAVE_STATUSES;

  get leaveId(): string {
    return this.route.snapshot.paramMap.get('id') ?? '';
  }

  isFinal = computed(() => {
    const status = this.leave()?.status;
    return status === 4 || status === 5 || status === 6;
  });

  canReview = computed(() => this.auth.hasRole('ER'));
  canSubmitDraft = computed(() => this.auth.hasRole('Admin') && this.leave()?.status === 1);
  canStartReview = computed(() => this.canReview() && this.leave()?.status === 2);
  canDecide = computed(() => this.canReview() && (this.leave()?.status === 2 || this.leave()?.status === 3));

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    if (!this.leaveId) {
      this.error.set('Missing leave id');
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.leavesService.getLeaveById(this.leaveId).subscribe({
      next: (res) => {
        this.leave.set(res);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to load leave');
        this.loading.set(false);
      }
    });
  }

  submitDraft(): void {
    if (!this.leave()) return;
    this.loading.set(true);
    this.leavesService.submitLeave(this.leaveId).subscribe({
      next: (res) => {
        this.leave.set(res);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to submit leave');
        this.loading.set(false);
      }
    });
  }

  startReview(): void {
    this.loading.set(true);
    this.leavesService.startReview(this.leaveId, this.reviewRemark).subscribe({
      next: (res) => {
        this.leave.set(res);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to move to review');
        this.loading.set(false);
      }
    });
  }

  approve(): void {
    this.decide('Approve');
  }

  reject(): void {
    this.decide('Reject');
  }

  private decide(decision: 'Approve' | 'Reject'): void {
    this.loading.set(true);
    this.leavesService.reviewLeave(this.leaveId, { decision, remark: this.reviewRemark }).subscribe({
      next: (res) => {
        this.leave.set(res);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to update leave');
        this.loading.set(false);
      }
    });
  }

  statusClass(status?: number): string {
    return this.leavesService.getStatusClass((status ?? 1) as any);
  }
}
