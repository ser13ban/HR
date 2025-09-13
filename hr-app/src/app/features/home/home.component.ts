import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.sass'
})
export class HomeComponent {
  private authService = inject(AuthService);

  get currentUser() {
    return this.authService.currentUser;
  }

  logout(): void {
    this.authService.logout();
  }

  getRoleDisplayName(role: number): string {
    const roleNames = ['Employee', 'Manager', 'Admin'];
    return roleNames[role] || 'Unknown';
  }
}
