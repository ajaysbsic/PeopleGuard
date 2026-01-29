import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { InvestigationsService, InvestigationDto } from '../../core/services/investigations.service';
import { InvestigationWorkflowComponent } from '../../shared/components/investigation-workflow.component';

export interface InvestigationRemark {
  id: string;
  content: string;
  createdBy: string;
  createdDate: string;
}

@Component({
  selector: 'pg-investigation-detail',
  standalone: true,
  imports: [CommonModule, InvestigationWorkflowComponent],
  templateUrl: './investigation-detail.component.html',
  styleUrls: ['./investigation-detail.component.scss']
})
export class InvestigationDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private investigationsSvc = inject(InvestigationsService);

  investigation: InvestigationDto | null = null;
  remarks: InvestigationRemark[] = [];
  loading = true;
  submitting = false;

  caseTypeMap: { [key: number]: string } = {
    0: 'General',
    1: 'Misconduct',
    2: 'Performance',
    3: 'Attendance',
    4: 'Other'
  };

  statusMap: { [key: number]: string } = {
    0: 'Open',
    1: 'Under Investigation',
    2: 'Closed'
  };

  outcomeMap: { [key: number]: string } = {
    0: 'No Action',
    1: 'Verbal Warning',
    2: 'Written Warning'
  };

  ngOnInit() {
    this.loadInvestigation();
  }

  loadInvestigation() {
    this.loading = true;
    const id = this.route.snapshot.paramMap.get('id');
    
    if (!id) {
      this.router.navigateByUrl('/investigations');
      return;
    }

    this.investigationsSvc.getById(id).subscribe({
      next: (investigation) => {
        this.investigation = investigation;
        // TODO: Load remarks from API
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        console.error(err);
        alert('Failed to load investigation');
      }
    });
  }

  onStatusChange(newStatus: string) {
    if (!this.investigation) return;

    this.submitting = true;
    const statusValue = this.getStatusValue(newStatus);

    this.investigationsSvc.updateStatus(this.investigation.id, statusValue).subscribe({
      next: () => {
        this.submitting = false;
        alert('Status updated successfully');
        this.loadInvestigation();
      },
      error: (err) => {
        this.submitting = false;
        console.error(err);
        alert('Failed to update status');
      }
    });
  }

  onAddRemark(remark: string) {
    if (!this.investigation || !remark.trim()) return;

    this.submitting = true;
    this.investigationsSvc.addRemark(this.investigation.id, remark).subscribe({
      next: () => {
        this.submitting = false;
        alert('Remark added successfully');
        this.loadInvestigation();
      },
      error: (err) => {
        this.submitting = false;
        console.error(err);
        alert('Failed to add remark');
      }
    });
  }

  onCloseCase() {
    if (!this.investigation) return;

    if (!confirm('Are you sure you want to close this investigation?')) {
      return;
    }

    this.submitting = true;
    this.investigationsSvc.updateStatus(this.investigation.id, 2).subscribe({
      next: () => {
        this.submitting = false;
        alert('Investigation closed successfully');
        this.loadInvestigation();
      },
      error: (err) => {
        this.submitting = false;
        console.error(err);
        alert('Failed to close investigation');
      }
    });
  }

  getStatusValue(status: string): number {
    const map: { [key: string]: number } = {
      'Open': 0,
      'Under Investigation': 1,
      'Closed': 2
    };
    return map[status] ?? 0;
  }

  getStatusLabel(value: number): string {
    return this.statusMap[value] ?? 'Unknown';
  }

  getCaseTypeLabel(value: number): string {
    return this.caseTypeMap[value] ?? 'Unknown';
  }

  getOutcomeLabel(value?: number): string {
    if (value === undefined || value === null) return 'Not Set';
    return this.outcomeMap[value] ?? 'Unknown';
  }

  goBack() {
    this.router.navigateByUrl('/investigations');
  }
}

