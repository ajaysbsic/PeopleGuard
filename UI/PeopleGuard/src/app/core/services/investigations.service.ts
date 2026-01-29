import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface InvestigationDto {
  id: string;
  employeeId: string;
  employeeName?: string;
  title: string;
  description: string;
  caseType: number;
  status: number;
  outcome?: number;
  createdDate?: string;
}

@Injectable({ providedIn: 'root' })
export class InvestigationsService {
  private readonly baseUrl = `${environment.apiBaseUrl}/Investigations`;
  constructor(private http: HttpClient) {}
  
  getAll() { return this.http.get<InvestigationDto[]>(`${this.baseUrl}`); }
  
  getById(id: string) { return this.http.get<InvestigationDto>(`${this.baseUrl}/${id}`); }
  
  create(data: any) { return this.http.post<InvestigationDto>(`${this.baseUrl}`, data); }
  
  update(id: string, data: any) { return this.http.put<InvestigationDto>(`${this.baseUrl}/${id}`, data); }
  
  delete(id: string) { return this.http.delete<void>(`${this.baseUrl}/${id}`); }
  
  updateStatus(id: string, status: number) { return this.http.patch<InvestigationDto>(`${this.baseUrl}/${id}/status`, { status }); }
  
  addRemark(id: string, remark: string) { return this.http.post<void>(`${this.baseUrl}/${id}/remarks`, { content: remark }); }
}
