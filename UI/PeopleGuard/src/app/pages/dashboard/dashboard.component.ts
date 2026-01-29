import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardService, DashboardDto } from '../../core/services/dashboard.service';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration } from 'chart.js';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'pg-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, BaseChartDirective],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  private cdr = inject(ChangeDetectorRef);
  private svc = inject(DashboardService);
  private router = inject(Router);
  
  data?: DashboardDto;
  loading = true;

  // Filters
  dateFrom = '';
  dateTo = '';
  groupBy: 'factory' | 'department' | 'type' = 'factory';

  // Bar chart for violations by factory
  barChartData?: ChartConfiguration<'bar'>['data'];
  barChartOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    plugins: {
      legend: { display: false },
      title: { display: true, text: 'Violations by Factory' }
    }
  };

  // Pie chart for violations by department
  pieChartData?: ChartConfiguration<'pie'>['data'];
  pieChartOptions: ChartConfiguration<'pie'>['options'] = {
    responsive: true,
    plugins: {
      legend: { position: 'right' },
      title: { display: true, text: 'Violations by Department' }
    }
  };

  // Line chart for trend
  lineChartData?: ChartConfiguration<'line'>['data'];
  lineChartOptions: ChartConfiguration<'line'>['options'] = {
    responsive: true,
    plugins: {
      legend: { position: 'top' },
      title: { display: true, text: 'Violations Trend' }
    }
  };

  ngOnInit() {
    this.svc.getDashboard().subscribe({
      next: d => { 
        this.data = d; 
        this.prepareCharts(d);
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: _ => { 
        this.loading = false; 
        this.cdr.detectChanges();
      }
    });

    this.loadGrouped();
  }

  private prepareCharts(data: DashboardDto) {
    // Bar chart data
    this.barChartData = {
      labels: data.violationsByFactory.map(v => v.label),
      datasets: [{
        data: data.violationsByFactory.map(v => v.value),
        backgroundColor: 'rgba(54, 162, 235, 0.6)',
        borderColor: 'rgba(54, 162, 235, 1)',
        borderWidth: 1,
        label: 'Count'
      }]
    };

    // Pie chart data
    this.pieChartData = {
      labels: data.violationsByDepartment.map(v => v.label),
      datasets: [{
        data: data.violationsByDepartment.map(v => v.value),
        backgroundColor: [
          'rgba(255, 99, 132, 0.6)',
          'rgba(54, 162, 235, 0.6)',
          'rgba(255, 206, 86, 0.6)',
          'rgba(75, 192, 192, 0.6)',
          'rgba(153, 102, 255, 0.6)',
        ]
      }]
    };

    // Line chart data
    this.lineChartData = {
      labels: data.violationsTrend.map(v => v.label),
      datasets: [{
        data: data.violationsTrend.map(v => v.value),
        label: 'Violations',
        fill: false,
        borderColor: 'rgba(75, 192, 192, 1)',
        tension: 0.1
      }]
    };
  }

  loadGrouped() {
    this.svc.getViolations(this.groupBy, { from: this.dateFrom || undefined, to: this.dateTo || undefined }).subscribe(res => {
      this.barChartData = {
        labels: res.labels,
        datasets: [{
          data: res.values,
          backgroundColor: 'rgba(54, 162, 235, 0.6)',
          borderColor: 'rgba(54, 162, 235, 1)',
          borderWidth: 1,
          label: 'Count'
        }]
      };
      this.pieChartData = {
        labels: res.labels,
        datasets: [{ data: res.values }]
      };
      this.cdr.detectChanges();
    });
  }

  onDateChange() {
    this.loadGrouped();
  }

  onGroupChange(group: 'factory' | 'department' | 'type') {
    this.groupBy = group;
    this.loadGrouped();
  }

  onPieChartClick(event: any) {
    if (event.active && event.active[0] && this.pieChartData?.labels) {
      const index = event.active[0].index;
      const label = String(this.pieChartData.labels[index] || '');
      this.drillToCases(label);
    }
  }

  drillToCases(label: string) {
    const params = new URLSearchParams();
    if (this.groupBy === 'factory') params.set('factory', label);
    if (this.groupBy === 'department') params.set('department', label);
    if (this.groupBy === 'type') params.set('type', label);
    if (this.dateFrom) params.set('from', this.dateFrom);
    if (this.dateTo) params.set('to', this.dateTo);
    this.router.navigateByUrl('/cases?' + params.toString());
  }

  String = String;
}
