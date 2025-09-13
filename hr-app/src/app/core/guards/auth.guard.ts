import { Injectable, inject } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  private authService = inject(AuthService);
  private router = inject(Router);

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): boolean {
    if (this.authService.isUserAuthenticated()) {
      return true;
    }

    // Store the attempted URL for redirecting after login
    this.router.navigate(['/login'], { 
      queryParams: { returnUrl: state.url } 
    });
    
    return false;
  }
}

@Injectable({
  providedIn: 'root'
})
export class GuestGuard implements CanActivate {
  private authService = inject(AuthService);
  private router = inject(Router);

  canActivate(): boolean {
    if (!this.authService.isUserAuthenticated()) {
      return true;
    }

    // Redirect authenticated users to home page
    this.router.navigate(['/']);
    return false;
  }
}
