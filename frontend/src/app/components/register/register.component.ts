import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { BusService } from '../../services/bus.service';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  form = { name: '', email: '', password: '', role: 'User', companyName: '', headOfficeLocationId: '' };
  error = ''; success = ''; loading = false;
  fieldErrors: { [key: string]: string } = {};
  locations: any[] = [];

  constructor(private auth: AuthService, private router: Router, private busService: BusService) {
    this.loadLocations();
  }

  validateForm(): boolean {
    this.fieldErrors = {};
    let isValid = true;

    if (!this.form.name.trim()) {
      this.fieldErrors['name'] = 'Full name is required';
      isValid = false;
    } else if (this.form.name.trim().length < 2) {
      this.fieldErrors['name'] = 'Name must be at least 2 characters';
      isValid = false;
    }

    if (!this.form.email) {
      this.fieldErrors['email'] = 'Email is required';
      isValid = false;
    } else if (!this.isValidEmail(this.form.email)) {
      this.fieldErrors['email'] = 'Please enter a valid email address';
      isValid = false;
    }

    if (!this.form.password) {
      this.fieldErrors['password'] = 'Password is required';
      isValid = false;
    } else if (this.form.password.length < 6) {
      this.fieldErrors['password'] = 'Password must be at least 6 characters';
      isValid = false;
    }

    if (this.form.role === 'Operator' && !this.form.companyName.trim()) {
      this.fieldErrors['companyName'] = 'Company name is required for operators';
      isValid = false;
    }

    if (this.form.role === 'Operator' && !this.form.headOfficeLocationId) {
      this.fieldErrors['headOfficeLocationId'] = 'Head office location is required for operators';
      isValid = false;
    }

    return isValid;
  }

  private isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }

  loadLocations() {
    this.busService.getLocations().subscribe({
      next: (locations: any[]) => {
        this.locations = locations;
      },
      error: (err: any) => {
        console.error('Error loading locations:', err);
      }
    });
  }

  onSubmit() {
    if (!this.validateForm()) return;

    this.loading = true;
    this.error = '';
    this.success = '';
    this.fieldErrors = {};

    const payload: any = {
      name: this.form.name.trim(),
      email: this.form.email,
      password: this.form.password,
      role: this.form.role
    };
    if (this.form.role === 'Operator') {
      payload.companyName = this.form.companyName.trim();
      if (this.form.headOfficeLocationId) {
        payload.headOfficeLocationId = parseInt(this.form.headOfficeLocationId);
      }
    }

    this.auth.register(payload)
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: (res: any) => {
          this.success = res.message || 'Registered successfully!';
          if (this.form.role === 'User') {
            setTimeout(() => this.router.navigate(['/login']), 1500);
          } else {
            this.form = { name: '', email: '', password: '', role: 'User', companyName: '', headOfficeLocationId: '' };
          }
        },
        error: (err) => {
          console.error('Registration error:', err);
          if (err.status === 0) {
            this.error = 'Unable to connect to server. Please check your internet connection.';
          } else if (err.status === 400) {
            // Handle validation errors
            if (err.error?.errors) {
              this.fieldErrors = err.error.errors;
            } else {
              this.error = err.error?.message || 'Please check your input and try again.';
            }
          } else if (err.status === 409) {
            this.error = 'An account with this email already exists.';
          } else if (err.status >= 500) {
            this.error = 'Server error. Please try again later.';
          } else {
            this.error = err.error?.message || 'Registration failed. Please try again.';
          }
        }
      });
  }
}
