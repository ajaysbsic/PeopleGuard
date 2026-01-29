import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { of } from 'rxjs';

export interface ChartDataDto { label: string; value: number; }
export interface TrendDataDto { label: string; value: number; }
export interface TopViolatorDto { employeeName: string; count: number; }
export interface RecentInvestigationDto { id: string; title: string; createdAt: string; }
export interface DashboardDto {
  totalViolations: number;
  activeInvestigations: number;
  pendingWarnings: number;
  totalEmployees: number;
  employeesWithViolations: number;
  violationsByFactory: ChartDataDto[];
  violationsByDepartment: ChartDataDto[];
  violationsByType: ChartDataDto[];
  violationsByOutcome: ChartDataDto[];
  violationsTrend: TrendDataDto[];
  topViolators: TopViolatorDto[];
  recentInvestigations: RecentInvestigationDto[];
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly baseUrl = `${environment.apiBaseUrl}/Dashboard`;
  
  // MOCK DASHBOARD DATA FOR TESTING
  private mockDashboard: DashboardDto = {
    totalViolations: 24,
    activeInvestigations: 8,
    pendingWarnings: 5,
    totalEmployees: 127,
    employeesWithViolations: 18,
    violationsByFactory: [
      { label: 'Main Factory', value: 16 },
      { label: 'Secondary Factory', value: 8 }
    ],
    violationsByDepartment: [
      { label: 'Operations', value: 12 },
      { label: 'Finance', value: 4 },
      { label: 'HR', value: 3 },
      { label: 'IT', value: 5 }
    ],
    violationsByType: [
      { label: 'Attendance', value: 10 },
      { label: 'Safety', value: 7 },
      { label: 'Performance', value: 4 },
      { label: 'Conduct', value: 3 }
    ],
    violationsByOutcome: [
      { label: 'Verbal Warning', value: 8 },
      { label: 'Written Warning', value: 10 },
      { label: 'Suspension', value: 4 },
      { label: 'Pending', value: 2 }
    ],
    violationsTrend: [
      { label: 'Jan', value: 5 },
      { label: 'Feb', value: 7 },
      { label: 'Mar', value: 6 },
      { label: 'Apr', value: 8 },
      { label: 'May', value: 12 },
      { label: 'Jun', value: 10 }
    ],
    topViolators: [
      { employeeName: 'Ahmed Al Mansouri (EMP001)', count: 4 },
      { employeeName: 'Fatima Al Kaabi (EMP002)', count: 3 },
      { employeeName: 'Mohammed Al Rashid (EMP003)', count: 2 }
    ],
    recentInvestigations: [
      { id: '1', title: 'Investigation - Attendance Issue', createdAt: '2026-01-25' },
      { id: '2', title: 'Investigation - Safety Violation', createdAt: '2026-01-24' },
      { id: '3', title: 'Investigation - Performance Concern', createdAt: '2026-01-23' }
    ]
  };

  constructor(private http: HttpClient) {}
  
  getDashboard() { 
    return this.http.get<DashboardDto>(this.baseUrl);
  }

  getViolations(groupBy: 'factory' | 'department' | 'type', opts: { from?: string; to?: string } = {}) {
    let params = new HttpParams().set('groupBy', groupBy);
    if (opts.from) params = params.set('from', opts.from);
    if (opts.to) params = params.set('to', opts.to);
    return this.http.get<{ labels: string[]; values: number[]; items: any[] }>(`${environment.apiBaseUrl}/Analytics/violations`, { params });
  }
}
