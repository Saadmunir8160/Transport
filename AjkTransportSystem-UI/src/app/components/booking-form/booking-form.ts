import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PassengerService } from '../../services/passenger.service';
import { AuthGuardService } from '../../services/auth-guard';

@Component({
  selector: 'app-booking-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './booking-form.html',
  styleUrls: ['./booking-form.css']
})
export class BookingFormComponent implements OnInit {
  tripId = 0;
  totalCapacity = 0;
  bookedSeats: string[] = [];
  selectedSeatsList: string[] = [];
  
  passengerName = '';
  passengerPhone = '';
  passengerCNIC = '';
  farePerSeat = 500; // Will be set from search page or hardcoded
  userId = 0;

  constructor(
    private activatedRoute: ActivatedRoute,
    private router: Router,
    private passengerService: PassengerService,
    private authGuard: AuthGuardService
  ) {}

  ngOnInit() {
    if (!this.authGuard.checkSession()) return;
    
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    if (!user.userId) {
      this.router.navigate(['/login']);
      return;
    }
    this.userId = user.userId;
    this.passengerName = user.name;

    this.tripId = this.activatedRoute.snapshot.params['id'];
    this.loadSeatMap();
  }

  loadSeatMap() {
    this.passengerService.getTripSeats(this.tripId).subscribe({
      next: (data) => {
        this.totalCapacity = data.totalCapacity;
        this.bookedSeats = data.bookedSeats || [];
      },
      error: (err) => {
        console.error('Error loading seats:', err);
        alert('Failed to load seat layout.');
      }
    });
  }

  getSeatArray(): number[] {
    return Array(this.totalCapacity).fill(0).map((x, i) => i + 1);
  }

  toggleSeat(seatNumber: number) {
    const seatStr = seatNumber.toString();
    if (this.bookedSeats.includes(seatStr)) return; // Already booked
    
    const index = this.selectedSeatsList.indexOf(seatStr);
    if (index > -1) {
      this.selectedSeatsList.splice(index, 1);
    } else {
      this.selectedSeatsList.push(seatStr);
    }
  }

  isSeatSelected(seatNumber: number): boolean {
    return this.selectedSeatsList.includes(seatNumber.toString());
  }

  isSeatBooked(seatNumber: number): boolean {
    return this.bookedSeats.includes(seatNumber.toString());
  }

  getTotalFare(): number {
    return this.farePerSeat * this.selectedSeatsList.length;
  }

  bookTicket() {
    if (this.selectedSeatsList.length === 0) {
      alert('Please select at least one seat.');
      return;
    }

    if (!this.passengerPhone || !this.passengerCNIC) {
      alert('Please provide Phone and CNIC.');
      return;
    }

    const payload = {
      tripId: this.tripId,
      passengerName: this.passengerName,
      passengerPhone: this.passengerPhone,
      passengerCNIC: this.passengerCNIC,
      selectedSeats: this.selectedSeatsList
    };

    this.passengerService.bookTrip(payload).subscribe({
      next: (response) => {
        alert(
          '✅ BOOKING CONFIRMED!\n\n' +
          '🎫 Booking ID: #' + response.bookingId + '\n' +
          '🪑 Seats: ' + this.selectedSeatsList.join(', ') + '\n' +
          '💰 Total: Rs. ' + response.totalFare + '\n\n' +
          '✅ Check "My Bookings" to print your ticket!'
        );
        this.router.navigate(['/my-bookings']);
      },
      error: (err) => {
        console.error('Booking error:', err);
        alert('❌ Booking failed!\n\nError: ' + (err.error?.message || err.message));
      }
    });
  }
}
