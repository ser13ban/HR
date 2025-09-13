import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { RegisterRequest, EmployeeRole, ApiError } from '../../../core/models/auth.models';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.sass'
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  registerForm: FormGroup;
  isLoading = signal(false);
  error = signal<string | null>(null);
  fieldErrors = signal<{[key: string]: string}>({});

  // Expose EmployeeRole enum to template
  EmployeeRole = EmployeeRole;
  roleOptions = [
    { value: EmployeeRole.Employee, label: 'Employee' },
    { value: EmployeeRole.Manager, label: 'Manager' },
    { value: EmployeeRole.Admin, label: 'Admin' }
  ];

  constructor() {
    this.registerForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.maxLength(100)]],
      lastName: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(255)]],
      password: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(100)]],
      role: [EmployeeRole.Employee, [Validators.required]],
      department: ['', [Validators.maxLength(100)]],
      team: ['', [Validators.maxLength(100)]],
      description: ['', [Validators.maxLength(1000)]]
    });
  }

  onSubmit(): void {
    if (this.registerForm.valid) {
      this.isLoading.set(true);
      this.error.set(null);
      this.fieldErrors.set({});

      const registerData: RegisterRequest = {
        firstName: this.registerForm.value.firstName.trim(),
        lastName: this.registerForm.value.lastName.trim(),
        email: this.registerForm.value.email.trim().toLowerCase(),
        password: this.registerForm.value.password,
        role: this.registerForm.value.role,
        department: this.registerForm.value.department?.trim() || undefined,
        team: this.registerForm.value.team?.trim() || undefined,
        description: this.registerForm.value.description?.trim() || undefined
      };

      this.authService.register(registerData).subscribe({
        next: (response) => {
          this.isLoading.set(false);
          // Registration successful, redirect to login
          this.router.navigate(['/login'], { 
            queryParams: { 
              message: 'Registration successful! Please log in with your credentials.' 
            } 
          });
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
    Object.keys(this.registerForm.controls).forEach(key => {
      const control = this.registerForm.get(key);
      control?.markAsTouched();
    });
  }

  getFieldError(fieldName: string): string | null {
    const control = this.registerForm.get(fieldName);
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
      if (errors?.['minlength']) {
        return `${this.getFieldDisplayName(fieldName)} must be at least ${errors['minlength'].requiredLength} characters`;
      }
      if (errors?.['maxlength']) {
        return `${this.getFieldDisplayName(fieldName)} cannot exceed ${errors['maxlength'].requiredLength} characters`;
      }
    }

    return null;
  }

  private getFieldDisplayName(fieldName: string): string {
    const displayNames: {[key: string]: string} = {
      firstName: 'First name',
      lastName: 'Last name',
      email: 'Email',
      password: 'Password',
      role: 'Role',
      department: 'Department',
      team: 'Team',
      description: 'Description'
    };
    return displayNames[fieldName] || fieldName;
  }

  hasFieldError(fieldName: string): boolean {
    return this.getFieldError(fieldName) !== null;
  }
}
