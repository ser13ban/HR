import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { EmployeeListItem, EmployeeProfile } from '../models/employee.models';

@Injectable({
  providedIn: 'root'
})
export class EmployeeService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5099/api/employee';

  getAllEmployees(): Observable<EmployeeListItem[]> {
    return this.http.get<EmployeeListItem[]>(this.apiUrl).pipe(
      catchError(this.handleError)
    );
  }

  getEmployeeById(id: string): Observable<EmployeeProfile> {
    return this.http.get<EmployeeProfile>(`${this.apiUrl}/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  private handleError = (error: HttpErrorResponse): Observable<never> => {
    let errorMessage = 'An unknown error occurred';
    
    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = `Error: ${error.error.message}`;
    } else {
      // Server-side error
      switch (error.status) {
        case 401:
          errorMessage = 'You are not authorized to access this resource';
          break;
        case 403:
          errorMessage = 'You do not have permission to access this resource';
          break;
        case 404:
          errorMessage = 'The requested employee was not found';
          break;
        case 500:
          errorMessage = 'Internal server error. Please try again later';
          break;
        default:
          errorMessage = `Error ${error.status}: ${error.error?.message || error.message}`;
      }
    }

    console.error('EmployeeService Error:', error);
    return throwError(() => new Error(errorMessage));
  };
}
