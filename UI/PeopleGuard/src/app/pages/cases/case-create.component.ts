import { Component, OnInit, inject, signal, computed, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { debounceTime, distinctUntilChanged, Subject, forkJoin } from 'rxjs';
import { InvestigationsService } from '../../core/services/investigations.service';
import { FileUploadService, UploadProgress, FILE_UPLOAD_CONFIG } from '../../core/services/file-upload.service';
import { TranslatePipe } from '../../core/pipes/translate.pipe';

// Employee interface for master data
export interface EmployeeMaster {
  employeeId: string;
  name: string;
  nameAr: string;
  department: string;
  departmentAr: string;
  factory: string;
  factoryAr: string;
  designation: string;
  designationAr: string;
  email: string;
  phone: string;
}

// Attachment tracking
interface AttachmentFile {
  id: string;
  file: File;
  progress: UploadProgress;
  uploadedFileId?: string;
}

@Component({
  selector: 'pg-case-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, TranslatePipe],
  templateUrl: './case-create.component.html',
  styleUrls: ['./case-create.component.scss']
})
export class CaseCreateComponent implements OnInit {
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;
  @ViewChild('searchInput') searchInput!: ElementRef<HTMLInputElement>;

  private fb = inject(FormBuilder);
  private router = inject(Router);
  private http = inject(HttpClient);
  private investigationsSvc = inject(InvestigationsService);
  private fileUploadSvc = inject(FileUploadService);

  // Form state
  form!: FormGroup;
  submitted = false;
  loading = false;

  // Employee search
  allEmployees: EmployeeMaster[] = [];
  filteredEmployees = signal<EmployeeMaster[]>([]);
  selectedEmployee = signal<EmployeeMaster | null>(null);
  searchQuery = signal('');
  showDropdown = signal(false);
  highlightedIndex = signal(-1);
  loadingEmployees = signal(true);

  private searchSubject = new Subject<string>();

  // Case types
  caseTypes = [
    { value: 1, labelEn: 'Violation', labelAr: 'مخالفة' },
    { value: 2, labelEn: 'Safety Issue', labelAr: 'مشكلة سلامة' },
    { value: 3, labelEn: 'Misbehavior', labelAr: 'سوء سلوك' },
    { value: 4, labelEn: 'Investigation', labelAr: 'تحقيق' }
  ];

  // File upload
  attachments = signal<AttachmentFile[]>([]);
  maxFiles = FILE_UPLOAD_CONFIG.maxFiles;
  maxFileSizeMb = FILE_UPLOAD_CONFIG.maxSizeBytes / (1024 * 1024);
  uploadError = signal<string | null>(null);

  // Computed values
  canAddMoreFiles = computed(() => this.attachments().length < this.maxFiles);
  totalUploadSize = computed(() => {
    return this.attachments().reduce((sum, a) => sum + a.file.size, 0);
  });
  allUploadsComplete = computed(() => {
    const files = this.attachments();
    return files.length === 0 || files.every(a => a.progress.status === 'complete');
  });

  // Validation constants
  readonly SUMMARY_MIN_LENGTH = 20;
  readonly SUMMARY_MAX_LENGTH = 5000;

  ngOnInit() {
    this.initForm();
    this.loadEmployeeMaster();
    this.setupSearch();
  }

  initForm() {
    this.form = this.fb.group({
      employeeId: ['', Validators.required],
      employeeName: ['', Validators.required],
      factory: ['', Validators.required],
      department: [''],
      caseType: [1, Validators.required],
      summary: ['', [
        Validators.required,
        Validators.minLength(this.SUMMARY_MIN_LENGTH),
        Validators.maxLength(this.SUMMARY_MAX_LENGTH)
      ]]
    });
  }

  loadEmployeeMaster() {
    this.loadingEmployees.set(true);
    this.http.get<EmployeeMaster[]>('/assets/data/employee_master.json').subscribe({
      next: (employees) => {
        this.allEmployees = employees;
        this.loadingEmployees.set(false);
      },
      error: () => {
        this.loadingEmployees.set(false);
        console.error('Failed to load employee master data');
      }
    });
  }

  setupSearch() {
    this.searchSubject.pipe(
      debounceTime(200),
      distinctUntilChanged()
    ).subscribe(query => {
      this.filterEmployees(query);
    });
  }

  // Employee autocomplete methods
  onSearchInput(event: Event) {
    const query = (event.target as HTMLInputElement).value;
    this.searchQuery.set(query);
    this.searchSubject.next(query);
    this.showDropdown.set(true);
    this.highlightedIndex.set(-1);
  }

  filterEmployees(query: string) {
    if (!query || query.length < 2) {
      this.filteredEmployees.set([]);
      return;
    }

    const lowerQuery = query.toLowerCase();
    const filtered = this.allEmployees.filter(emp =>
      emp.employeeId.toLowerCase().includes(lowerQuery) ||
      emp.name.toLowerCase().includes(lowerQuery) ||
      emp.nameAr.includes(query)
    ).slice(0, 10); // Limit results

    this.filteredEmployees.set(filtered);
  }

  selectEmployee(emp: EmployeeMaster) {
    this.selectedEmployee.set(emp);
    this.searchQuery.set(`${emp.employeeId} - ${emp.name}`);
    this.showDropdown.set(false);

    // Update form values
    this.form.patchValue({
      employeeId: emp.employeeId,
      employeeName: emp.name,
      factory: emp.factory,
      department: emp.department
    });
  }

  clearEmployee() {
    this.selectedEmployee.set(null);
    this.searchQuery.set('');
    this.form.patchValue({
      employeeId: '',
      employeeName: '',
      factory: '',
      department: ''
    });
    this.searchInput?.nativeElement?.focus();
  }

  // Keyboard navigation for dropdown
  onSearchKeydown(event: KeyboardEvent) {
    const employees = this.filteredEmployees();

    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault();
        if (this.highlightedIndex() < employees.length - 1) {
          this.highlightedIndex.update(i => i + 1);
        }
        break;
      case 'ArrowUp':
        event.preventDefault();
        if (this.highlightedIndex() > 0) {
          this.highlightedIndex.update(i => i - 1);
        }
        break;
      case 'Enter':
        event.preventDefault();
        if (this.highlightedIndex() >= 0 && employees[this.highlightedIndex()]) {
          this.selectEmployee(employees[this.highlightedIndex()]);
        }
        break;
      case 'Escape':
        this.showDropdown.set(false);
        break;
    }
  }

  onSearchBlur() {
    // Delay to allow click on dropdown item
    setTimeout(() => this.showDropdown.set(false), 200);
  }

  onSearchFocus() {
    if (this.searchQuery().length >= 2) {
      this.showDropdown.set(true);
    }
  }

  // File upload methods
  triggerFileInput() {
    this.fileInput.nativeElement.click();
  }

  onFilesSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files) return;

    this.uploadError.set(null);
    const files = Array.from(input.files);

    // Check max files limit
    const currentCount = this.attachments().length;
    const availableSlots = this.maxFiles - currentCount;

    if (files.length > availableSlots) {
      this.uploadError.set(`You can only add ${availableSlots} more file(s). Maximum ${this.maxFiles} files allowed.`);
      input.value = '';
      return;
    }

    // Validate each file
    for (const file of files) {
      const validation = this.fileUploadSvc.validateFile(file);
      if (!validation.valid) {
        this.uploadError.set(`${file.name}: ${validation.error}`);
        input.value = '';
        return;
      }
    }

    // Start uploading files
    for (const file of files) {
      const attachment: AttachmentFile = {
        id: crypto.randomUUID(),
        file,
        progress: { fileName: file.name, progress: 0, status: 'pending' }
      };

      this.attachments.update(arr => [...arr, attachment]);
      this.uploadFile(attachment);
    }

    input.value = ''; // Reset input
  }

  uploadFile(attachment: AttachmentFile) {
    attachment.progress.status = 'uploading';

    this.fileUploadSvc.uploadFile(attachment.file).subscribe({
      next: (progress) => {
        this.attachments.update(arr =>
          arr.map(a => a.id === attachment.id
            ? { ...a, progress, uploadedFileId: progress.result?.fileId }
            : a
          )
        );
      },
      error: (err) => {
        this.attachments.update(arr =>
          arr.map(a => a.id === attachment.id
            ? { ...a, progress: { ...a.progress, status: 'error', error: err.message } }
            : a
          )
        );
      }
    });
  }

  removeAttachment(id: string) {
    const attachment = this.attachments().find(a => a.id === id);
    if (attachment?.uploadedFileId) {
      // Delete from server
      this.fileUploadSvc.deleteFile(attachment.uploadedFileId).subscribe();
    }
    this.attachments.update(arr => arr.filter(a => a.id !== id));
  }

  retryUpload(id: string) {
    const attachment = this.attachments().find(a => a.id === id);
    if (attachment) {
      attachment.progress = { fileName: attachment.file.name, progress: 0, status: 'pending' };
      this.uploadFile(attachment);
    }
  }

  formatFileSize(bytes: number): string {
    return this.fileUploadSvc.formatFileSize(bytes);
  }

  // Drag and drop
  onDragOver(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();

    if (!event.dataTransfer?.files.length) return;
    if (!this.canAddMoreFiles()) {
      this.uploadError.set(`Maximum ${this.maxFiles} files allowed.`);
      return;
    }

    const mockEvent = { target: { files: event.dataTransfer.files } } as unknown as Event;
    this.onFilesSelected(mockEvent);
  }

  // Form submission
  onSubmit() {
    this.submitted = true;
    this.uploadError.set(null);

    if (this.form.invalid) {
      // Focus first invalid field
      const firstInvalid = document.querySelector('.ng-invalid[formControlName]');
      if (firstInvalid) {
        (firstInvalid as HTMLElement).focus();
      }
      return;
    }

    if (!this.allUploadsComplete()) {
      this.uploadError.set('Please wait for all uploads to complete.');
      return;
    }

    this.loading = true;

    // Collect attachment IDs
    const attachmentIds = this.attachments()
      .filter(a => a.uploadedFileId)
      .map(a => a.uploadedFileId);

    const payload = {
      employeeId: this.form.value.employeeId,
      title: `Case for ${this.form.value.employeeName}`,
      description: this.form.value.summary,
      caseType: parseInt(this.form.value.caseType),
      status: 0, // Open
      attachmentIds
    };

    this.investigationsSvc.create(payload).subscribe({
      next: (result) => {
        this.loading = false;
        this.router.navigateByUrl(`/cases/${result.id}`);
      },
      error: (err) => {
        this.loading = false;
        console.error(err);
        this.uploadError.set('Failed to create case. Please try again.');
      }
    });
  }

  onCancel() {
    // Clean up uploaded files
    this.attachments().forEach(a => {
      if (a.uploadedFileId) {
        this.fileUploadSvc.deleteFile(a.uploadedFileId).subscribe();
      }
    });
    this.router.navigateByUrl('/cases');
  }

  // Helper for summary character count
  get summaryLength(): number {
    return this.form.get('summary')?.value?.length || 0;
  }

  get summaryRemaining(): number {
    return this.SUMMARY_MAX_LENGTH - this.summaryLength;
  }
}
