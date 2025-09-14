import { EmployeeRole } from './auth.models';

export interface EmployeeListItem {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  department: string;
  team: string;
  position: string;
  profilePictureUrl?: string;
  role: EmployeeRole;
}

export interface EmployeeProfile {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phoneNumber?: string;
  department: string;
  team: string;
  position: string;
  startDate: string;
  profilePictureUrl?: string;
  role: EmployeeRole;
  dateOfBirth?: string;
  address?: string;
  emergencyContact?: string;
  emergencyPhone?: string;
  isLimitedView: boolean;
}

export interface UpdateEmployeeRequest {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  department?: string;
  team?: string;
  position?: string;
  bio?: string;
  profilePictureUrl?: string;
  dateOfBirth?: string;
  address?: string;
  emergencyContact?: string;
  emergencyPhone?: string;
}
