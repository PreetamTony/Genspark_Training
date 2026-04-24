import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  email = ''; password = ''; error = ''; loading = false;
  redirectTo = '';
  emailError = ''; passwordError = '';

  constructor(private auth: AuthService, private router: Router) {
    const nav = this.router.getCurrentNavigation();
    this.redirectTo = nav?.extras?.state?.['redirectTo'] || '';
  }

  validateForm(): boolean {
    this.emailError = '';
    this.passwordError = '';
    let isValid = true;

    if (!this.email) {
      this.emailError = 'Email is required';
      isValid = false;
    } else if (!this.isValidEmail(this.email)) {
      this.emailError = 'Please enter a valid email address';
      isValid = false;
    }

    if (!this.password) {
      this.passwordError = 'Password is required';
      isValid = false;
    } else if (this.password.length < 6) {
      this.passwordError = 'Password must be at least 6 characters';
      isValid = false;
    }

    return isValid;
  }

  private isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }

  clearPasswordError() {
    if (this.passwordError) {
      this.passwordError = '';
    }
  }

  clearEmailError() {
    if (this.emailError) {
      this.emailError = '';
    }
  }

  onSubmit() {
    if (!this.validateForm()) return;

    this.loading = true;
    this.error = '';
    this.emailError = '';
    this.passwordError = '';

    this.auth.login(this.email, this.password)
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: (res) => {
          const role = this.auth.currentUser?.role;
          if (this.redirectTo) this.router.navigateByUrl(this.redirectTo);
          else if (role === 'Admin') this.router.navigate(['/admin']);
          else if (role === 'Operator') this.router.navigate(['/operator']);
          else this.router.navigate(['/']);
        },
        error: (err) => {
          console.error('Login error:', err);
          if (err.status === 0) {
            this.error = 'Unable to connect to server. Please check your internet connection.';
          } else if (err.status === 401) {
            const errorMessage = err.error?.message || '';
            if (errorMessage.toLowerCase().includes('invalid email') || errorMessage.toLowerCase().includes('not found')) {
              this.error = 'Account not found. Please check your email address or <a href="/register">create a new account</a>.';
              this.emailError = 'Email not registered';
            } else if (errorMessage.toLowerCase().includes('password')) {
              this.error = 'Incorrect password. Please try again or <a href="/register">reset your password</a>.';
              this.passwordError = 'Incorrect password';
            } else {
              this.error = 'Invalid login credentials. Please check your email and password.';
            }
          } else if (err.status === 403) {
            this.error = 'Your account has been disabled. Please contact support.';
          } else if (err.status === 429) {
            this.error = 'Too many login attempts. Please wait 15 minutes before trying again.';
          } else if (err.status >= 500) {
            this.error = 'Server error. Please try again later.';
          } else {
            this.error = 'Login failed. Please check your credentials and try again.';
          }
          
          // Clear password field on error for security
          this.password = '';
        }
      });
  }
}
