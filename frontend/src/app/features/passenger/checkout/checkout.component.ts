import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { PassengerService } from '../../../core/services/passenger.service';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="checkout-container">
      <h2>Checkout</h2>
      <div class="details" *ngIf="tripId && seatNumber">
        <p><strong>Trip ID:</strong> {{ tripId }}</p>
        <p><strong>Seat Number:</strong> {{ seatNumber }}</p>
        
        <div class="alert alert-danger" *ngIf="errorMessage">{{ errorMessage }}</div>
        
        <button class="btn btn-primary" (click)="confirmBooking()" [disabled]="isProcessing">
          {{ isProcessing ? 'Processing...' : 'Confirm & Pay' }}
        </button>
      </div>
    </div>
  `,
  styles: [`
    .checkout-container { max-width: 500px; margin: 2rem auto; padding: 2rem; background: white; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); text-align: center; }
    .details { margin-top: 1.5rem; text-align: left; }
    .btn { padding: 0.75rem 1.5rem; width: 100%; margin-top: 1.5rem; background: #007bff; color: white; border: none; border-radius: 4px; font-size: 1.1rem; cursor: pointer; }
    .btn:hover:not(:disabled) { background: #0056b3; }
    .btn:disabled { opacity: 0.7; cursor: not-allowed; }
    .alert { padding: 1rem; border-radius: 4px; margin-top: 1rem; }
    .alert-danger { background: #f8d7da; color: #721c24; border: 1px solid #f5c6cb; }
  `]
})
export class CheckoutComponent implements OnInit {
  tripId: string | null = null;
  seatNumber: number | null = null;
  isProcessing = false;
  errorMessage = '';

  constructor(private route: ActivatedRoute, private passengerService: PassengerService, private router: Router) {}

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.tripId = params['tripId'];
      this.seatNumber = Number(params['seatNumber']);
    });
  }

  confirmBooking() {
    if (!this.tripId || !this.seatNumber) return;
    
    this.isProcessing = true;
    this.errorMessage = '';

    this.passengerService.createBooking({
      tripId: this.tripId,
      seatNumber: this.seatNumber
    }).subscribe({
      next: (res: any) => {
        this.isProcessing = false;
        alert('Booking confirmed!');
        this.router.navigate(['/my-bookings']);
      },
      error: (err: any) => {
        this.isProcessing = false;
        this.errorMessage = err.error || 'An error occurred during booking. Please try again.';
      }
    });
  }
}
