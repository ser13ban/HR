import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { FeedbackListDto, CreateFeedbackDto, FeedbackDetailDto } from '../models/feedback.models';
@Injectable({
  providedIn: 'root'
})
export class FeedbackService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5099/api/feedback';

  getFeedbackForEmployee(employeeId: number): Observable<FeedbackListDto[]> {
    return this.http.get<FeedbackListDto[]>(`${this.apiUrl}/received/${employeeId}`)
      .pipe(
        map(feedbacks => feedbacks.map(feedback => ({
          ...feedback,
          createdAt: new Date(feedback.createdAt).toISOString(),
          updatedAt: new Date(feedback.updatedAt).toISOString()
        }))),
        catchError(this.handleError)
      );
  }

  getGivenFeedback(employeeId: number): Observable<FeedbackListDto[]> {
    return this.http.get<FeedbackListDto[]>(`${this.apiUrl}/given/${employeeId}`)
      .pipe(
        map(feedbacks => feedbacks.map(feedback => ({
          ...feedback,
          createdAt: new Date(feedback.createdAt).toISOString(),
          updatedAt: new Date(feedback.updatedAt).toISOString()
        }))),
        catchError(this.handleError)
      );
  }

  getFeedbackById(id: number): Observable<FeedbackDetailDto> {
    return this.http.get<FeedbackDetailDto>(`${this.apiUrl}/${id}`)
      .pipe(
        map(feedback => ({
          ...feedback,
          createdAt: new Date(feedback.createdAt).toISOString(),
          updatedAt: new Date(feedback.updatedAt).toISOString()
        })),
        catchError(this.handleError)
      );
  }

  createFeedback(feedback: CreateFeedbackDto): Observable<FeedbackDetailDto> {
    return this.http.post<FeedbackDetailDto>(this.apiUrl, feedback)
      .pipe(
        map(createdFeedback => ({
          ...createdFeedback,
          createdAt: new Date(createdFeedback.createdAt).toISOString(),
          updatedAt: new Date(createdFeedback.updatedAt).toISOString()
        })),
        catchError(this.handleError)
      );
  }

  canViewFeedback(employeeId: number): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/can-view/${employeeId}`)
      .pipe(catchError(this.handleError));
  }

  canGiveFeedback(employeeId: number): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/can-give/${employeeId}`)
      .pipe(catchError(this.handleError));
  }

  private handleError = (error: HttpErrorResponse): Observable<never> => {
    let errorMessage = 'An unknown error occurred';
    
    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = error.error.message;
    } else {
      // Server-side error
      switch (error.status) {
        case 400:
          errorMessage = error.error?.message || 'Bad request';
          break;
        case 401:
          errorMessage = 'You are not authorized to perform this action';
          break;
        case 403:
          errorMessage = error.error || 'Access forbidden';
          break;
        case 404:
          errorMessage = 'Feedback not found';
          break;
        case 500:
          errorMessage = 'Server error occurred';
          break;
        default:
          errorMessage = `Error: ${error.status} - ${error.statusText}`;
      }
    }
    
    return throwError(() => new Error(errorMessage));
  };
}
