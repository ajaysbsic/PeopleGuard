import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { WarningLettersService } from '../../core/services/warning-letters.service';
import { InvestigationsService, InvestigationDto } from '../../core/services/investigations.service';

@Component({
  selector: 'pg-warning-letter-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './warning-letter-create.component.html',
  styleUrls: ['./warning-letter-create.component.scss']
})
export class WarningLetterCreateComponent implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private warningLettersSvc = inject(WarningLettersService);
  private investigationsSvc = inject(InvestigationsService);
  private cdr = inject(ChangeDetectorRef);

  form!: FormGroup;
  investigation: InvestigationDto | null = null;
  investigations: InvestigationDto[] = [];
  loading = false;
  loadingInvestigation = true;
  loadingInvestigations = true;
  submitted = false;
  showInvestigationSelector = true;

  outcomeTypes = [
    { value: 1, label: 'No Action' },
    { value: 2, label: 'Verbal Warning' },
    { value: 3, label: 'Written Warning' }
  ];

  outcomeTemplates: { [key: number]: string } = {
    1: 'No action to be taken. Case closed.',
    2: 'Employee to receive verbal warning regarding the investigation findings.',
    3: 'Employee to receive written warning regarding the investigation findings.'
  };

  ngOnInit() {
    this.initForm();
    this.loadInvestigation();
    this.loadInvestigations();
  }

  initForm() {
    this.form = this.fb.group({
      investigationId: ['', Validators.required],
      outcome: [2, Validators.required],
      reason: ['', Validators.required]
    });
  }

  loadInvestigations() {
    this.investigationsSvc.getAll().subscribe({
      next: (invs) => {
        // Filter to only show closed or ready-for-warning investigations
        this.investigations = invs.filter(inv => inv.status === 3); // Status 3 = Closed
        this.loadingInvestigations = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loadingInvestigations = false;
        this.cdr.detectChanges();
      }
    });
  }

  loadInvestigation() {
    const investigationId = this.route.snapshot.queryParamMap.get('investigationId');
    
    if (!investigationId) {
      this.loadingInvestigation = false;
      return;
    }

    this.investigationsSvc.getById(investigationId).subscribe({
      next: (inv) => {
        this.investigation = inv;
        this.showInvestigationSelector = false;
        this.form.patchValue({ investigationId: inv.id });
        this.loadingInvestigation = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loadingInvestigation = false;
        this.cdr.detectChanges();
      }
    });
  }

  onInvestigationSelect(investigationId: string) {
    const selected = this.investigations.find(inv => inv.id === investigationId);
    if (selected) {
      this.investigation = selected;
      this.form.patchValue({ investigationId: selected.id });
    }
  }

  onOutcomeChange() {
    const outcome = this.form.get('outcome')?.value;
    if (outcome !== undefined) {
      const template = this.outcomeTemplates[outcome] || '';
      this.form.patchValue({ reason: template }, { emitEvent: false });
    }
  }

  onSubmit() {
    this.submitted = true;

    if (this.form.invalid || !this.investigation) {
      alert('Please select an investigation and fill in all required fields');
      return;
    }

    this.loading = true;
    const payload = {
      investigationId: this.investigation.id,
      employeeId: this.investigation.employeeId,
      outcome: Number(this.form.value.outcome),
      reason: this.form.value.reason
    };

    this.warningLettersSvc.create(payload).subscribe({
      next: (result) => {
        this.loading = false;
        alert('Warning letter created successfully');
        this.router.navigateByUrl(`/investigations/${this.investigation?.id}`);
      },
      error: (err) => {
        this.loading = false;
        console.error(err);
        alert('Failed to create warning letter');
      }
    });
  }

  onCancel() {
    if (this.investigation) {
      this.router.navigateByUrl(`/investigations/${this.investigation.id}`);
    } else {
      this.router.navigateByUrl('/warning-letters');
    }
  }

  getOutcomeLabel(value: number): string {
    const type = this.outcomeTypes.find(t => t.value === value);
    return type ? type.label : 'Unknown';
  }
}
