import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { LeavesService, LEAVE_TYPES, LeaveType, LeaveRequest } from '../../core/services/leaves.service';
import { FileUploadService, UploadProgress } from '../../core/services/file-upload.service';
import { TranslatePipe } from '../../core/pipes/translate.pipe';

interface AttachmentItem {
  fileName: string;
  progress: UploadProgress;
}

@Component({
  selector: 'pg-leave-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslatePipe],
  templateUrl: './leave-create.component.html',
  styleUrls: ['./leave-create.component.scss']
})
export class LeaveCreateComponent implements OnInit {
  private fb = inject(FormBuilder);
  private leavesService = inject(LeavesService);
  private fileUploadService = inject(FileUploadService);
  private router = inject(Router);

  form = this.fb.group({
    employeeId: ['', Validators.required],
    employeeName: ['', Validators.required],
    type: [1 as LeaveType, Validators.required],
    startDate: ['', Validators.required],
    endDate: ['', Validators.required],
    reason: [''],
    submitNow: [true]
  });

  submitted = signal(false);
  loading = signal(false);
  error = signal<string | null>(null);
  uploadError = signal<string | null>(null);

  attachments = signal<AttachmentItem[]>([]);
  uploadedFiles = signal<{ fileId: string; fileName: string; size: number; url: string }[]>([]);

  readonly leaveTypes = LEAVE_TYPES;

  ngOnInit(): void {}

  onFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files) return;

    this.uploadError.set(null);
    const files = Array.from(input.files);

    files.forEach(file => {
      const validation = this.fileUploadService.validateFile(file);
      if (!validation.valid) {
        this.uploadError.set(validation.error ?? 'File not allowed');
        return;
      }

      const upload$ = this.fileUploadService.uploadFile(file);
      const item: AttachmentItem = {
        fileName: file.name,
        progress: { fileName: file.name, progress: 0, status: 'pending' }
      };
      this.attachments.update(list => [...list, item]);

      upload$.subscribe({
        next: (progress) => {
          this.attachments.update(list => list.map(a => a.fileName === file.name ? { ...a, progress } : a));
          if (progress.status === 'complete' && progress.result) {
            this.uploadedFiles.update(list => [...list, {
              fileId: progress.result!.fileId,
              fileName: progress.result!.fileName,
              size: progress.result!.size,
              url: progress.result!.url
            }]);
          }
        },
        error: (err) => {
          this.uploadError.set(err.message || 'Upload failed');
          this.attachments.update(list => list.map(a => a.fileName === file.name ? { ...a, progress: { ...a.progress, status: 'error', error: err.message } } : a));
        }
      });
    });

    // Clear input to allow re-selection of the same file
    input.value = '';
  }

  removeAttachment(fileId: string): void {
    this.uploadedFiles.update(list => list.filter(f => f.fileId !== fileId));
    this.attachments.update(list => list.filter(a => a.progress.result?.fileId !== fileId));
  }

  submit(): void {
    this.submitted.set(true);
    this.error.set(null);

    if (this.form.invalid) {
      this.error.set('Please fill all required fields.');
      return;
    }

    const value = this.form.value;
    const start = new Date(value.startDate!);
    const end = new Date(value.endDate!);
    if (start > end) {
      this.error.set('Start date cannot be after end date.');
      return;
    }

    const type = value.type as LeaveType;
    if (this.leavesService.requiresAttachment(type) && this.uploadedFiles().length === 0) {
      this.error.set('Attachments are required for this leave type.');
      return;
    }

    const payload = {
      employeeId: value.employeeId?.trim() ?? '',
      employeeName: value.employeeName?.trim() ?? '',
      type,
      startDate: value.startDate!,
      endDate: value.endDate!,
      reason: value.reason?.trim(),
      attachments: this.uploadedFiles().map(f => ({
        fileId: f.fileId,
        fileName: f.fileName,
        sizeBytes: f.size,
        url: f.url
      })),
      submit: value.submitNow ?? true
    };

    this.loading.set(true);
    this.leavesService.createLeave(payload).subscribe({
      next: (leave: LeaveRequest) => {
        this.loading.set(false);
        this.router.navigate(['/leaves', leave.leaveId]);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.message || 'Failed to create leave request');
      }
    });
  }
}
