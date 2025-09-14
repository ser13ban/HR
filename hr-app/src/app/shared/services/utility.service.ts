import { Injectable } from '@angular/core';
import { EmployeeRole, EmployeeRoleLabels } from '../../core/models/auth.models';
import { AbsenceType, AbsenceStatus, AbsenceTypeLabels, AbsenceStatusLabels } from '../../core/models/absence.models';
import { FeedbackType, FeedbackTypeLabels } from '../../core/models/feedback.models';
import { EmployeeListItem, EmployeeProfile } from '../../core/models/employee.models';

@Injectable({
  providedIn: 'root'
})
export class UtilityService {

  /**
   * Get display name for employee role
   */
  getRoleDisplayName(role: EmployeeRole): string {
    return EmployeeRoleLabels[role] || 'Unknown';
  }

  /**
   * Get display name for absence type
   */
  getAbsenceTypeDisplayName(type: AbsenceType): string {
    return AbsenceTypeLabels[type] || 'Unknown';
  }

  /**
   * Get display name for absence status
   */
  getAbsenceStatusDisplayName(status: AbsenceStatus): string {
    return AbsenceStatusLabels[status] || 'Unknown';
  }

  /**
   * Get display name for feedback type
   */
  getFeedbackTypeDisplayName(type: FeedbackType): string {
    return FeedbackTypeLabels[type] || 'Unknown';
  }

  /**
   * Get profile picture URL with fallback to default avatar
   */
  getProfilePictureUrl(employee: EmployeeListItem | EmployeeProfile | { profilePictureUrl?: string }): string {
    return employee.profilePictureUrl || '/assets/images/default-avatar.svg';
  }

  /**
   * Format date string for display
   * @param dateString - ISO date string
   * @param format - Optional format type ('short', 'long', 'simple')
   */
  formatDate(dateString: string, format: 'short' | 'long' | 'simple' = 'long'): string {
    try {
      const date = new Date(dateString);
      
      switch (format) {
        case 'simple':
          return date.toLocaleDateString();
        case 'short':
          return date.toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
          });
        case 'long':
        default:
          return date.toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
          });
      }
    } catch {
      return 'Not available';
    }
  }

  /**
   * Calculate duration between two dates in days
   */
  calculateDaysBetween(startDate: string, endDate: string): number {
    const start = new Date(startDate);
    const end = new Date(endDate);
    const diffTime = Math.abs(end.getTime() - start.getTime());
    return Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1;
  }
}
