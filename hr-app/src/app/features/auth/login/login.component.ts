import { Component, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { LoginRequest, ApiError } from '../../../core/models/auth.models';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.sass'
})
export class LoginComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  loginForm: FormGroup;
  isLoading = signal(false);
  error = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  fieldErrors = signal<{[key: string]: string}>({});

  private returnUrl = '/';

  constructor() {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]]
    });
  }

  ngOnInit(): void {
    // Get return URL from route parameters or default to '/'
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
    
    // Check for success message from registration
    const message = this.route.snapshot.queryParams['message'];
    if (message) {
      this.successMessage.set(message);
    }

    // If user is already authenticated, redirect to return URL
    if (this.authService.isUserAuthenticated()) {
      this.router.navigate([this.returnUrl]);
    }
  }

  onSubmit(): void {
    if (this.loginForm.valid) {
      this.isLoading.set(true);
      this.error.set(null);
      this.fieldErrors.set({});

      const loginData: LoginRequest = {
        email: this.loginForm.value.email.trim().toLowerCase(),
        password: this.loginForm.value.password
      };

      this.authService.login(loginData).subscribe({
        next: (response) => {
          this.isLoading.set(false);
          // Login successful, redirect to return URL or home
          this.router.navigate([this.returnUrl]);
        },
        error: (apiError: ApiError) => {
          this.isLoading.set(false);
          this.handleError(apiError);
        }
      });
    } else {
      this.markFormGroupTouched();
    }
  }

  private handleError(apiError: ApiError): void {
    if (apiError.errors && apiError.errors.length > 0) {
      // Handle validation errors
      const errors: {[key: string]: string} = {};
      apiError.errors.forEach(error => {
        errors[error.field] = error.message;
      });
      this.fieldErrors.set(errors);
    } else {
      // Handle general error
      this.error.set(apiError.message);
    }
  }

  private markFormGroupTouched(): void {
    Object.keys(this.loginForm.controls).forEach(key => {
      const control = this.loginForm.get(key);
      control?.markAsTouched();
    });
  }

  getFieldError(fieldName: string): string | null {
    const control = this.loginForm.get(fieldName);
    const fieldErrors = this.fieldErrors();
    
    // Check for server-side validation errors first
    if (fieldErrors[fieldName]) {
      return fieldErrors[fieldName];
    }

    // Check for client-side validation errors
    if (control?.invalid && (control?.dirty || control?.touched)) {
      const errors = control.errors;
      if (errors?.['required']) {
        return `${this.getFieldDisplayName(fieldName)} is required`;
      }
      if (errors?.['email']) {
        return 'Please enter a valid email address';
      }
    }

    return null;
  }

  private getFieldDisplayName(fieldName: string): string {
    const displayNames: {[key: string]: string} = {
      email: 'Email',
      password: 'Password'
    };
    return displayNames[fieldName] || fieldName;
  }

  hasFieldError(fieldName: string): boolean {
    return this.getFieldError(fieldName) !== null;
  }

  dismissSuccessMessage(): void {
    this.successMessage.set(null);
  }
}
