import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpEventType, HttpRequest, HttpEvent } from '@angular/common/http';
import { Observable, Subject, throwError } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface FileUploadResult {
  fileId: string;
  url: string;
  fileName: string;
  size: number;
  contentType: string;
}

export interface UploadProgress {
  fileName: string;
  progress: number;
  status: 'pending' | 'uploading' | 'complete' | 'error';
  result?: FileUploadResult;
  error?: string;
}

// File validation constants
export const FILE_UPLOAD_CONFIG = {
  maxSizeBytes: 10 * 1024 * 1024, // 10MB
  maxFiles: 10,
  allowedTypes: [
    'image/jpeg',
    'image/png',
    'image/gif',
    'application/pdf',
    'application/msword',
    'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
    'application/vnd.ms-excel',
    'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
    'text/plain'
  ],
  allowedExtensions: ['.jpg', '.jpeg', '.png', '.gif', '.pdf', '.doc', '.docx', '.xls', '.xlsx', '.txt']
};

@Injectable({ providedIn: 'root' })
export class FileUploadService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/Files`;

  /**
   * Validate a file before upload
   */
  validateFile(file: File): { valid: boolean; error?: string } {
    // Check file size
    if (file.size > FILE_UPLOAD_CONFIG.maxSizeBytes) {
      const maxMb = FILE_UPLOAD_CONFIG.maxSizeBytes / (1024 * 1024);
      return { valid: false, error: `File size exceeds ${maxMb}MB limit` };
    }

    // Check file type
    const extension = '.' + file.name.split('.').pop()?.toLowerCase();
    if (!FILE_UPLOAD_CONFIG.allowedExtensions.includes(extension)) {
      return { valid: false, error: `File type not allowed: ${extension}` };
    }

    return { valid: true };
  }

  /**
   * Upload a single file with progress tracking
   */
  uploadFile(file: File): Observable<UploadProgress> {
    const validation = this.validateFile(file);
    if (!validation.valid) {
      return throwError(() => new Error(validation.error));
    }

    const formData = new FormData();
    formData.append('file', file, file.name);

    const req = new HttpRequest('POST', this.baseUrl, formData, {
      reportProgress: true
    });

    const progressSubject = new Subject<UploadProgress>();

    this.http.request(req).pipe(
      catchError(err => {
        progressSubject.next({
          fileName: file.name,
          progress: 0,
          status: 'error',
          error: err.message || 'Upload failed'
        });
        progressSubject.complete();
        return throwError(() => err);
      })
    ).subscribe({
      next: (event: HttpEvent<any>) => {
        if (event.type === HttpEventType.UploadProgress) {
          const progress = event.total ? Math.round((100 * event.loaded) / event.total) : 0;
          progressSubject.next({
            fileName: file.name,
            progress,
            status: 'uploading'
          });
        } else if (event.type === HttpEventType.Response) {
          progressSubject.next({
            fileName: file.name,
            progress: 100,
            status: 'complete',
            result: event.body as FileUploadResult
          });
          progressSubject.complete();
        }
      },
      error: (err) => {
        progressSubject.next({
          fileName: file.name,
          progress: 0,
          status: 'error',
          error: err.message || 'Upload failed'
        });
        progressSubject.complete();
      }
    });

    return progressSubject.asObservable();
  }

  /**
   * Upload multiple files
   */
  uploadFiles(files: File[]): Observable<UploadProgress>[] {
    return files.map(file => this.uploadFile(file));
  }

  /**
   * Delete an uploaded file
   */
  deleteFile(fileId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${fileId}`);
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
}
