import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { map } from 'rxjs/operators';

export interface WarningLetterDto {
  id: string;
  investigationId: string;
  employeeId: string;
  employeeName?: string;
  outcome: number;
  reason: string;
  letterContent: string;
  pdfPath: string;
  createdAt: string;
  issuedAt: string;
}

export interface CreateWarningLetterRequest {
  investigationId: string;
  employeeId: string;
  outcome: number;
  reason: string;
}

@Injectable({ providedIn: 'root' })
export class WarningLettersService {
  private readonly baseUrl = `${environment.apiBaseUrl}/WarningLetters`;
  constructor(private http: HttpClient) {}
  
  getAll() {
    return this.http.get<WarningLetterDto[] | { data: WarningLetterDto[] }>(`${this.baseUrl}`)
      .pipe(map(res => Array.isArray(res) ? res : (res?.data ?? [])));
  }
  
  create(req: CreateWarningLetterRequest) {
    return this.http.post<WarningLetterDto>(`${this.baseUrl}`, req);
  }
  getByInvestigationId(id: string) {
    return this.http.get<WarningLetterDto>(`${this.baseUrl}/by-investigation/${id}`);
  }
  downloadPdf(id: string) {
    return this.http.get(`${this.baseUrl}/${id}/pdf`, { responseType: 'blob' });
  }
}
