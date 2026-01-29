import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { 
  CasesService, 
  CaseDetail, 
  CaseHistoryEvent, 
  CaseAttachment, 
  CaseRemark,
  CASE_STATUSES,
  OUTCOMES
} from '../../core/services/cases.service';
import { AuthService } from '../../core/services/auth.service';
import { TranslatePipe } from '../../core/pipes/translate.pipe';
import { WarningLettersService, WarningLetterDto } from '../../core/services/warning-letters.service';

type TabId = 'overview' | 'investigation' | 'attachments' | 'history';

@Component({
  selector: 'app-case-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, TranslatePipe],
  templateUrl: './case-detail.component.html',
  styleUrls: ['./case-detail.component.scss']
})
export class CaseDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private casesService = inject(CasesService);
  private authService = inject(AuthService);
  private warningLettersService = inject(WarningLettersService);

  // Data signals
  caseDetail = signal<CaseDetail | null>(null);
  history = signal<CaseHistoryEvent[]>([]);
  attachments = signal<CaseAttachment[]>([]);
  remarks = signal<CaseRemark[]>([]);

  // Loading states
  loading = signal(true);
  loadingHistory = signal(false);
  loadingAttachments = signal(false);
  loadingRemarks = signal(false);
  loadingWarningLetter = signal(false);

  // UI state
  activeTab = signal<TabId>('overview');
  showStatusModal = signal(false);
  showRemarkModal = signal(false);
  actionInProgress = signal(false);
  error = signal<string | null>(null);

  // Form state
  newRemark = '';
  selectedStatus = '';
  selectedOutcome: number | undefined;

  // File upload
  uploadProgress = signal(0);
  uploading = signal(false);

  // Constants
  readonly statuses = CASE_STATUSES;
  readonly outcomes = OUTCOMES;
  readonly tabs: { id: TabId; label: string; icon: string }[] = [
    { id: 'overview', label: 'CASES.TAB_OVERVIEW', icon: '📋' },
    { id: 'investigation', label: 'CASES.TAB_INVESTIGATION', icon: '🔍' },
    { id: 'attachments', label: 'CASES.TAB_ATTACHMENTS', icon: '📎' },
    { id: 'history', label: 'CASES.TAB_HISTORY', icon: '📜' }
  ];

  // Computed
  canManage = computed(() => {
    const roles = this.authService.user()?.roles ?? [];
    return roles.some(r => ['Admin', 'ER', 'HR'].includes(r));
  });

  caseId = computed(() => this.route.snapshot.paramMap.get('id') ?? '');

  ngOnInit(): void {
    this.loadCaseDetail();
  }

  private loadCaseDetail(): void {
    const id = this.caseId();
    if (!id) {
      this.router.navigate(['/cases']);
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.casesService.getCaseById(id).subscribe({
      next: (detail) => {
        this.caseDetail.set(detail);
        this.loading.set(false);
        // Load additional data for active tab
        this.onTabChange(this.activeTab());
      },
      error: (err) => {
        console.error('Error loading case:', err);
        this.error.set(err.error?.message ?? 'Failed to load case');
        this.loading.set(false);
      }
    });
  }

  onTabChange(tab: TabId): void {
    this.activeTab.set(tab);

    // Load data for tab if not already loaded
    switch (tab) {
      case 'history':
        if (this.history().length === 0) {
          this.loadHistory();
        }
        break;
      case 'attachments':
        if (this.attachments().length === 0) {
          this.loadAttachments();
        }
        break;
      case 'investigation':
        if (this.remarks().length === 0) {
          this.loadRemarks();
        }
        break;
    }
  }

  private loadHistory(): void {
    const id = this.caseId();
    this.loadingHistory.set(true);
    
    this.casesService.getCaseHistory(id).subscribe({
      next: (events) => {
        this.history.set(events);
        this.loadingHistory.set(false);
      },
      error: () => this.loadingHistory.set(false)
    });
  }

  private loadAttachments(): void {
    const id = this.caseId();
    this.loadingAttachments.set(true);
    
    this.casesService.getCaseAttachments(id).subscribe({
      next: (files) => {
        this.attachments.set(files);
        this.loadingAttachments.set(false);
      },
      error: () => this.loadingAttachments.set(false)
    });
  }

  private loadRemarks(): void {
    const id = this.caseId();
    this.loadingRemarks.set(true);
    
    this.casesService.getCaseRemarks(id).subscribe({
      next: (items) => {
        this.remarks.set(items);
        this.loadingRemarks.set(false);
      },
      error: () => this.loadingRemarks.set(false)
    });
  }

  // Status management
  openStatusModal(): void {
    const current = this.caseDetail();
    if (current) {
      this.selectedStatus = '';
      this.selectedOutcome = current.outcome;
      this.showStatusModal.set(true);
    }
  }

  closeStatusModal(): void {
    this.showStatusModal.set(false);
    this.selectedStatus = '';
  }

  canTransitionTo(targetStatus: number): boolean {
    const current = this.caseDetail();
    if (!current) return false;
    const hasOutcome = !!current.outcome || !!this.selectedOutcome;
    return this.casesService.canTransitionTo(current.status, targetStatus, hasOutcome);
  }

  updateStatus(): void {
    if (!this.selectedStatus || this.actionInProgress()) return;

    const id = this.caseId();
    this.actionInProgress.set(true);

    this.casesService.updateCaseStatus(id, {
      status: this.selectedStatus as 'Open' | 'UnderInvestigation' | 'Closed',
      outcome: this.selectedOutcome
    }).subscribe({
      next: (updated) => {
        this.caseDetail.set(updated);
        this.closeStatusModal();
        this.actionInProgress.set(false);
        // Refresh history
        this.loadHistory();
      },
      error: (err) => {
        console.error('Error updating status:', err);
        this.error.set(err.error?.message ?? 'Failed to update status');
        this.actionInProgress.set(false);
      }
    });
  }

  // Remark management
  openRemarkModal(): void {
    this.newRemark = '';
    this.showRemarkModal.set(true);
  }

  closeRemarkModal(): void {
    this.showRemarkModal.set(false);
    this.newRemark = '';
  }

  addRemark(): void {
    if (!this.newRemark.trim() || this.actionInProgress()) return;

    const id = this.caseId();
    this.actionInProgress.set(true);

    this.casesService.addRemark(id, { text: this.newRemark }).subscribe({
      next: (remark) => {
        this.remarks.update(list => [remark, ...list]);
        const detail = this.caseDetail();
        if (detail) {
          this.caseDetail.set({ ...detail, remarksCount: detail.remarksCount + 1 });
        }
        this.closeRemarkModal();
        this.actionInProgress.set(false);
        // Refresh history
        this.loadHistory();
      },
      error: (err) => {
        console.error('Error adding remark:', err);
        this.error.set(err.error?.message ?? 'Failed to add remark');
        this.actionInProgress.set(false);
      }
    });
  }

  // File upload
  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;

    const file = input.files[0];
    this.uploadFile(file);
    input.value = ''; // Reset for re-upload
  }

  uploadFile(file: File): void {
    const id = this.caseId();
    this.uploading.set(true);
    this.uploadProgress.set(0);

    this.casesService.uploadAttachment(id, file).subscribe({
      next: (attachment) => {
        this.attachments.update(list => [attachment, ...list]);
        const detail = this.caseDetail();
        if (detail) {
          this.caseDetail.set({ ...detail, attachmentsCount: detail.attachmentsCount + 1 });
        }
        this.uploading.set(false);
        // Refresh history
        this.loadHistory();
      },
      error: (err) => {
        console.error('Error uploading file:', err);
        this.error.set(err.error?.message ?? 'Failed to upload file');
        this.uploading.set(false);
      }
    });
  }

  deleteAttachment(attachment: CaseAttachment): void {
    if (!confirm(`Delete ${attachment.fileName}?`)) return;

    const id = this.caseId();
    this.casesService.deleteAttachment(id, attachment.id).subscribe({
      next: () => {
        this.attachments.update(list => list.filter(a => a.id !== attachment.id));
        const detail = this.caseDetail();
        if (detail) {
          this.caseDetail.set({ ...detail, attachmentsCount: detail.attachmentsCount - 1 });
        }
        // Refresh history
        this.loadHistory();
      },
      error: (err) => {
        console.error('Error deleting attachment:', err);
        this.error.set(err.error?.message ?? 'Failed to delete attachment');
      }
    });
  }

  downloadAttachment(attachment: CaseAttachment): void {
    const url = this.casesService.getAttachmentDownloadUrl(this.caseId(), attachment.id);
    window.open(url, '_blank');
  }

  // Helpers
  getStatusClass(status: number): string {
    return this.casesService.getStatusClass(status);
  }

  getHistoryEventStyle(eventType: number): { icon: string; color: string } {
    return this.casesService.getHistoryEventStyle(eventType);
  }

  formatFileSize(bytes: number): string {
    return this.casesService.formatFileSize(bytes);
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  formatDateTime(dateStr: string): string {
    return new Date(dateStr).toLocaleString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  getFileIcon(contentType: string): string {
    if (contentType.includes('pdf')) return '📕';
    if (contentType.includes('word') || contentType.includes('doc')) return '📘';
    if (contentType.includes('excel') || contentType.includes('sheet')) return '📗';
    if (contentType.includes('image')) return '🖼️';
    return '📄';
  }

  goBack(): void {
    this.router.navigate(['/cases']);
  }

  viewWarningLetter(): void {
    const detail = this.caseDetail();
    if (!detail?.id) return;

    this.loadingWarningLetter.set(true);
    this.warningLettersService.getByInvestigationId(detail.id).subscribe({
      next: (letter) => {
        this.loadingWarningLetter.set(false);
        this.openWarningLetterWindow(letter);
      },
      error: () => {
        this.loadingWarningLetter.set(false);
        this.error.set('Failed to load warning letter');
      }
    });
  }

  private openWarningLetterWindow(letter: WarningLetterDto): void {
    const win = window.open('', '_blank');
    if (!win) return;

    const content = letter.letterContent || `<p>${letter.reason || ''}</p>`;
    const outcomeLabel = this.outcomes.find(o => o.value === letter.outcome)?.label ?? 'Unknown';
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
            <div class="meta">Type: ${outcomeLabel}</div>
            <div class="meta">Issued: ${new Date(letter.issuedAt || letter.createdAt).toLocaleDateString()}</div>
          </div>
          <div class="content">${content}</div>
        </body>
      </html>
    `);
    win.document.close();
  }
}
