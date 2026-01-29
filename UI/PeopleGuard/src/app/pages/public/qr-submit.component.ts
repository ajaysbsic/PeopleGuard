import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { QrService, QrSubmissionRequest } from '../../core/services/qr.service';

@Component({
  selector: 'pg-qr-submit',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './qr-submit.component.html',
  styleUrls: ['./qr-submit.component.scss']
})
export class QrSubmitComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private qrService = inject(QrService);

  token = signal<string | null>(null);
  loading = signal(false);
  error = signal<string | null>(null);
  success = signal(false);

  form = signal({
    category: 'complaint',
    message: '',
    submitterName: '',
    submitterEmail: '',
    submitterPhone: ''
  });

  ngOnInit() {
    const token = this.route.snapshot.paramMap.get('token');
    if (!token) {
      this.error.set('Invalid QR token');
      return;
    }
    this.token.set(token);
  }

  onSubmit() {
    if (!this.form().message.trim()) {
      this.error.set('Please provide a message');
      return;
    }

    const req: QrSubmissionRequest = {
      token: this.token()!,
      category: this.form().category,
      message: this.form().message,
      submitterName: this.form().submitterName || undefined,
      submitterEmail: this.form().submitterEmail || undefined,
      submitterPhone: this.form().submitterPhone || undefined
    };

    this.loading.set(true);
    this.error.set(null);

    this.qrService.submitComplaint(req).subscribe({
      next: (res) => {
        this.loading.set(false);
        this.success.set(true);
        setTimeout(() => {
          this.router.navigateByUrl('/');
        }, 3000);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.error?.message ?? 'Submission failed. Token may be invalid or expired.');
      }
    });
  }

  reset() {
    this.form.set({
      category: 'complaint',
      message: '',
      submitterName: '',
      submitterEmail: '',
      submitterPhone: ''
    });
    this.error.set(null);
  }
}
