import { Injectable, signal, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, BehaviorSubject, throwError } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';
import { 
  RegisterRequest, 
  LoginRequest, 
  AuthResponse, 
  User, 
  ApiError 
} from '../models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly API_URL = 'http://localhost:5099/api/auth';
  private readonly TOKEN_KEY = 'hr_auth_token';
  private readonly USER_KEY = 'hr_current_user';

  // Reactive state using signals
  private currentUserSignal = signal<User | null>(null);
  private isAuthenticatedSignal = signal<boolean>(false);
  
  // Observable for backward compatibility
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.initializeAuth();
  }

  // Signal getters
  get currentUser() {
    return this.currentUserSignal.asReadonly();
  }

  get isAuthenticated() {
    return this.isAuthenticatedSignal.asReadonly();
  }

  private initializeAuth(): void {
    if (isPlatformBrowser(this.platformId)) {
      const token = this.getStoredToken();
      const user = this.getStoredUser();
      
      if (token && user && !this.isTokenExpired(token)) {
        this.setCurrentUser(user);
      } else {
        this.clearAuth();
      }
    }
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    // Send the request as-is to the backend - backend will validate password confirmation
    return this.http.post<AuthResponse>(`${this.API_URL}/register`, request)
      .pipe(
        tap(response => this.handleAuthSuccess(response)),
        catchError(this.handleError)
      );
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.API_URL}/login`, request)
      .pipe(
        tap(response => this.handleAuthSuccess(response)),
        catchError(this.handleError)
      );
  }

  logout(): void {
    this.clearAuth();
    this.router.navigate(['/login']);
  }

  getCurrentUser(): User | null {
    return this.currentUserSignal();
  }

  isUserAuthenticated(): boolean {
    return this.isAuthenticatedSignal();
  }

  getToken(): string | null {
    return this.getStoredToken();
  }

  refreshToken(): Observable<AuthResponse> {
    // For now, we'll redirect to login if token is expired
    // In a real app, you might implement refresh token logic
    this.logout();
    return throwError(() => new Error('Session expired'));
  }

  private handleAuthSuccess(response: AuthResponse): void {
    this.storeToken(response.token);
    this.storeUser(response.user);
    this.setCurrentUser(response.user);
  }

  private setCurrentUser(user: User): void {
    this.currentUserSignal.set(user);
    this.isAuthenticatedSignal.set(true);
    this.currentUserSubject.next(user);
  }

  private clearAuth(): void {
    this.removeStoredToken();
    this.removeStoredUser();
    this.currentUserSignal.set(null);
    this.isAuthenticatedSignal.set(false);
    this.currentUserSubject.next(null);
  }

  private storeToken(token: string): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem(this.TOKEN_KEY, token);
    }
  }

  private getStoredToken(): string | null {
    if (isPlatformBrowser(this.platformId)) {
      return localStorage.getItem(this.TOKEN_KEY);
    }
    return null;
  }

  private removeStoredToken(): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem(this.TOKEN_KEY);
    }
  }

  private storeUser(user: User): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem(this.USER_KEY, JSON.stringify(user));
    }
  }

  private getStoredUser(): User | null {
    if (isPlatformBrowser(this.platformId)) {
      const userJson = localStorage.getItem(this.USER_KEY);
      return userJson ? JSON.parse(userJson) : null;
    }
    return null;
  }

  private removeStoredUser(): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem(this.USER_KEY);
    }
  }

  private isTokenExpired(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const expiry = payload.exp * 1000; // Convert to milliseconds
      return Date.now() > expiry;
    } catch {
      return true; // If we can't parse the token, consider it expired
    }
  }

  private handleError = (error: HttpErrorResponse): Observable<never> => {
    let apiError: ApiError;

    if (error.error instanceof ErrorEvent) {
      // Client-side error
      apiError = {
        message: 'A network error occurred. Please check your connection and try again.'
      };
    } else {
      // Server-side error
      if (error.status === 400 && error.error?.errors) {
        // Validation errors
        const validationErrors = Object.keys(error.error.errors).map(key => ({
          field: key.toLowerCase(),
          message: error.error.errors[key][0]
        }));
        apiError = {
          message: error.error.message || 'Validation failed',
          errors: validationErrors
        };
      } else if (error.status === 401) {
        apiError = {
          message: error.error?.message || 'Invalid credentials'
        };
      } else if (error.status === 0) {
        apiError = {
          message: 'Unable to connect to the server. Please check if the server is running.'
        };
      } else {
        apiError = {
          message: error.error?.message || 'An unexpected error occurred'
        };
      }
    }

    return throwError(() => apiError);
  };
}
