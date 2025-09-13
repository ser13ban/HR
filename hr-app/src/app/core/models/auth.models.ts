export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  role: EmployeeRole;
  department?: string;
  team?: string;
  description?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  expires: string;
  user: User;
}

export interface User {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  department?: string;
  team?: string;
  description?: string;
  position?: string;
  hireDate?: string;
  bio?: string;
  profilePictureUrl?: string;
  role: EmployeeRole;
  fullName: string;
}

export enum EmployeeRole {
  Employee = 'Employee',
  Manager = 'Manager',
  Admin = 'Admin'
}

export interface ValidationError {
  field: string;
  message: string;
}

export interface ApiError {
  message: string;
  errors?: ValidationError[];
}
