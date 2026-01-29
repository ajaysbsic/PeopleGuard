import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { of } from 'rxjs';

export interface EmployeeDto {
  id: string;
  employeeId: string;
  name: string;
  email?: string;
  phone?: string;
  department: string;
  factory: string;
  position?: string;
  designation: string;
  status: number;
}

export interface EmployeeStatsDto {
  totalCases: number;
  open: number;
  closed: number;
  verbalWarnings: number;
  writtenWarnings: number;
}

export interface EmployeeHistoryItem {
  id: string;
  investigationId?: string;
  kind: string;
  title: string;
  caseType: string;
  status: string;
  outcome?: string;
  date: string;
  description?: string;
}

export interface PagedResponse<T> {
  data: T[];
  page: number;
  size: number;
  total: number;
  totalPages: number;
}

@Injectable({ providedIn: 'root' })
export class EmployeesService {
  private readonly baseUrl = `${environment.apiBaseUrl}/Employees`;

  constructor(private http: HttpClient) {}
  
  getAll() { 
    return this.http.get<EmployeeDto[]>(this.baseUrl);
  }
  
  getById(id: string) { return this.http.get<EmployeeDto>(`${this.baseUrl}/${id}`); }
  
  /**
   * Search employees by query (ID or Name)
   * GET /api/employees?query=xxx
   */
  search(query: string) {
    const params = new HttpParams().set('query', query);
    return this.http.get<EmployeeDto[]>(`${this.baseUrl}`, { params });
  }

  getStats(id: string) {
    return this.http.get<EmployeeStatsDto>(`${this.baseUrl}/${id}/stats`);
  }

  getHistory(id: string, opts: { type?: string; from?: string; to?: string; page?: number; size?: number }) {
    let params = new HttpParams();
    if (opts.type) params = params.set('type', opts.type);
    if (opts.from) params = params.set('from', opts.from);
    if (opts.to) params = params.set('to', opts.to);
    if (opts.page) params = params.set('page', opts.page);
    if (opts.size) params = params.set('size', opts.size);
    return this.http.get<PagedResponse<EmployeeHistoryItem>>(`${this.baseUrl}/${id}/history`, { params });
  }
  
  create(data: any) { return this.http.post<EmployeeDto>(`${this.baseUrl}`, data); }
  
  update(id: string, data: any) { return this.http.put<EmployeeDto>(`${this.baseUrl}/${id}`, data); }
  
  delete(id: string) { return this.http.delete<void>(`${this.baseUrl}/${id}`); }
}
