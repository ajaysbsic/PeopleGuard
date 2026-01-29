import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'pg-json-modal',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="modal-overlay" (click)="close.emit()">
      <div class="modal-content" (click)="$event.stopPropagation()">
        <div class="modal-header">
          <h2>{{ title }}</h2>
          <button
            class="btn-close"
            (click)="close.emit()"
            aria-label="Close modal"
          >
            ×
          </button>
        </div>
        <div class="modal-body">
          <pre class="json-display">{{ data | json }}</pre>
        </div>
        <div class="modal-footer">
          <button
            class="btn-secondary"
            (click)="copyToClipboard()"
            aria-label="Copy JSON to clipboard"
          >
            Copy to Clipboard
          </button>
          <button
            class="btn-primary"
            (click)="close.emit()"
            aria-label="Close"
          >
            Close
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .modal-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0, 0, 0, 0.5);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1000;
    }

    .modal-content {
      background: white;
      border-radius: 8px;
      max-width: 600px;
      max-height: 80vh;
      display: flex;
      flex-direction: column;
      box-shadow: 0 4px 16px rgba(0, 0, 0, 0.2);

      @media (max-width: 768px) {
        max-width: 90vw;
      }
    }

    .modal-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1.5rem;
      border-bottom: 1px solid #ddd;

      h2 {
        margin: 0;
        font-size: 1.25rem;
        color: #333;
      }

      .btn-close {
        background: none;
        border: none;
        font-size: 1.5rem;
        cursor: pointer;
        color: #666;
        width: 32px;
        height: 32px;
        display: flex;
        align-items: center;
        justify-content: center;
        border-radius: 4px;
        transition: all 0.2s;

        &:hover {
          background: #f0f0f0;
          color: #333;
        }

        &:focus {
          outline: none;
          box-shadow: 0 0 0 3px rgba(33, 150, 243, 0.2);
        }
      }
    }

    .modal-body {
      flex: 1;
      overflow: auto;
      padding: 1.5rem;
      background: #f9f9f9;

      .json-display {
        margin: 0;
        font-size: 0.85rem;
        line-height: 1.6;
        color: #333;
        background: white;
        padding: 1rem;
        border-radius: 4px;
        border: 1px solid #ddd;
        max-height: 100%;
        overflow: auto;
        font-family: 'Courier New', monospace;
        white-space: pre-wrap;
        word-wrap: break-word;
      }
    }

    .modal-footer {
      display: flex;
      gap: 1rem;
      justify-content: flex-end;
      padding: 1.5rem;
      border-top: 1px solid #ddd;
      background: #fafafa;
    }

    button {
      padding: 0.6rem 1.2rem;
      border: none;
      border-radius: 4px;
      font-size: 0.95rem;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.2s;

      &:focus {
        outline: none;
        box-shadow: 0 0 0 3px rgba(33, 150, 243, 0.2);
      }

      &:disabled {
        opacity: 0.5;
        cursor: not-allowed;
      }
    }

    .btn-primary {
      background: #2196f3;
      color: white;

      &:hover {
        background: #1976d2;
      }
    }

    .btn-secondary {
      background: #e0e0e0;
      color: #333;

      &:hover {
        background: #bdbdbd;
      }
    }
  `]
})
export class JsonModalComponent {
  @Input() title: string = 'Details';
  @Input() data: any;
  @Output() close = new EventEmitter<void>();

  copyToClipboard() {
    const json = JSON.stringify(this.data, null, 2);
    navigator.clipboard.writeText(json).then(() => {
      alert('Copied to clipboard!');
    });
  }
}
