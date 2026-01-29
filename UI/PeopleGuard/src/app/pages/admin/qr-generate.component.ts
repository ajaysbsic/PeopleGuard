import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { QrService, GenerateQrRequest } from '../../core/services/qr.service';

@Component({
  selector: 'pg-qr-generate',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './qr-generate.component.html',
  styleUrls: ['./qr-generate.component.scss']
})
export class QrGenerateComponent implements OnInit {
  private qrService = inject(QrService);
  private router = inject(Router);

  form = signal({
    targetType: 'general',
    targetId: 'general',
    label: ''
  });

  tokens = signal<any[]>([]);
  selectedToken = signal<any | null>(null);
  loading = signal(false);
  loadingTokens = signal(false);
  page = signal(1);

  ngOnInit() {
    this.loadTokens();
  }

  onGenerate() {
    const req: GenerateQrRequest = {
      targetType: this.form().targetType,
      targetId: this.form().targetId,
      label: this.form().label || undefined
    };

    this.loading.set(true);
    this.qrService.generateToken(req).subscribe({
      next: (res) => {
        this.selectedToken.set(res);
        this.loading.set(false);
        this.loadTokens();
      },
      error: (err) => {
        console.error(err);
        this.loading.set(false);
      }
    });
  }

  loadTokens() {
    this.loadingTokens.set(true);
    this.qrService.listTokens(this.page(), 10).subscribe({
      next: (res) => {
        this.tokens.set(res.data);
        this.loadingTokens.set(false);
      },
      error: () => this.loadingTokens.set(false)
    });
  }

  downloadQr(tokenId: string) {
    const url = this.qrService.getQrImage(tokenId);
    window.open(url, '_blank');
  }

  onPageChange(delta: number) {
    const next = this.page() + delta;
    if (next < 1) return;
    this.page.set(next);
    this.loadTokens();
  }
}
