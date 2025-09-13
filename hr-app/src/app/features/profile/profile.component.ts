import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { EmployeeService } from '../../core/services/employee.service';
import { AuthService } from '../../core/services/auth.service';
import { EmployeeProfile } from '../../core/models/employee.models';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.sass']
})
export class ProfileComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly employeeService = inject(EmployeeService);
  private readonly authService = inject(AuthService);

  // Reactive state using signals
  employee = signal<EmployeeProfile | null>(null);
  loading = signal<boolean>(false);
  error = signal<string | null>(null);
  isOwnProfile = signal<boolean>(false);

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const employeeId = params.get('id');
      if (employeeId) {
        this.loadEmployeeProfile(employeeId);
      } else {
        this.error.set('Invalid employee ID');
      }
    });
  }

  private loadEmployeeProfile(employeeId: string): void {
    this.loading.set(true);
    this.error.set(null);

    // Check if this is the current user's profile
    const currentUser = this.authService.getCurrentUser();
    this.isOwnProfile.set(currentUser?.id.toString() === employeeId);

    this.employeeService.getEmployeeById(employeeId).subscribe({
      next: (employee) => {
        this.employee.set(employee);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading employee profile:', error);
        this.error.set(error.message || 'Failed to load employee profile');
        this.loading.set(false);
      }
    });
  }

  onGoBack(): void {
    this.router.navigate(['/home']);
  }

  onRetry(): void {
    const employeeId = this.route.snapshot.paramMap.get('id');
    if (employeeId) {
      this.loadEmployeeProfile(employeeId);
    }
  }

  getProfilePictureUrl(employee: EmployeeProfile): string {
    return employee.profilePictureUrl || '/assets/images/default-avatar.svg';
  }

  getRoleDisplayName(role: string): string {
    switch (role.toLowerCase()) {
      case 'admin':
        return 'Administrator';
      case 'manager':
        return 'Manager';
      case 'employee':
        return 'Employee';
      default:
        return role;
    }
  }

  formatDate(dateString: string): string {
    try {
      const date = new Date(dateString);
      return date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
      });
    } catch {
      return 'Not available';
    }
  }

  shouldShowField(employee: EmployeeProfile, fieldName: string): boolean {
    if (!employee.isLimitedView) {
      return true; // Show all fields for full access
    }

    // For limited view, only show certain fields
    const allowedFields = ['firstName', 'lastName', 'fullName', 'department', 'team', 'position', 'role', 'startDate'];
    return allowedFields.includes(fieldName);
  }
}
