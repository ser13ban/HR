import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { UtilityService } from '../../shared/services/utility.service';
import { EmployeeRole } from '../../core/models/auth.models';
import { CoWorkersGridComponent } from '../../shared/components/co-workers-grid/co-workers-grid.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, CoWorkersGridComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.sass'
})
export class HomeComponent {
  private authService = inject(AuthService);
  private router = inject(Router);
  private utilityService = inject(UtilityService);

  get currentUser() {
    return this.authService.currentUser;
  }

  logout(): void {
    this.authService.logout();
  }

  navigateToProfile(): void {
    const currentUser = this.authService.getCurrentUser();
    if (currentUser) {
      this.router.navigate(['/profile', currentUser.id.toString()]);
    }
  }

  navigateToAbsences(): void {
    this.router.navigate(['/absence']);
  }

  navigateToFeedback(): void {
    const currentUser = this.authService.getCurrentUser();
    if (currentUser) {
      this.router.navigate(['/feedback', currentUser.id.toString()]);
    }
  }

  getRoleDisplayName(role: EmployeeRole): string {
    return this.utilityService.getRoleDisplayName(role);
  }
}
