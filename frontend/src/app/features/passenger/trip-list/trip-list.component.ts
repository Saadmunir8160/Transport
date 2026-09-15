import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { PassengerService } from '../../../core/services/passenger.service';

@Component({
  selector: 'app-trip-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="list-container">
      <h2>Available Trips</h2>
      <div *ngIf="trips.length === 0">No trips found for this date.</div>
      <div class="trip-card" *ngFor="let trip of trips">
        <h3>{{ trip.routeName }}</h3>
        <p><strong>Departure:</strong> {{ trip.departureTime | date:'short' }}</p>
        <p><strong>Arrival:</strong> {{ trip.estimatedArrivalTime | date:'short' }}</p>
        <p><strong>Vehicle:</strong> {{ trip.vehicleType }} (Capacity: {{ trip.capacity }})</p>
        <p><strong>Price:</strong> {{ trip.price | currency:'INR' }}</p>
        <button class="btn btn-success" (click)="selectSeat(trip.id)">Select Seat</button>
      </div>
    </div>
  `,
  styles: [`
    .list-container { max-width: 800px; margin: 2rem auto; padding: 1rem; }
    .trip-card { background: white; padding: 1.5rem; margin-bottom: 1rem; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
    .btn { padding: 0.5rem 1rem; color: white; border: none; border-radius: 4px; cursor: pointer; }
    .btn-success { background: #28a745; }
    .btn-success:hover { background: #218838; }
  `]
})
export class TripListComponent implements OnInit {
  trips: any[] = [];

  constructor(private route: ActivatedRoute, private passengerService: PassengerService, private router: Router) {}

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      const { originId, destinationId, date } = params;
      if (originId && destinationId && date) {
        this.passengerService.searchTrips(originId, destinationId, date)
          .subscribe((res: any[]) => {
            this.trips = res;
          });
      }
    });
  }

  selectSeat(tripId: string) {
    this.router.navigate(['/seat-map', tripId]);
  }
}
