import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface AuditLogEntry {
  id: string;
  userName: string;
  entityType: string;
  entityId: string;
  action: string;
  endpoint: string;
  httpMethod: string;
  statusCode: number;
  ipAddress: string;
  timestamp: string;
  details?: string;
}

export interface AuditResponse {
  data: AuditLogEntry[];
  page: number;
  size: number;
  total: number;
  totalPages: number;
}

@Injectable({ providedIn: 'root' })
export class AuditService {
  private readonly baseUrl = `${environment.apiBaseUrl}/Audit`;
  constructor(private http: HttpClient) {}

  getLogs(filters: {
    userName?: string;
    action?: string;
    entityType?: string;
    from?: string;
    to?: string;
    page?: number;
    size?: number;
    pageIndex?: number;
    pageSize?: number;
  }) {
    let params = new HttpParams();
    if (filters.userName) params = params.set('userName', filters.userName);
    if (filters.action) params = params.set('action', filters.action);
    if (filters.entityType) params = params.set('entityType', filters.entityType);
    if (filters.from) params = params.set('from', filters.from);
    if (filters.to) params = params.set('to', filters.to);

    const page = filters.page ?? filters.pageIndex;
    const size = filters.size ?? filters.pageSize;
    if (page !== undefined) params = params.set('page', page.toString());
    if (size !== undefined) params = params.set('size', size.toString());

    return this.http.get<AuditResponse>(`${this.baseUrl}`, { params });
  }

  export(filters: {
    userName?: string;
    action?: string;
    entityType?: string;
    from?: string;
    to?: string;
  }) {
    let params = new HttpParams();
    if (filters.userName) params = params.set('userName', filters.userName);
    if (filters.action) params = params.set('action', filters.action);
    if (filters.entityType) params = params.set('entityType', filters.entityType);
    if (filters.from) params = params.set('from', filters.from);
    if (filters.to) params = params.set('to', filters.to);

    return this.http.get(`${this.baseUrl}/export`, { params, responseType: 'blob' });
  }
}
