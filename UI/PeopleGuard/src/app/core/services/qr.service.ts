import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface GenerateQrRequest {
  targetType: string;
  targetId: string;
  label?: string;
}

export interface QrTokenResponse {
  tokenId: string;
  token: string;
  qrImageUrl: string;
  expiresAt: string;
}

export interface QrSubmissionRequest {
  token: string;
  category: string;
  message: string;
  submitterName?: string;
  submitterEmail?: string;
  submitterPhone?: string;
  attachmentUrls?: string[];
}

export interface QrSubmissionResponse {
  submissionId: string;
  caseId?: string;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class QrService {
  private readonly baseUrl = `${environment.apiBaseUrl}/Qr`;
  constructor(private http: HttpClient) {}

  generateToken(req: GenerateQrRequest) {
    return this.http.post<QrTokenResponse>(`${this.baseUrl}/generate`, req);
  }

  getToken(tokenId: string) {
    return this.http.get<QrTokenResponse>(`${this.baseUrl}/${tokenId}`);
  }

  getQrImage(tokenId: string) {
    return `${this.baseUrl}/${tokenId}/image`;
  }

  listTokens(page = 1, size = 20) {
    return this.http.get<{ data: any[]; page: number; size: number; total: number; totalPages: number }>
      (`${this.baseUrl}?page=${page}&size=${size}`);
  }

  submitComplaint(req: QrSubmissionRequest) {
    return this.http.post<QrSubmissionResponse>(`${this.baseUrl}/submit`, req);
  }
}
