import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AbsenceService } from '../../../core/services/absence.service';
import { AuthService } from '../../../core/services/auth.service';
import { UtilityService } from '../../../shared/services/utility.service';
import { 
  AbsenceRequest, 
  CreateAbsenceRequest, 
  ApprovalAction,
  AbsenceType,
  AbsenceStatus,
  AbsenceTypeLabels,
  AbsenceStatusLabels,
  AbsenceStatusColors
} from '../../../core/models/absence.models';
import { EmployeeRole } from '../../../core/models/auth.models';

@Component({
  selector: 'app-absence-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './absence-page.component.html',
  styleUrls: ['./absence-page.component.sass']
})
export class AbsencePageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  readonly router = inject(Router);
  private readonly absenceService = inject(AbsenceService);
  private readonly authService = inject(AuthService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly utilityService = inject(UtilityService);

  // Reactive state using signals
  myRequests = signal<AbsenceRequest[]>([]);
  approvedRequests = signal<AbsenceRequest[]>([]);
  teamApprovedRequests = signal<AbsenceRequest[]>([]); // Approved requests excluding current user
  loading = signal<boolean>(false);
  error = signal<string | null>(null);
  isManager = signal<boolean>(false);
  showRequestForm = signal<boolean>(false);
  submitting = signal<boolean>(false);
  submitError = signal<string | null>(null);
  currentUserId = signal<number>(0);
  
  // Manager-specific state
  managerRequests = signal<AbsenceRequest[]>([]);
  processingRequest = signal<number | null>(null);
  processingError = signal<string | null>(null);

  // Form for creating new requests
  requestForm: FormGroup;
  
  // Form for approval actions
  approvalForm: FormGroup;
  showApprovalModal = signal<boolean>(false);
  currentRequestForApproval = signal<AbsenceRequest | null>(null);
  approvalAction = signal<'approve' | 'decline'>('approve');

  // Expose enums and labels to template
  readonly AbsenceType = AbsenceType;
  readonly AbsenceStatus = AbsenceStatus;
  readonly AbsenceTypeLabels = AbsenceTypeLabels;
  readonly AbsenceStatusLabels = AbsenceStatusLabels;
  readonly AbsenceStatusColors = AbsenceStatusColors;

  constructor() {
    // Initialize forms
    this.requestForm = this.formBuilder.group({
      type: [AbsenceType.Vacation, [Validators.required]],
      startDate: ['', [Validators.required]],
      endDate: ['', [Validators.required]],
      reason: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(500)]]
    });

    this.approvalForm = this.formBuilder.group({
      approverNotes: ['', [Validators.maxLength(500)]]
    });
  }

  async ngOnInit(): Promise<void> {
    // Check if user is manager and get user ID
    const currentUser = this.authService.currentUser();
    this.isManager.set(currentUser?.role === EmployeeRole.Manager);
    this.currentUserId.set(currentUser?.id || 0);

    await this.loadData();
  }

  async loadData(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);

    try {
      // Load my requests
      const myRequestsPromise = this.absenceService.getMyAbsenceRequests().toPromise();
      
      // Load approved requests for all employees
      const approvedRequestsPromise = this.absenceService.getApprovedAbsenceRequests().toPromise();

      // Load pending requests for manager approval (if user is manager)
      const promises = [myRequestsPromise, approvedRequestsPromise];
      if (this.isManager()) {
        promises.push(this.absenceService.getPendingApprovalsForManager().toPromise());
      }

      const results = await Promise.all(promises);
      
      this.myRequests.set(results[0] || []);
      const allApprovedRequests = results[1] || [];
      this.approvedRequests.set(allApprovedRequests);
      
      // Filter out current user's requests from team approved requests
      const currentUserId = this.currentUserId();
      this.teamApprovedRequests.set(
        allApprovedRequests.filter(request => request.employeeId !== currentUserId)
      );
      
      if (this.isManager() && results[2]) {
        this.managerRequests.set(results[2] || []);
      }

    } catch (error: any) {
      this.error.set(error?.error?.message || 'Failed to load absence data');
      console.error('Error loading absence data:', error);
    } finally {
      this.loading.set(false);
    }
  }

  onToggleRequestForm(): void {
    this.showRequestForm.set(!this.showRequestForm());
    if (this.showRequestForm()) {
      // Reset form when opening
      this.requestForm.reset({
        type: AbsenceType.Vacation
      });
      this.submitError.set(null);
    }
  }

  async onSubmitRequest(): Promise<void> {
    if (this.requestForm.invalid) {
      this.requestForm.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.submitError.set(null);

    try {
      const formValue = this.requestForm.value;
      const createRequest: CreateAbsenceRequest = {
        type: formValue.type,
        startDate: formValue.startDate,
        endDate: formValue.endDate,
        reason: formValue.reason
      };

      const newRequest = await this.absenceService.createAbsenceRequest(createRequest).toPromise();
      
      if (newRequest) {
        // Add to my requests
        this.myRequests.update(requests => [newRequest, ...requests]);
        
        // Reset form and hide it
        this.requestForm.reset({ type: AbsenceType.Vacation });
        this.showRequestForm.set(false);
      }

    } catch (error: any) {
      this.submitError.set(error?.error?.message || 'Failed to create absence request');
      console.error('Error creating absence request:', error);
    } finally {
      this.submitting.set(false);
    }
  }

  async onCancelRequest(request: AbsenceRequest): Promise<void> {
    if (!confirm('Are you sure you want to cancel this absence request?')) {
      return;
    }

    try {
      await this.absenceService.cancelAbsenceRequest(request.id).toPromise();
      
      // Update the request status in the list
      this.myRequests.update(requests => 
        requests.map(r => r.id === request.id 
          ? { ...r, status: AbsenceStatus.Cancelled } 
          : r
        )
      );

    } catch (error: any) {
      console.error('Error cancelling absence request:', error);
      alert(error?.error?.message || 'Failed to cancel absence request');
    }
  }

  onShowApprovalModal(request: AbsenceRequest, action: 'approve' | 'decline'): void {
    this.currentRequestForApproval.set(request);
    this.approvalAction.set(action);
    this.showApprovalModal.set(true);
    this.approvalForm.reset();
    this.processingError.set(null);
  }

  onCloseApprovalModal(): void {
    this.showApprovalModal.set(false);
    this.currentRequestForApproval.set(null);
    this.processingError.set(null);
  }

  async onSubmitApproval(): Promise<void> {
    const request = this.currentRequestForApproval();
    const action = this.approvalAction();
    
    if (!request) return;

    this.processingRequest.set(request.id);
    this.processingError.set(null);

    try {
      const approvalData: ApprovalAction = {
        approverNotes: this.approvalForm.value.approverNotes || undefined
      };

      let updatedRequest: AbsenceRequest;

      if (action === 'approve') {
        updatedRequest = await this.absenceService.approveAbsenceRequest(request.id, approvalData).toPromise() as AbsenceRequest;
      } else {
        updatedRequest = await this.absenceService.declineAbsenceRequest(request.id, approvalData).toPromise() as AbsenceRequest;
      }

      // Update the request in my requests if it's there
      this.myRequests.update(requests => 
        requests.map(r => r.id === request.id ? updatedRequest : r)
      );

      // Remove from manager requests (since it's no longer pending)
      this.managerRequests.update(requests => 
        requests.filter(r => r.id !== request.id)
      );

      // If approved, add to approved requests
      if (updatedRequest.status === AbsenceStatus.Approved) {
        this.approvedRequests.update(requests => {
          const existingIndex = requests.findIndex(r => r.id === updatedRequest.id);
          if (existingIndex >= 0) {
            return requests.map(r => r.id === updatedRequest.id ? updatedRequest : r);
          } else {
            return [updatedRequest, ...requests];
          }
        });

        // Also add to team approved requests (since it's not the current user's request)
        this.teamApprovedRequests.update(requests => [updatedRequest, ...requests]);
      }

      this.onCloseApprovalModal();

    } catch (error: any) {
      this.processingError.set(error?.error?.message || `Failed to ${action} absence request`);
      console.error(`Error ${action}ing absence request:`, error);
    } finally {
      this.processingRequest.set(null);
    }
  }

  onViewProfile(employeeId: number): void {
    this.router.navigate(['/profile', employeeId]);
  }

  getStatusBadgeClass(status: AbsenceStatus): string {
    const baseClasses = 'badge badge-';
    return baseClasses + AbsenceStatusColors[status];
  }

  canCancelRequest(request: AbsenceRequest): boolean {
    return request.status === AbsenceStatus.Pending;
  }

  canApproveRequest(request: AbsenceRequest): boolean {
    return this.isManager() && request.status === AbsenceStatus.Pending;
  }

  formatDate(dateString: string): string {
    return this.utilityService.formatDate(dateString, 'simple');
  }

  calculateDuration(startDate: string, endDate: string): number {
    return this.utilityService.calculateDaysBetween(startDate, endDate);
  }

  getAbsenceTypeOptions(): { value: AbsenceType; label: string }[] {
    return Object.entries(AbsenceTypeLabels).map(([key, label]) => ({
      value: Number(key) as AbsenceType,
      label
    }));
  }

  getTodayDateString(): string {
    return new Date().toISOString().split('T')[0];
  }

  getPendingRequests(): AbsenceRequest[] {
    return this.myRequests().filter(r => r.status === AbsenceStatus.Pending);
  }
}
