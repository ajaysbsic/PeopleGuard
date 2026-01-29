import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface CaseListItem {
  id: string;
  caseId: string;
  employeeId: string;
  employeeName: string;
  employeeCode: string;
  factory: string;
  department: string;
  caseType: number;
  caseTypeName: string;
  status: number;
  statusName: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CaseDetail {
  id: string;
  caseId: string;
  employeeId: string;
  employeeName: string;
  employeeCode: string;
  factory: string;
  department: string;
  designation: string;
  title: string;
  description: string;
  caseType: number;
  caseTypeName: string;
  status: number;
  statusName: string;
  outcome?: number;
  outcomeName?: string;
  createdAt: string;
  closedAt?: string;
  remarksCount: number;
  attachmentsCount: number;
  hasWarningLetter: boolean;
  warningLetterId?: string;
}

export interface CaseHistoryEvent {
  id: string;
  eventType: number;
  eventTypeName: string;
  description: string;
  oldValue?: string;
  newValue?: string;
  referenceId?: string;
  userId?: string;
  userName?: string;
  createdAt: string;
}

export interface CaseRemark {
  id: string;
  text: string;
  userId: string;
  userName: string;
  createdAt: string;
}

export interface CaseAttachment {
  id: string;
  fileName: string;
  contentType: string;
  fileSize: number;
  uploadedAt: string;
  downloadUrl: string;
}

export interface PagedResponse<T> {
  data: T[];
  page: number;
  size: number;
  total: number;
  totalPages: number;
}

export interface CaseFilterParams {
  employeeId?: string;
  factory?: string;
  type?: number;
  status?: number;
  from?: string;
  to?: string;
  page?: number;
  size?: number;
  sortBy?: string;
  sortDesc?: boolean;
}

export interface CaseStats {
  total: number;
  open: number;
  underInvestigation: number;
  closed: number;
}

export interface UpdateStatusRequest {
  status: 'Open' | 'UnderInvestigation' | 'Closed';
  outcome?: number;
}

export interface AddRemarkRequest {
  text: string;
}

// Constants for case types and statuses
export const CASE_TYPES = [
  { value: 1, label: 'Violation', labelAr: 'مخالفة' },
  { value: 2, label: 'Safety Issue', labelAr: 'مشكلة سلامة' },
  { value: 3, label: 'Misbehavior', labelAr: 'سوء سلوك' },
  { value: 4, label: 'Investigation', labelAr: 'تحقيق' }
];

export const CASE_STATUSES = [
  { value: 1, label: 'Open', labelAr: 'مفتوح', color: 'status-open' },
  { value: 2, label: 'Under Investigation', labelAr: 'قيد التحقيق', color: 'status-under-investigation' },
  { value: 3, label: 'Closed', labelAr: 'مغلق', color: 'status-closed' }
];

export const OUTCOMES = [
  { value: 1, label: 'No Action', labelAr: 'لا إجراء' },
  { value: 2, label: 'Verbal Warning', labelAr: 'تحذير شفهي' },
  { value: 3, label: 'Written Warning', labelAr: 'تحذير كتابي' }
];

export const HISTORY_EVENT_TYPES: Record<number, { icon: string; color: string }> = {
  1: { icon: '📝', color: 'event-created' },
  2: { icon: '🔄', color: 'event-status' },
  3: { icon: '💬', color: 'event-remark' },
  4: { icon: '📎', color: 'event-attachment' },
  5: { icon: '🗑️', color: 'event-attachment-removed' },
  6: { icon: '📄', color: 'event-letter' },
  7: { icon: '⚖️', color: 'event-outcome' }
};

@Injectable({ providedIn: 'root' })
export class CasesService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/Cases`;

  /**
   * Get paginated list of cases with filters
   */
  getCases(filters: CaseFilterParams = {}): Observable<PagedResponse<CaseListItem>> {
    let params = new HttpParams();

    if (filters.employeeId) params = params.set('employeeId', filters.employeeId);
    if (filters.factory) params = params.set('factory', filters.factory);
    if (filters.type !== undefined) params = params.set('type', filters.type.toString());
    if (filters.status !== undefined) params = params.set('status', filters.status.toString());
    if (filters.from) params = params.set('from', filters.from);
    if (filters.to) params = params.set('to', filters.to);
    if (filters.page) params = params.set('page', filters.page.toString());
    if (filters.size) params = params.set('size', filters.size.toString());
    if (filters.sortBy) params = params.set('sortBy', filters.sortBy);
    if (filters.sortDesc !== undefined) params = params.set('sortDesc', filters.sortDesc.toString());

    return this.http.get<PagedResponse<CaseListItem>>(this.baseUrl, { params });
  }

  /**
   * Get case detail by ID
   */
  getCaseById(id: string): Observable<CaseDetail> {
    return this.http.get<CaseDetail>(`${this.baseUrl}/${id}`);
  }

  /**
   * Get case history/timeline
   */
  getCaseHistory(id: string): Observable<CaseHistoryEvent[]> {
    return this.http.get<CaseHistoryEvent[]>(`${this.baseUrl}/${id}/history`);
  }

  /**
   * Get case attachments
   */
  getCaseAttachments(id: string): Observable<CaseAttachment[]> {
    return this.http.get<CaseAttachment[]>(`${this.baseUrl}/${id}/attachments`);
  }

  /**
   * Get case remarks
   */
  getCaseRemarks(id: string): Observable<CaseRemark[]> {
    return this.http.get<CaseRemark[]>(`${this.baseUrl}/${id}/remarks`);
  }

  /**
   * Update case status
   */
  updateCaseStatus(id: string, request: UpdateStatusRequest): Observable<CaseDetail> {
    return this.http.patch<CaseDetail>(`${this.baseUrl}/${id}/status`, request);
  }

  /**
   * Add remark to case
   */
  addRemark(id: string, request: AddRemarkRequest): Observable<CaseRemark> {
    return this.http.post<CaseRemark>(`${this.baseUrl}/${id}/remarks`, request);
  }

  /**
   * Upload attachment to case
   */
  uploadAttachment(id: string, file: File): Observable<CaseAttachment> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<CaseAttachment>(`${this.baseUrl}/${id}/attachments`, formData);
  }

  /**
   * Delete attachment from case
   */
  deleteAttachment(caseId: string, attachmentId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${caseId}/attachments/${attachmentId}`);
  }

  /**
   * Get attachment download URL
   */
  getAttachmentDownloadUrl(caseId: string, attachmentId: string): string {
    return `${this.baseUrl}/${caseId}/attachments/${attachmentId}/download`;
  }

  /**
   * Get available factories for filter dropdown
   */
  getFactories(): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/factories`);
  }

  /**
   * Get case statistics
   */
  getStats(): Observable<CaseStats> {
    return this.http.get<CaseStats>(`${this.baseUrl}/stats`);
  }

  /**
   * Get case type label
   */
  getCaseTypeLabel(type: number): string {
    return CASE_TYPES.find(t => t.value === type)?.label ?? 'Unknown';
  }

  /**
   * Get status label
   */
  getStatusLabel(status: number): string {
    return CASE_STATUSES.find(s => s.value === status)?.label ?? 'Unknown';
  }

  /**
   * Get status CSS class
   */
  getStatusClass(status: number): string {
    return CASE_STATUSES.find(s => s.value === status)?.color ?? '';
  }

  /**
   * Get outcome label
   */
  getOutcomeLabel(outcome?: number): string {
    if (!outcome) return '';
    return OUTCOMES.find(o => o.value === outcome)?.label ?? 'Unknown';
  }

  /**
   * Get history event icon and color
   */
  getHistoryEventStyle(eventType: number): { icon: string; color: string } {
    return HISTORY_EVENT_TYPES[eventType] ?? { icon: '📌', color: 'event-default' };
  }

  /**
   * Format file size for display
   */
  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  /**
   * Check if status transition is valid
   */
  canTransitionTo(currentStatus: number, targetStatus: number, hasOutcome: boolean): boolean {
    if (currentStatus === targetStatus) return false;
    
    // Open -> UnderInvestigation: always allowed
    if (currentStatus === 1 && targetStatus === 2) return true;
    
    // Open -> Closed: only if outcome is set
    if (currentStatus === 1 && targetStatus === 3) return hasOutcome;
    
    // UnderInvestigation -> Closed: always allowed
    if (currentStatus === 2 && targetStatus === 3) return true;
    
    // UnderInvestigation -> Open: reopen allowed
    if (currentStatus === 2 && targetStatus === 1) return true;
    
    // Closed -> Open: reopen allowed
    if (currentStatus === 3 && targetStatus === 1) return true;
    
    return false;
  }
}
