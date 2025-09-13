import { Routes } from '@angular/router';
import { AuthGuard, GuestGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  // Default redirect to home
  {
    path: '',
    redirectTo: '/home',
    pathMatch: 'full'
  },

  // Auth routes (only accessible when not authenticated)
  {
    path: 'login',
    canActivate: [GuestGuard],
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    canActivate: [GuestGuard],
    loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent)
  },

  // Protected routes (only accessible when authenticated)
  {
    path: 'home',
    canActivate: [AuthGuard],
    loadComponent: () => import('./features/home/home.component').then(m => m.HomeComponent)
  },
  {
    path: 'profile/:id',
    canActivate: [AuthGuard],
    loadComponent: () => import('./features/profile/profile.component').then(m => m.ProfileComponent)
  },

  // Wildcard route - redirect to home
  {
    path: '**',
    redirectTo: '/home'
  }
];
