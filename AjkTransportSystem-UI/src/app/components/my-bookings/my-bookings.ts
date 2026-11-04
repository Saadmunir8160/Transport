import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { PassengerService } from '../../services/passenger.service';
import { AuthGuardService } from '../../services/auth-guard';

@Component({
  selector: 'app-my-bookings',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './my-bookings.html',
  styleUrls: ['./my-bookings.css']
})
export class MyBookingsComponent implements OnInit {
  bookings: any[] = [];
  userName = '';
  activeBookingsCount = 0;
  showCancelledMessage = false;

  constructor(
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
    this.userName = user.name;
    this.loadBookings();
  }

  loadBookings() {
    this.passengerService.getMyBookings().subscribe({
      next: (data) => {
        this.bookings = data;
        this.activeBookingsCount = data.filter((b: any) => b.status !== 'Cancelled').length;
      },
      error: (err) => console.error(err)
    });
  }

  printTicket(bookingId: number) {
    this.passengerService.getTicket(bookingId).subscribe(ticket => {
      const printWindow = window.open('', '_blank');
      if (printWindow) {
        printWindow.document.write(`
          <html>
            <head>
              <title>Ticket - Booking #${ticket.bookingId}</title>
              <style>
                body { font-family: Arial; padding: 20px; }
                .ticket { border: 2px solid #000; padding: 30px; max-width: 600px; margin: 0 auto; }
                .header { text-align: center; border-bottom: 2px dashed #000; padding-bottom: 20px; margin-bottom: 20px; }
                .header h1 { margin: 0; color: #2563eb; }
                .row { display: flex; justify-content: space-between; margin: 10px 0; }
                .label { font-weight: bold; }
                .qr { text-align: center; margin-top: 20px; padding: 20px; background: #f0f0f0; }
                @media print { button { display: none; } }
              </style>
            </head>
            <body>
              <div class="ticket">
                <div class="header">
                  <h1>🚌 AJK TRANSPORT SYSTEM</h1>
                  <p>Bus Ticket</p>
                </div>
                <div class="row"><span class="label">Booking ID:</span><span>#${ticket.bookingId}</span></div>
                <div class="row"><span class="label">Passenger:</span><span>${ticket.passengerName}</span></div>
                <div class="row"><span class="label">CNIC:</span><span>${ticket.passengerCNIC}</span></div>
                <div class="row"><span class="label">Phone:</span><span>${ticket.passengerPhone}</span></div>
                <div class="row"><span class="label">Route:</span><span>${ticket.routeName}</span></div>
                <div class="row"><span class="label">From:</span><span>${ticket.startPoint}</span></div>
                <div class="row"><span class="label">To:</span><span>${ticket.endPoint}</span></div>
                <div class="row"><span class="label">Travel Date:</span><span>${new Date(ticket.travelDate).toLocaleDateString()} at ${ticket.departureTime || 'N/A'}</span></div>
                <div class="row"><span class="label">Seats:</span><span>${ticket.seatNumber} (${ticket.seats} total)</span></div>
                <div class="row"><span class="label">Total Fare:</span><span>Rs. ${ticket.totalFare}</span></div>
                <div class="row"><span class="label">Status:</span><span style="color: green;">${ticket.status}</span></div>
                <div class="qr">
                  <p><strong>QR Code:</strong></p>
                  <p style="font-size: 12px;">${ticket.qrCode}</p>
                </div>
                <p style="text-align: center; margin-top: 20px; font-size: 12px;">Thank you for choosing AJK Transport! Safe Journey!</p>
                <button onclick="window.print()" style="width: 100%; padding: 10px; background: #2563eb; color: white; border: none; cursor: pointer; margin-top: 20px;">Print Ticket</button>
              </div>
            </body>
          </html>
        `);
        printWindow.document.close();
      }
    });
  }

  cancelBooking(id: number) {
    if (confirm('Are you sure you want to cancel this booking?')) {
      this.passengerService.cancelTrip(id).subscribe({
        next: (response) => {
          console.log('Booking cancelled:', response);
          this.loadBookings();
          this.showCancelledMessage = true;
          setTimeout(() => this.showCancelledMessage = false, 5000);
        },
        error: (err) => {
          console.error('Error cancelling booking:', err);
          alert('Failed to cancel booking! ' + (err.error?.message || err.message));
        }
      });
    }
  }

  logout() {
    localStorage.removeItem('user');
    this.router.navigate(['/']);
  }
}
