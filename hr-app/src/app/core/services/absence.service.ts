import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AbsenceRequest, CreateAbsenceRequest, ApprovalAction } from '../models/absence.models';

@Injectable({
  providedIn: 'root'
})
export class AbsenceService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5099/api/absence';

  getMyAbsenceRequests(): Observable<AbsenceRequest[]> {
    return this.http.get<AbsenceRequest[]>(`${this.apiUrl}/my-requests`);
  }

  getEmployeeAbsenceRequests(employeeId: number): Observable<AbsenceRequest[]> {
    return this.http.get<AbsenceRequest[]>(`${this.apiUrl}/employee/${employeeId}`);
  }

  getApprovedAbsenceRequests(): Observable<AbsenceRequest[]> {
    return this.http.get<AbsenceRequest[]>(`${this.apiUrl}/approved`);
  }

  getPendingApprovalsForManager(): Observable<AbsenceRequest[]> {
    return this.http.get<AbsenceRequest[]>(`${this.apiUrl}/pending-approvals`);
  }

  createAbsenceRequest(request: CreateAbsenceRequest): Observable<AbsenceRequest> {
    return this.http.post<AbsenceRequest>(this.apiUrl, request);
  }

  approveAbsenceRequest(requestId: number, approval: ApprovalAction): Observable<AbsenceRequest> {
    return this.http.put<AbsenceRequest>(`${this.apiUrl}/${requestId}/approve`, approval);
  }

  declineAbsenceRequest(requestId: number, approval: ApprovalAction): Observable<AbsenceRequest> {
    return this.http.put<AbsenceRequest>(`${this.apiUrl}/${requestId}/decline`, approval);
  }

  cancelAbsenceRequest(requestId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${requestId}`);
  }
}
