import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { EmployeeService } from '../../core/services/employee.service';
import { AuthService } from '../../core/services/auth.service';
import { FeedbackService } from '../../core/services/feedback.service';
import { UtilityService } from '../../shared/services/utility.service';
import { EmployeeProfile, UpdateEmployeeRequest } from '../../core/models/employee.models';
import { EmployeeRole } from '../../core/models/auth.models';
import { GiveFeedbackComponent } from '../feedback/give-feedback/give-feedback.component';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, GiveFeedbackComponent],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.sass']
})
export class ProfileComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly employeeService = inject(EmployeeService);
  private readonly authService = inject(AuthService);
  private readonly feedbackService = inject(FeedbackService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly utilityService = inject(UtilityService);

  // Reactive state using signals
  employee = signal<EmployeeProfile | null>(null);
  loading = signal<boolean>(false);
  error = signal<string | null>(null);
  isOwnProfile = signal<boolean>(false);
  isEditing = signal<boolean>(false);
  isManager = signal<boolean>(false);
  canEdit = signal<boolean>(false);
  saving = signal<boolean>(false);
  saveError = signal<string | null>(null);
  
  // Feedback-related state
  canViewFeedback = signal<boolean>(false);
  canGiveFeedback = signal<boolean>(false);
  showGiveFeedbackModal = signal<boolean>(false);

  // Form for editing
  editForm: FormGroup;

  constructor() {
    // Initialize the edit form
    this.editForm = this.formBuilder.group({
      firstName: ['', [Validators.required, Validators.maxLength(100)]],
      lastName: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
      phoneNumber: ['', [Validators.maxLength(20)]],
      department: ['', [Validators.maxLength(100)]],
      team: ['', [Validators.maxLength(100)]],
      position: ['', [Validators.maxLength(100)]],
      bio: ['', [Validators.maxLength(500)]],
      profilePictureUrl: ['', [Validators.maxLength(255)]],
      dateOfBirth: [''],
      address: ['', [Validators.maxLength(500)]],
      emergencyContact: ['', [Validators.maxLength(100)]],
      emergencyPhone: ['', [Validators.maxLength(20)]]
    });
  }

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

    // Check if this is the current user's profile and set permissions
    const currentUser = this.authService.getCurrentUser();
    const isOwnProfile = currentUser?.id.toString() === employeeId;
    const isManager = currentUser?.role === EmployeeRole.Manager;
    
    this.isOwnProfile.set(isOwnProfile);
    this.isManager.set(isManager);
    this.canEdit.set(isOwnProfile || isManager);

    this.employeeService.getEmployeeById(employeeId).subscribe({
      next: (employee) => {
        this.employee.set(employee);
        this.populateEditForm(employee);
        this.loadFeedbackPermissions(parseInt(employeeId));
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

  // Editing methods
  private populateEditForm(employee: EmployeeProfile): void {
    this.editForm.patchValue({
      firstName: employee.firstName,
      lastName: employee.lastName,
      email: employee.email,
      phoneNumber: employee.phoneNumber || '',
      department: employee.department === 'Not Assigned' ? '' : employee.department,
      team: employee.team === 'Not Assigned' ? '' : employee.team,
      position: employee.position === 'Not Assigned' ? '' : employee.position,
      bio: '', // Bio not in current EmployeeProfile interface, will be empty
      profilePictureUrl: employee.profilePictureUrl || '',
      dateOfBirth: employee.dateOfBirth || '',
      address: employee.address || '',
      emergencyContact: employee.emergencyContact || '',
      emergencyPhone: employee.emergencyPhone || ''
    });
  }

  toggleEditMode(): void {
    if (this.isEditing()) {
      this.cancelEdit();
    } else {
      this.isEditing.set(true);
      this.saveError.set(null);
    }
  }

  saveProfile(): void {
    if (this.editForm.invalid) {
      this.markFormGroupTouched(this.editForm);
      return;
    }

    const employee = this.employee();
    if (!employee) {
      return;
    }

    this.saving.set(true);
    this.saveError.set(null);

    const updateData: UpdateEmployeeRequest = {
      firstName: this.editForm.value.firstName,
      lastName: this.editForm.value.lastName,
      email: this.editForm.value.email,
      phoneNumber: this.editForm.value.phoneNumber || undefined,
      department: this.editForm.value.department || undefined,
      team: this.editForm.value.team || undefined,
      position: this.editForm.value.position || undefined,
      bio: this.editForm.value.bio || undefined,
      profilePictureUrl: this.editForm.value.profilePictureUrl || undefined,
      dateOfBirth: this.editForm.value.dateOfBirth || undefined,
      address: this.editForm.value.address || undefined,
      emergencyContact: this.editForm.value.emergencyContact || undefined,
      emergencyPhone: this.editForm.value.emergencyPhone || undefined
    };

    this.employeeService.updateEmployee(employee.id, updateData).subscribe({
      next: (updatedEmployee) => {
        this.employee.set(updatedEmployee);
        this.populateEditForm(updatedEmployee);
        this.isEditing.set(false);
        this.saving.set(false);
        // Could add success toast notification here
      },
      error: (error) => {
        console.error('Error updating employee profile:', error);
        this.saveError.set(error.message || 'Failed to update profile');
        this.saving.set(false);
      }
    });
  }

  cancelEdit(): void {
    const employee = this.employee();
    if (employee) {
      this.populateEditForm(employee);
    }
    this.isEditing.set(false);
    this.saveError.set(null);
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(field => {
      const control = formGroup.get(field);
      control?.markAsTouched({ onlySelf: true });
    });
  }

  getFieldError(fieldName: string): string | null {
    const control = this.editForm.get(fieldName);
    if (control && control.invalid && (control.dirty || control.touched)) {
      if (control.errors?.['required']) {
        return `${this.getFieldDisplayName(fieldName)} is required`;
      }
      if (control.errors?.['email']) {
        return 'Please enter a valid email address';
      }
      if (control.errors?.['maxlength']) {
        const maxLength = control.errors?.['maxlength'].requiredLength;
        return `${this.getFieldDisplayName(fieldName)} cannot exceed ${maxLength} characters`;
      }
    }
    return null;
  }

  private getFieldDisplayName(fieldName: string): string {
    const displayNames: { [key: string]: string } = {
      firstName: 'First Name',
      lastName: 'Last Name',
      email: 'Email',
      phoneNumber: 'Phone Number',
      department: 'Department',
      team: 'Team',
      position: 'Position',
      bio: 'Bio',
      profilePictureUrl: 'Profile Picture URL',
      dateOfBirth: 'Date of Birth',
      address: 'Address',
      emergencyContact: 'Emergency Contact',
      emergencyPhone: 'Emergency Phone'
    };
    return displayNames[fieldName] || fieldName;
  }

  getProfilePictureUrl(employee: EmployeeProfile): string {
    return this.utilityService.getProfilePictureUrl(employee);
  }

  getRoleDisplayName(role: string): string {
    return this.utilityService.getRoleDisplayName(role);
  }

  formatDate(dateString: string): string {
    return this.utilityService.formatDate(dateString);
  }

  shouldShowField(employee: EmployeeProfile, fieldName: string): boolean {
    if (!employee.isLimitedView) {
      return true; // Show all fields for full access
    }

    // For limited view, only show certain fields
    const allowedFields = ['firstName', 'lastName', 'fullName', 'department', 'team', 'position', 'role', 'startDate'];
    return allowedFields.includes(fieldName);
  }

  // Feedback-related methods
  private loadFeedbackPermissions(employeeId: number): void {
    // Load feedback permissions
    this.feedbackService.canViewFeedback(employeeId).subscribe({
      next: (canView) => {
        this.canViewFeedback.set(canView);
      },
      error: (error) => {
        console.error('Error checking view feedback permission:', error);
        this.canViewFeedback.set(false);
      }
    });

    this.feedbackService.canGiveFeedback(employeeId).subscribe({
      next: (canGive) => {
        this.canGiveFeedback.set(canGive);
      },
      error: (error) => {
        console.error('Error checking give feedback permission:', error);
        this.canGiveFeedback.set(false);
      }
    });
  }

  onViewFeedback(): void {
    const employee = this.employee();
    if (employee) {
      this.router.navigate(['/feedback', employee.id]);
    }
  }

  onGiveFeedback(): void {
    this.showGiveFeedbackModal.set(true);
  }

  onFeedbackSubmitted(): void {
    // Feedback was successfully submitted
    // Could show a success message or refresh data if needed
    console.log('Feedback submitted successfully');
  }

  onGiveFeedbackModalClosed(): void {
    this.showGiveFeedbackModal.set(false);
  }

  onViewAbsenceRequests(): void {
    this.router.navigate(['/absence']);
  }

  getEmployeeIdAsNumber(): number {
    const employee = this.employee();
    return employee ? parseInt(employee.id) : 0;
  }
}
