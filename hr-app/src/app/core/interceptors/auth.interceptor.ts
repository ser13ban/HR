import { inject } from '@angular/core';
import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  
  // Get the auth token from the service
  const authToken = authService.getToken();
  
  // Clone the request and add the authorization header if token exists
  let authRequest = req;
  if (authToken && !isAuthEndpoint(req.url)) {
    authRequest = req.clone({
      headers: req.headers.set('Authorization', `Bearer ${authToken}`)
    });
  }

  // Handle the request and catch any 401 errors
  return next(authRequest).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !isAuthEndpoint(req.url)) {
        // Token is invalid or expired, logout user
        authService.logout();
      }
      return throwError(() => error);
    })
  );
};

function isAuthEndpoint(url: string): boolean {
  return url.includes('/auth/login') || 
         url.includes('/auth/register') || 
         url.includes('/auth/validate');
}
