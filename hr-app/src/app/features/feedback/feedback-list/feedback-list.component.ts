import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FeedbackService } from '../../../core/services/feedback.service';
import { AuthService } from '../../../core/services/auth.service';
import { FeedbackListDto, FeedbackType, FeedbackTypeLabels } from '../../../core/models/feedback.models';

@Component({
  selector: 'app-feedback-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './feedback-list.component.html',
  styleUrls: ['./feedback-list.component.sass']
})
export class FeedbackListComponent implements OnInit {
  private readonly feedbackService = inject(FeedbackService);
  private readonly authService = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  feedbacks = signal<FeedbackListDto[]>([]);
  loading = signal<boolean>(false);
  error = signal<string | null>(null);
  employeeId = signal<number>(0);
  isOwnProfile = signal<boolean>(false);

  // Computed properties
  averageRating = computed(() => {
    const feedbackList = this.feedbacks();
    if (feedbackList.length === 0) return 0;
    const sum = feedbackList.reduce((acc, feedback) => acc + feedback.rating, 0);
    return Math.round((sum / feedbackList.length) * 10) / 10;
  });

  feedbackByType = computed(() => {
    const feedbackList = this.feedbacks();
    const grouped: Record<number, FeedbackListDto[]> = {};
    
    feedbackList.forEach(feedback => {
      if (!grouped[feedback.type]) {
        grouped[feedback.type] = [];
      }
      grouped[feedback.type].push(feedback);
    });
    
    return grouped;
  });

  // Expose enums to template
  readonly FeedbackType = FeedbackType;
  readonly FeedbackTypeLabels = FeedbackTypeLabels;

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      const employeeId = parseInt(params['employeeId'], 10);
      if (employeeId) {
        this.employeeId.set(employeeId);
        this.checkIfOwnProfile();
        this.loadFeedback();
      } else {
        this.error.set('Invalid employee ID');
      }
    });
  }

  private checkIfOwnProfile(): void {
    const currentUser = this.authService.currentUser();
    this.isOwnProfile.set(currentUser?.id === this.employeeId());
  }

  loadFeedback(): void {
    this.loading.set(true);
    this.error.set(null);

    this.feedbackService.getFeedbackForEmployee(this.employeeId()).subscribe({
      next: (feedbacks) => {
        this.feedbacks.set(feedbacks);
        this.loading.set(false);
      },
      error: (error) => {
        this.error.set(error.message || 'Failed to load feedback');
        this.loading.set(false);
      }
    });
  }

  getTypeLabel(type: FeedbackType): string {
    return FeedbackTypeLabels[type];
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  getRatingStars(rating: number): string[] {
    const fullStars = Math.floor(rating);
    const hasHalfStar = rating % 1 >= 0.5;
    const emptyStars = 10 - fullStars - (hasHalfStar ? 1 : 0);
    
    const stars: string[] = [];
    
    // Full stars
    for (let i = 0; i < fullStars; i++) {
      stars.push('★');
    }
    
    // Half star
    if (hasHalfStar) {
      stars.push('☆');
    }
    
    // Empty stars
    for (let i = 0; i < emptyStars; i++) {
      stars.push('☆');
    }
    
    return stars;
  }

  goBack(): void {
    this.router.navigate(['/profile', this.employeeId()]);
  }

  getObjectKeys(obj: any): string[] {
    return Object.keys(obj);
  }
}
