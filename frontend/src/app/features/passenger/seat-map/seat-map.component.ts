import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { PassengerService } from '../../../core/services/passenger.service';

@Component({
  selector: 'app-seat-map',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="seat-map-container">
      <h2>Select Your Seat</h2>
      <div class="grid">
        <button 
          *ngFor="let seat of seats" 
          class="seat" 
          [class.occupied]="seat.isOccupied"
          [class.selected]="seat.number === selectedSeat"
          [disabled]="seat.isOccupied"
          (click)="selectSeat(seat.number)">
          {{ seat.number }}
        </button>
      </div>
      <div class="actions" *ngIf="selectedSeat">
        <p>Selected Seat: {{ selectedSeat }}</p>
        <button class="btn btn-primary" (click)="proceedToCheckout()">Proceed to Checkout</button>
      </div>
    </div>
  `,
  styles: [`
    .seat-map-container { max-width: 600px; margin: 2rem auto; padding: 2rem; text-align: center; background: white; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }
    .grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 1rem; margin-top: 1.5rem; justify-items: center; }
    .seat { width: 50px; height: 50px; border-radius: 8px; border: 2px solid #ccc; background: #fff; cursor: pointer; font-weight: bold; }
    .seat:hover:not(.occupied) { border-color: #007bff; }
    .seat.selected { background: #007bff; color: white; border-color: #007bff; }
    .seat.occupied { background: #dc3545; color: white; border-color: #dc3545; cursor: not-allowed; opacity: 0.7; }
    .actions { margin-top: 2rem; }
    .btn { padding: 0.5rem 1rem; background: #28a745; color: white; border: none; border-radius: 4px; cursor: pointer; }
    .btn:hover { background: #218838; }
  `]
})
export class SeatMapComponent implements OnInit {
  tripId: string | null = null;
  seats: { number: number, isOccupied: boolean }[] = [];
  selectedSeat: number | null = null;

  constructor(private route: ActivatedRoute, private passengerService: PassengerService, private router: Router) {}

  ngOnInit() {
    this.tripId = this.route.snapshot.paramMap.get('id');
    if (this.tripId) {
      this.passengerService.getTripSeats(this.tripId).subscribe((res: any) => {
        const { capacity, occupiedSeats } = res;
        for (let i = 1; i <= capacity; i++) {
          this.seats.push({
            number: i,
            isOccupied: occupiedSeats.includes(i)
          });
        }
      });
    }
  }

  selectSeat(seatNumber: number) {
    this.selectedSeat = seatNumber;
  }

  proceedToCheckout() {
    if (this.selectedSeat && this.tripId) {
      this.router.navigate(['/checkout'], {
        queryParams: { tripId: this.tripId, seatNumber: this.selectedSeat }
      });
    }
  }
}
