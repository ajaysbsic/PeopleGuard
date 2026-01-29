import { Directive, HostListener, Input, Output, EventEmitter, inject, ElementRef } from '@angular/core';

/**
 * Keyboard Navigation Directive
 * 
 * Usage:
 * <button pgKeyboardNav (arrowUp)="previousItem()" (arrowDown)="nextItem()">Next</button>
 */
@Directive({
  selector: '[pgKeyboardNav]',
  standalone: true
})
export class KeyboardNavDirective {
  private el = inject(ElementRef);

  @Input() pgKeyboardNav: 'vertical' | 'horizontal' = 'vertical';
  @Output() arrowUp = new EventEmitter<KeyboardEvent>();
  @Output() arrowDown = new EventEmitter<KeyboardEvent>();
  @Output() arrowLeft = new EventEmitter<KeyboardEvent>();
  @Output() arrowRight = new EventEmitter<KeyboardEvent>();
  @Output() enter = new EventEmitter<KeyboardEvent>();
  @Output() escape = new EventEmitter<KeyboardEvent>();
  @Output() space = new EventEmitter<KeyboardEvent>();

  @HostListener('keydown', ['$event'])
  onKeyDown(event: KeyboardEvent) {
    switch (event.key) {
      case 'ArrowUp':
        this.arrowUp.emit(event);
        event.preventDefault();
        break;
      case 'ArrowDown':
        this.arrowDown.emit(event);
        event.preventDefault();
        break;
      case 'ArrowLeft':
        this.arrowLeft.emit(event);
        event.preventDefault();
        break;
      case 'ArrowRight':
        this.arrowRight.emit(event);
        event.preventDefault();
        break;
      case 'Enter':
        this.enter.emit(event);
        break;
      case 'Escape':
        this.escape.emit(event);
        break;
      case ' ':
        this.space.emit(event);
        event.preventDefault();
        break;
    }
  }
}

/**
 * Focus Management Service
 * Handles focus trap, focus restoration, and focus management patterns
 */
export class FocusManager {
  /**
   * Trap focus within an element (modal focus trap pattern)
   */
  static trapFocus(element: HTMLElement): () => void {
    const focusableElements = element.querySelectorAll(
      'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
    ) as NodeListOf<HTMLElement>;

    const firstElement = focusableElements[0];
    const lastElement = focusableElements[focusableElements.length - 1];

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key !== 'Tab') return;

      if (e.shiftKey) {
        if (document.activeElement === firstElement) {
          lastElement.focus();
          e.preventDefault();
        }
      } else {
        if (document.activeElement === lastElement) {
          firstElement.focus();
          e.preventDefault();
        }
      }
    };

    element.addEventListener('keydown', handleKeyDown);

    // Return cleanup function
    return () => {
      element.removeEventListener('keydown', handleKeyDown);
    };
  }

  /**
   * Move focus to an element with optional delay
   */
  static setFocus(element: HTMLElement, delay: number = 0): void {
    if (delay > 0) {
      setTimeout(() => {
        element.focus();
      }, delay);
    } else {
      element.focus();
    }
  }

  /**
   * Announce text for screen readers via aria-live region
   */
  static announce(message: string, priority: 'polite' | 'assertive' = 'polite'): void {
    const announcement = document.createElement('div');
    announcement.setAttribute('role', 'status');
    announcement.setAttribute('aria-live', priority);
    announcement.setAttribute('aria-atomic', 'true');
    announcement.style.position = 'absolute';
    announcement.style.left = '-10000px';
    announcement.style.width = '1px';
    announcement.style.height = '1px';
    announcement.style.overflow = 'hidden';
    announcement.textContent = message;

    document.body.appendChild(announcement);

    setTimeout(() => {
      announcement.remove();
    }, 1000);
  }
}

/**
 * Keyboard Shortcut Service
 * Global keyboard shortcuts for common actions
 */
export class KeyboardShortcuts {
  private static shortcuts: Map<string, () => void> = new Map();

  /**
   * Register a keyboard shortcut
   * Example: registerShortcut('Ctrl+S', saveDocument)
   */
  static register(combination: string, callback: () => void): void {
    this.shortcuts.set(combination, callback);
  }

  /**
   * Handle keyboard shortcut
   */
  static handleShortcut(event: KeyboardEvent): void {
    const combination = this.getKeyCombination(event);
    const callback = this.shortcuts.get(combination);
    if (callback) {
      callback();
      event.preventDefault();
    }
  }

  /**
   * Get string representation of key combination
   */
  private static getKeyCombination(event: KeyboardEvent): string {
    const parts: string[] = [];
    if (event.ctrlKey) parts.push('Ctrl');
    if (event.shiftKey) parts.push('Shift');
    if (event.altKey) parts.push('Alt');
    parts.push(event.key.toUpperCase());
    return parts.join('+');
  }
}

/**
 * Helper: Move focus to next focusable element
 */
export function moveFocusToNext(currentElement: HTMLElement): void {
  const focusableElements = Array.from(
    document.querySelectorAll(
      'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
    )
  ) as HTMLElement[];

  const currentIndex = focusableElements.indexOf(currentElement);
  const nextIndex = (currentIndex + 1) % focusableElements.length;

  focusableElements[nextIndex]?.focus();
}

/**
 * Helper: Move focus to previous focusable element
 */
export function moveFocusToPrevious(currentElement: HTMLElement): void {
  const focusableElements = Array.from(
    document.querySelectorAll(
      'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
    )
  ) as HTMLElement[];

  const currentIndex = focusableElements.indexOf(currentElement);
  const previousIndex = (currentIndex - 1 + focusableElements.length) % focusableElements.length;

  focusableElements[previousIndex]?.focus();
}
