import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OfflineSyncService, TripDto } from '../../services/offline-sync.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-driver-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="driver-dashboard">
      <header class="header">
        <h1>Driver Dashboard</h1>
        <div class="status-indicator" [class.offline]="!(isOnline$ | async)">
          {{ (isOnline$ | async) ? 'Online' : 'Offline' }}
        </div>
      </header>

      <main class="content">
        <h2>My Assigned Trips</h2>
        
        <div *ngIf="loading" class="loading">Loading trips...</div>
        
        <div *ngIf="!loading && trips.length === 0" class="no-trips">
          No trips assigned for today.
        </div>

        <div class="trip-list" *ngIf="!loading">
          <div class="trip-card" *ngFor="let trip of trips">
            <div class="trip-info">
              <h3>Route ID: {{ trip.routeId }}</h3>
              <p>Departure: {{ trip.departureTime | date:'short' }}</p>
              <p>Status: <span class="status-badge" [ngClass]="trip.status.toLowerCase()">{{ trip.status }}</span></p>
            </div>
            
            <div class="trip-actions" *ngIf="trip.status !== 'Completed'">
              <button 
                class="btn btn-primary" 
                *ngIf="trip.status === 'Scheduled'"
                (click)="updateStatus(trip.id, 'Active')">
                Start Trip
              </button>
              
              <button 
                class="btn btn-success" 
                *ngIf="trip.status === 'Active'"
                (click)="updateStatus(trip.id, 'Completed')">
                Complete Trip
              </button>
            </div>
          </div>
        </div>
      </main>
    </div>
  `,
  styles: [`
    .driver-dashboard {
      font-family: 'Inter', sans-serif;
      min-height: 100vh;
      background: #f4f7f6;
    }
    .header {
      background: #2b2d42;
      color: white;
      padding: 1rem 2rem;
      display: flex;
      justify-content: space-between;
      align-items: center;
      box-shadow: 0 4px 6px rgba(0,0,0,0.1);
    }
    .status-indicator {
      padding: 0.5rem 1rem;
      border-radius: 20px;
      background: #4caf50;
      font-weight: 500;
    }
    .status-indicator.offline {
      background: #f44336;
    }
    .content {
      padding: 2rem;
      max-width: 800px;
      margin: 0 auto;
    }
    .trip-card {
      background: white;
      border-radius: 12px;
      padding: 1.5rem;
      margin-bottom: 1rem;
      box-shadow: 0 4px 15px rgba(0,0,0,0.05);
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    .status-badge {
      padding: 0.2rem 0.6rem;
      border-radius: 12px;
      font-size: 0.85rem;
      font-weight: bold;
    }
    .status-badge.scheduled { background: #e0f2fe; color: #0284c7; }
    .status-badge.active { background: #dcfce7; color: #16a34a; }
    .status-badge.completed { background: #f3f4f6; color: #4b5563; }
    
    .btn {
      padding: 0.8rem 1.5rem;
      border: none;
      border-radius: 8px;
      cursor: pointer;
      font-weight: 600;
      transition: all 0.2s ease;
    }
    .btn-primary { background: #2b2d42; color: white; }
    .btn-primary:hover { background: #1a1c2b; }
    .btn-success { background: #10b981; color: white; }
    .btn-success:hover { background: #059669; }
    
    @media (max-width: 600px) {
      .trip-card { flex-direction: column; align-items: flex-start; }
      .trip-actions { margin-top: 1rem; width: 100%; }
      .btn { width: 100%; }
    }
  `]
})
export class DriverDashboardComponent implements OnInit {
  trips: TripDto[] = [];
  loading = true;
  isOnline$;

  constructor(private syncService: OfflineSyncService) {
    this.isOnline$ = this.syncService.isOnline$;
  }

  ngOnInit() {
    this.loadTrips();
  }

  loadTrips() {
    this.loading = true;
    this.syncService.fetchTrips().subscribe({
      next: (trips) => {
        this.trips = trips;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  updateStatus(tripId: string, status: string) {
    // Optimistic UI update
    const trip = this.trips.find(t => t.id === tripId);
    if (trip) trip.status = status;

    this.syncService.updateTripStatus(tripId, status).subscribe();
  }
}
