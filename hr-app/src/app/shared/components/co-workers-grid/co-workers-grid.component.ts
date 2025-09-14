import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { EmployeeService } from '../../../core/services/employee.service';
import { UtilityService } from '../../services/utility.service';
import { EmployeeListItem } from '../../../core/models/employee.models';

@Component({
  selector: 'app-co-workers-grid',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './co-workers-grid.component.html',
  styleUrls: ['./co-workers-grid.component.sass']
})
export class CoWorkersGridComponent implements OnInit {
  private readonly employeeService = inject(EmployeeService);
  private readonly router = inject(Router);
  private readonly utilityService = inject(UtilityService);

  // Reactive state using signals
  employees = signal<EmployeeListItem[]>([]);
  loading = signal<boolean>(false);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadEmployees();
  }

  private loadEmployees(): void {
    this.loading.set(true);
    this.error.set(null);

    this.employeeService.getAllEmployees().subscribe({
      next: (employees) => {
        this.employees.set(employees);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading employees:', error);
        this.error.set(error.message || 'Failed to load co-workers');
        this.loading.set(false);
      }
    });
  }

  onSeeProfile(employeeId: string): void {
    this.router.navigate(['/profile', employeeId]);
  }

  onRetry(): void {
    this.loadEmployees();
  }

  getProfilePictureUrl(employee: EmployeeListItem): string {
    return this.utilityService.getProfilePictureUrl(employee);
  }

  getRoleDisplayName(role: string): string {
    return this.utilityService.getRoleDisplayName(role);
  }

  trackByEmployeeId(index: number, employee: EmployeeListItem): string {
    return employee.id;
  }
}
