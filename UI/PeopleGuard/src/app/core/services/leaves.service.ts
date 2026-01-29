import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export type LeaveType = 1 | 2 | 3; // Emergency, Sick, OutsideKSA
export type LeaveStatus = 1 | 2 | 3 | 4 | 5 | 6; // Draft, Submitted, UnderReview, Approved, Rejected, Cancelled

export interface FileAttachment {
  fileId: string;
  fileName: string;
  sizeBytes: number;
  url: string;
}

export interface LeaveRequest {
  leaveId: string;
  employeeId: string;
  employeeName: string;
  type: LeaveType;
  status: LeaveStatus;
  startDate: string;
  endDate: string;
  reason?: string;
  attachments: FileAttachment[];
  createdAt: string;
  createdBy: string;
  createdByName: string;
  reviewedAt?: string;
  reviewedBy?: string;
  reviewedByName?: string;
  reviewRemark?: string;
}

export interface LeaveCreateRequest {
  employeeId: string;
  employeeName: string;
  type: LeaveType;
  startDate: string; // yyyy-MM-dd
  endDate: string;   // yyyy-MM-dd
  reason?: string;
  attachments?: FileAttachment[];
  submit?: boolean;
}

export interface LeaveFilter {
  employeeId?: string;
  type?: LeaveType;
  status?: LeaveStatus;
  from?: string; // yyyy-MM-dd
  to?: string;   // yyyy-MM-dd
  page?: number;
  size?: number;
}

export interface LeaveReviewRequest {
  decision: 'StartReview' | 'Approve' | 'Reject';
  remark?: string;
}

export interface PagedResponse<T> {
  data: T[];
  page: number;
  size: number;
  total: number;
  totalPages: number;
}

export const LEAVE_TYPES: { value: LeaveType; label: string; labelAr: string; requiresAttachment: boolean }[] = [
  { value: 1, label: 'Emergency', labelAr: 'طارئة', requiresAttachment: false },
  { value: 2, label: 'Sick', labelAr: 'مرَضية', requiresAttachment: true },
  { value: 3, label: 'Outside KSA', labelAr: 'خارج السعودية', requiresAttachment: true }
];

export const LEAVE_STATUSES: { value: LeaveStatus; label: string; labelAr: string; color: string }[] = [
  { value: 1, label: 'Draft', labelAr: 'مسودة', color: 'muted' },
  { value: 2, label: 'Submitted', labelAr: 'مقدمة', color: 'info' },
  { value: 3, label: 'Under Review', labelAr: 'قيد المراجعة', color: 'warning' },
  { value: 4, label: 'Approved', labelAr: 'موافَق عليها', color: 'success' },
  { value: 5, label: 'Rejected', labelAr: 'مرفوضة', color: 'danger' },
  { value: 6, label: 'Cancelled', labelAr: 'ملغاة', color: 'muted' }
];

@Injectable({ providedIn: 'root' })
export class LeavesService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/Leaves`;

  getLeaves(filters: LeaveFilter = {}): Observable<PagedResponse<LeaveRequest>> {
    let params = new HttpParams();

    if (filters.employeeId) params = params.set('employeeId', filters.employeeId);
    if (filters.type !== undefined) params = params.set('type', filters.type.toString());
    if (filters.status !== undefined) params = params.set('status', filters.status.toString());
    if (filters.from) params = params.set('from', filters.from);
    if (filters.to) params = params.set('to', filters.to);
    if (filters.page) params = params.set('page', filters.page.toString());
    if (filters.size) params = params.set('size', filters.size.toString());

    return this.http.get<PagedResponse<LeaveRequest>>(this.baseUrl, { params });
  }

  getLeaveById(id: string): Observable<LeaveRequest> {
    return this.http.get<LeaveRequest>(`${this.baseUrl}/${id}`);
  }

  createLeave(payload: LeaveCreateRequest): Observable<LeaveRequest> {
    return this.http.post<LeaveRequest>(this.baseUrl, payload);
  }

  submitLeave(id: string): Observable<LeaveRequest> {
    return this.http.post<LeaveRequest>(`${this.baseUrl}/${id}/submit`, {});
  }

  startReview(id: string, remark?: string): Observable<LeaveRequest> {
    return this.reviewLeave(id, { decision: 'StartReview', remark });
  }

  reviewLeave(id: string, request: LeaveReviewRequest): Observable<LeaveRequest> {
    return this.http.patch<LeaveRequest>(`${this.baseUrl}/${id}/review`, request);
  }

  getTypeLabel(type: LeaveType): string {
    return LEAVE_TYPES.find(t => t.value === type)?.label ?? 'Unknown';
  }

  getStatusLabel(status: LeaveStatus): string {
    return LEAVE_STATUSES.find(s => s.value === status)?.label ?? 'Unknown';
  }

  getStatusClass(status: LeaveStatus): string {
    return `status-pill status-${LEAVE_STATUSES.find(s => s.value === status)?.color ?? 'muted'}`;
  }

  requiresAttachment(type: LeaveType): boolean {
    return !!LEAVE_TYPES.find(t => t.value === type)?.requiresAttachment;
  }
}
