import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService, LoginRequestDto } from '../../core/services/auth.service';

@Component({
  selector: 'pg-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  model: LoginRequestDto = { email: '', password: '' };
  loading = false;
  error = '';

  constructor(private auth: AuthService, private router: Router) {}

  submit() {
    this.loading = true;
    this.error = '';
    this.auth.login(this.model).subscribe({
      next: r => { 
        // Session is already set by the login() method via tap()
        // Add small delay to ensure localStorage is fully written
        setTimeout(() => {
          this.router.navigateByUrl('/dashboard');
        }, 100);
      },
      error: err => { 
        this.error = err.error?.message || 'Invalid credentials. Please try again.'; 
        this.loading = false; 
      }
    });
  }
}
