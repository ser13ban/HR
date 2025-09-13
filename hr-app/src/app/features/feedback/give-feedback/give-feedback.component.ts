import { Component, Input, Output, EventEmitter, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { FeedbackService } from '../../../core/services/feedback.service';
import { CreateFeedbackDto, FeedbackType, FeedbackTypeLabels } from '../../../core/models/feedback.models';

@Component({
  selector: 'app-give-feedback',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './give-feedback.component.html',
  styleUrls: ['./give-feedback.component.sass']
})
export class GiveFeedbackComponent implements OnInit {
  @Input() toEmployeeId!: number;
  @Input() toEmployeeName!: string;
  @Input() isVisible = false;
  @Output() feedbackSubmitted = new EventEmitter<void>();
  @Output() modalClosed = new EventEmitter<void>();

  private readonly fb = inject(FormBuilder);
  private readonly feedbackService = inject(FeedbackService);

  feedbackForm!: FormGroup;
  isSubmitting = signal<boolean>(false);
  error = signal<string | null>(null);
  selectedRating = signal<number>(5);

  // Expose enums to template
  readonly FeedbackType = FeedbackType;
  readonly FeedbackTypeLabels = FeedbackTypeLabels;
  readonly feedbackTypes = Object.values(FeedbackType).filter(v => typeof v === 'number') as FeedbackType[];

  ngOnInit(): void {
    this.initializeForm();
  }

  private initializeForm(): void {
    this.feedbackForm = this.fb.group({
      content: ['', [Validators.required, Validators.maxLength(1000)]],
      type: [FeedbackType.General, [Validators.required]],
      rating: [5, [Validators.required, Validators.min(1), Validators.max(10)]],
      isAnonymous: [false]
    });

    // Watch rating changes
    this.feedbackForm.get('rating')?.valueChanges.subscribe(value => {
      this.selectedRating.set(value || 5);
    });
  }

  onSubmit(): void {
    if (this.feedbackForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.error.set(null);

    const formValue = this.feedbackForm.value;
    const createFeedbackDto: CreateFeedbackDto = {
      toEmployeeId: this.toEmployeeId,
      content: formValue.content.trim(),
      type: Number(formValue.type), // Convert to number (handles both string and number input)
      rating: Number(formValue.rating), // Convert to number
      isAnonymous: formValue.isAnonymous
    };

    // Debug logging (remove in production)
    console.log('Sending feedback:', createFeedbackDto);

    this.feedbackService.createFeedback(createFeedbackDto).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.feedbackForm.reset({
          type: FeedbackType.General,
          rating: 5,
          isAnonymous: false
        });
        this.selectedRating.set(5);
        this.feedbackSubmitted.emit();
        this.closeModal();
      },
      error: (error) => {
        this.error.set(error.message || 'Failed to submit feedback');
        this.isSubmitting.set(false);
      }
    });
  }

  closeModal(): void {
    this.feedbackForm.reset({
      type: FeedbackType.General,
      rating: 5,
      isAnonymous: false
    });
    this.selectedRating.set(5);
    this.error.set(null);
    this.modalClosed.emit();
  }

  onBackdropClick(event: Event): void {
    if (event.target === event.currentTarget) {
      this.closeModal();
    }
  }

  private markFormGroupTouched(): void {
    Object.keys(this.feedbackForm.controls).forEach(key => {
      const control = this.feedbackForm.get(key);
      control?.markAsTouched();
    });
  }

  getFieldError(fieldName: string): string | null {
    const field = this.feedbackForm.get(fieldName);
    if (field?.touched && field?.errors) {
      if (field.errors['required']) {
        return `${this.getFieldLabel(fieldName)} is required`;
      }
      if (field.errors['maxlength']) {
        return `${this.getFieldLabel(fieldName)} must be less than ${field.errors['maxlength'].requiredLength} characters`;
      }
      if (field.errors['min']) {
        return `Rating must be at least ${field.errors['min'].min}`;
      }
      if (field.errors['max']) {
        return `Rating must be at most ${field.errors['max'].max}`;
      }
    }
    return null;
  }

  private getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      content: 'Feedback content',
      type: 'Feedback type',
      rating: 'Rating'
    };
    return labels[fieldName] || fieldName;
  }

  getRatingStars(rating: number): string[] {
    const stars: string[] = [];
    for (let i = 1; i <= 10; i++) {
      stars.push(i <= rating ? '★' : '☆');
    }
    return stars;
  }

  setRating(rating: number): void {
    this.feedbackForm.patchValue({ rating });
    this.selectedRating.set(rating);
  }

  getTypeLabel(type: FeedbackType): string {
    return FeedbackTypeLabels[type];
  }

  getCharacterCount(): number {
    return this.feedbackForm.get('content')?.value?.length || 0;
  }
}
