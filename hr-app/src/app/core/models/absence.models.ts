export interface AbsenceRequest {
  id: number;
  employeeId: number;
  employeeName: string;
  employeeEmail: string;
  type: AbsenceType;
  startDate: string;
  endDate: string;
  reason?: string;
  status: AbsenceStatus;
  createdAt: string;
  updatedAt: string;
  approvalNotes?: string;
  approvedById?: number;
  approvedByName?: string;
  approvedAt?: string;
  durationInDays: number;
}

export interface CreateAbsenceRequest {
  type: AbsenceType;
  startDate: string;
  endDate: string;
  reason: string;
}

export interface ApprovalAction {
  approverNotes?: string;
}

export enum AbsenceType {
  Vacation = 0,
  SickLeave = 1,
  PersonalLeave = 2,
  Other = 3
}

export enum AbsenceStatus {
  Pending = 0,
  Approved = 1,
  Rejected = 2,
  Cancelled = 3
}

export const AbsenceTypeLabels: Record<AbsenceType, string> = {
  [AbsenceType.Vacation]: 'Vacation',
  [AbsenceType.SickLeave]: 'Sick Leave',
  [AbsenceType.PersonalLeave]: 'Personal Leave',
  [AbsenceType.Other]: 'Other'
};

export const AbsenceStatusLabels: Record<AbsenceStatus, string> = {
  [AbsenceStatus.Pending]: 'Pending',
  [AbsenceStatus.Approved]: 'Approved',
  [AbsenceStatus.Rejected]: 'Rejected',
  [AbsenceStatus.Cancelled]: 'Cancelled'
};

export const AbsenceStatusColors: Record<AbsenceStatus, string> = {
  [AbsenceStatus.Pending]: 'warning',
  [AbsenceStatus.Approved]: 'success',
  [AbsenceStatus.Rejected]: 'danger',
  [AbsenceStatus.Cancelled]: 'secondary'
};
