import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardService } from '../../../core/services/dashboard.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  private dashboardService = inject(DashboardService);
  stats: any = null;

  ngOnInit() {
    this.dashboardService.getSuperAdminStats().subscribe({
      next: (data) => this.stats = data,
      error: (err) => console.error('Failed to load stats', err)
    });
  }
}
