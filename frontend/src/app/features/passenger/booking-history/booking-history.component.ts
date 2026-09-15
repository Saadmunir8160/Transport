import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PassengerService } from '../../../core/services/passenger.service';

@Component({
  selector: 'app-booking-history',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="history-container">
      <h2>My Bookings</h2>
      <div *ngIf="bookings.length === 0">You have no bookings.</div>
      
      <div class="ticket" *ngFor="let booking of bookings">
        <div class="ticket-header">
          <h3>{{ booking.routeName }}</h3>
          <span class="status" [class.confirmed]="booking.status === 'Confirmed'" [class.cancelled]="booking.status === 'Cancelled'">{{ booking.status }}</span>
        </div>
        <div class="ticket-body">
          <p><strong>Departure:</strong> {{ booking.departureTime | date:'medium' }}</p>
          <p><strong>Seat Number:</strong> {{ booking.seatNumber }}</p>
          <p><strong>Price:</strong> {{ booking.price | currency:'INR' }}</p>
          <p><strong>Booking Date:</strong> {{ booking.bookingDate | date:'short' }}</p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .history-container { max-width: 800px; margin: 2rem auto; padding: 1rem; }
    .ticket { background: white; margin-bottom: 1.5rem; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); overflow: hidden; border-left: 5px solid #007bff; }
    .ticket-header { background: #f8f9fa; padding: 1rem; display: flex; justify-content: space-between; align-items: center; border-bottom: 1px solid #eee; }
    .ticket-header h3 { margin: 0; }
    .status { padding: 0.25rem 0.75rem; border-radius: 20px; font-size: 0.85rem; font-weight: bold; }
    .status.confirmed { background: #d4edda; color: #155724; }
    .status.cancelled { background: #f8d7da; color: #721c24; }
    .ticket-body { padding: 1rem; }
  `]
})
export class BookingHistoryComponent implements OnInit {
  bookings: any[] = [];

  constructor(private passengerService: PassengerService) {}

  ngOnInit() {
    this.passengerService.getMyBookings().subscribe((res: any[]) => {
      this.bookings = res;
    });
  }
}
