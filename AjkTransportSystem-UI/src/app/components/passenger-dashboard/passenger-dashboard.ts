import { Component, OnInit, OnDestroy } from '@angular/core';
import * as L from 'leaflet';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { RouteService } from '../../services/route';
import { AiService } from '../../services/ai';
import { AuthService } from '../../services/auth';
import { WeatherService } from '../../services/weather';
import { FeedbackService } from '../../services/feedback';
import { AuthGuardService } from '../../services/auth-guard';
import { PassengerService } from '../../services/passenger.service';

@Component({
  selector: 'app-passenger-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './passenger-dashboard.html',
  styleUrls: ['./passenger-dashboard.css']
})
export class PassengerDashboardComponent implements OnInit, OnDestroy {
  // Leaflet Live Map State
  private liveMap: any = null;
  private busMarker: any = null;
  private routePolyline: any = null;
  private mapSimInterval: any = null;
  public mapSimulating: boolean = false;
  public busSpeedKmH: number = 42;
  public busCurrentStop: string = 'Approaching Kohala Bridge';
  public lastGpsUpdate: string = 'Live';

  // Major AJK Route Coordinates
  public ajkGeoCoords: { [key: string]: [number, number] } = {
    'Muzaffarabad': [34.3597, 73.4711],
    'Rawalakot': [33.8584, 73.7604],
    'Mirpur': [33.1484, 73.7519],
    'Kotli': [33.5156, 73.9019],
    'Bagh': [33.9816, 73.7760],
    'Bhimber': [32.9741, 74.0785],
    'Hattian Bala': [34.1691, 73.7432],
    'Rawalpindi': [33.5651, 73.0169],
    'Islamabad': [33.6844, 73.0479],
    'Kohala': [34.0950, 73.4975],
    'Murree': [33.9070, 73.3943]
  };
  // Register state
  regStep = 'details';
  regName = '';
  regPhone = '';
  regEmail = '';
  regPassword = '';
  regLanguage = 'English';
  regOtp = '';
  regOtpAttempts = 0;
  regTimer = 300;
  regTimerInterval: any;
  regLoading = false;
  regMessage = '';

  userName = '';
  userEmail = '';
  userId = 0;
  userRole = '';
  routes: any[] = [];
  trips: any[] = [];
  filteredTrips: any[] = [];
  recommendations: any[] = [];
  searchFrom = '';
  searchTo = '';
  searchDate = new Date().toISOString().split('T')[0];
  passengerCount = 1;
  showFilters = false;
  filterTimeRange = 'all';
  filterVehicleType = 'all';
  filterAvailableOnly = false;
  sortOption = 'earliest';
  activeTab = 'dashboard';
    cancelStep = 1;
    cancellingBooking: any = { id: 'BKG-784321', route: 'Muzaffarabad to Rawalpindi Express', date: new Date().toISOString(), time: '08:30 AM', amountPaid: 3050, refundAmount: 3050, refundPolicy: 'More than 60 mins before departure' };
  selectedTrip: any = {
    id: 1,
    routeName: 'Muzaffarabad to Rawalpindi Express',
    startPoint: 'Muzaffarabad',
    endPoint: 'Rawalpindi',
    travelDate: new Date().toISOString(),
    departureTime: '08:30 AM',
    eta: '01:00 PM',
    duration: '4h 30m',
    fare: 1500,
    availableSeats: 12,
    registration: 'AJK-4402',
    driverName: 'Ahmed',
    advisory: 'Landslide reported near Muzaffarabad. Expect 30 mins delay.',
    advisoryType: 'Landslide',
    advisorySegment: 'Kohala Bridge to Muzaffarabad',
    advisoryDelay: '30 - 45 minutes',
    status: 'scheduled'
  };
  selectedSeats: any[] = [{ id: '1A', status: 'selected', type: 'Window' }, { id: '1B', status: 'selected', type: 'Aisle' }];
  seatMap: any[][] = [];


  // New variables
  trackingBooking: any = null;
  trackingData: any = null;
  bookingId = '';
  ticketCode = '';
  paymentMethod = 'Easypaisa';
  showPaymentModal = false;
  paymentAccountNo = '03451234567';
  paymentCardNo = '4242 •••• •••• 4242';
  paymentCardExpiry = '12/28';
  paymentCardCvc = '321';
  paymentOtp = '';
  paymentOtpSent = false;
  paymentProcessing = false;
  paymentSuccess = false;
  paymentTxnId = '';
  paymentReceiptDate = '';
  smartSearchQuery = '';
  sidebarActive = false;
  feedbackMessage = '';
  feedbackRating = 0;
  hoveredRating = 0;
  selectedEmoji = '';
  feedbackSubmitted = false;

  upcomingTrip = {
    routeName: 'Muzaffarabad to Rawalpindi Express',
    travelDate: new Date(new Date().getTime() + 86400000), // Tomorrow
    departureTime: '08:30 AM',
    seatNumber: '14A',
    boardingStop: 'Main Terminal MZ',
    eta: '08:25 AM'
  };

  recentRoutes = [
    { startPoint: 'Muzaffarabad', endPoint: 'Mirpur' },
    { startPoint: 'Rawalakot', endPoint: 'Islamabad' },
    { startPoint: 'Bagh', endPoint: 'Muzaffarabad' }
  ];


  activeBookingsTab = 'upcoming';
  myBookings: any[] = [
    {
      id: 'AJK-839212',
      route: 'Muzaffarabad to Rawalpindi Express',
      date: new Date(new Date().getTime() + 86400000), // Tomorrow
      time: '08:30 AM',
      seats: '14A, 14B',
      startPoint: 'Main Terminal MZ',
      endPoint: 'RWP Central',
      status: 'upcoming',
      amount: 2450,
      canCancel: true
    },
    {
      id: 'AJK-839910',
      route: 'Srinagar to Anantnag Fast',
      date: new Date(), // Today
      time: '12:00 PM',
      seats: '3C',
      startPoint: 'Srinagar HQ',
      endPoint: 'Anantnag Bus Stand',
      status: 'active',
      amount: 1550,
      canCancel: false
    },
    {
      id: 'AJK-102931',
      route: 'Bagh to Muzaffarabad',
      date: new Date(new Date().getTime() - 86400000 * 5), // 5 days ago
      time: '10:00 AM',
      seats: '5A',
      startPoint: 'Bagh Main',
      endPoint: 'Terminal MZ',
      status: 'completed',
      amount: 800,
      canCancel: false,
      actualArrivalTime: '01:15 PM',
      delay: '15 mins'
    },
    {
      id: 'AJK-091823',
      route: 'Mirpur to Bhimber',
      date: new Date(new Date().getTime() - 86400000 * 10), // 10 days ago
      time: '09:00 AM',
      seats: '10B, 10C',
      startPoint: 'Mirpur Chowk',
      endPoint: 'Bhimber City',
      status: 'cancelled',
      amount: 1200,
      canCancel: false,
      reason: 'Passenger request',
      refundStatus: 'Refunded to UPI'
    }
  ];

  get filteredMyBookings() {
    return this.myBookings.filter(b => b.status === this.activeBookingsTab);
  }

  constructor(
    private router: Router, 
    private routeService: RouteService,
    private aiService: AiService,
    private authService: AuthService,
    private weatherService: WeatherService,
    private feedbackService: FeedbackService,
    private passengerService: PassengerService,
    private authGuard: AuthGuardService
  ) {}

  
  requestOtp() {
    this.regMessage = '';
    if (!this.regName || !this.regPhone || !this.regPassword) {
      this.regMessage = 'Please fill all required fields';
      return;
    }
    if (this.regPassword.length < 8) {
      this.regMessage = 'Password must be at least 8 characters';
      return;
    }

    this.regLoading = true;
    
    this.authService.register({ phone: this.regPhone }).subscribe({
      next: (res: any) => {
        this.regLoading = false;
        this.regStep = 'otp';
        this.regMessage = '';
        this.startRegTimer();
      },
      error: (err: any) => {
        this.regLoading = false;
        // Mock success for UI demo
        this.regStep = 'otp';
        this.regMessage = '';
        this.startRegTimer();
      }
    });
  }

  startRegTimer() {
    this.regTimer = 300;
    clearInterval(this.regTimerInterval);
    this.regTimerInterval = setInterval(() => {
      this.regTimer--;
      if (this.regTimer <= 0) clearInterval(this.regTimerInterval);
    }, 1000);
  }

  getFormattedTimer() {
    const m = Math.floor(this.regTimer / 60);
    const s = this.regTimer % 60;
    return m + ':' + (s < 10 ? '0' : '') + s;
  }

  verifyOtp() {
    if (this.regOtp.length !== 6) {
      this.regMessage = 'Enter a valid 6-digit OTP';
      return;
    }
    this.regLoading = true;
    const payload = {
      name: this.regName,
      phone: this.regPhone,
      email: this.regEmail,
      password: this.regPassword,
      languagePreference: this.regLanguage,
      otp: this.regOtp,
      role: 'Passenger'
    };

    this.authService.register(payload).subscribe({
      next: (res: any) => {
        this.regLoading = false;
        alert('Registration successful! You are now logged in.');
        this.activeTab = 'dashboard';
      },
      error: (err: any) => {
        this.regLoading = false;
        // Mock success for UI demo
        alert('Registration successful! You are now logged in.');
        this.activeTab = 'dashboard';
      }
    });
  }

  ngOnInit() {
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    if (!user.userId) {
      this.router.navigate(['/login']);
      return;
    }
    this.userName = user.name;
    this.userEmail = user.email;
    this.userId = user.userId;
    this.userRole = user.role;
    this.loadRoutes();
    this.loadRecommendations();
  }

  getPageTitle(): string {
    const titles: any = {
      home: 'Welcome back!',
      search: 'Search Routes',
      bookings: 'My Bookings',
      profile: 'My Profile',
      feedback: 'Feedback'
    };
    return titles[this.activeTab] || 'Dashboard';
  }

  loadRoutes() {
    this.passengerService.searchTrips('', '', '').subscribe({
      next: (data) => {
        console.log('Trips loaded:', data);
        this.trips = data;
        this.filteredTrips = data;
        this.loadWeatherForTrips();
        this.generateInitialRecommendations();
      },
      error: (err) => {
        console.error('Error loading trips:', err);
        alert('Failed to load trips.');
      }
    });
  }

  generateInitialRecommendations() {
    if (this.trips.length > 0) {
      this.recommendations = this.trips.slice(0, 3).map(trip => ({
        ...trip,
        recommendation: this.getRecommendation(trip)
      }));
    }
  }

  loadWeatherForTrips() {
    this.filteredTrips.forEach(trip => {
      this.weatherService.getRouteWeather(trip.startPoint, trip.endPoint).subscribe(weather => {
        trip.weather = weather;
      });
    });
  }

  loadRecommendations() {
    // This method is called from ngOnInit but recommendations are now generated in loadRoutes
  }

  
  passengerName = '';
  passengerPhone = '';
  passengerId = '';
  holdTimeLeft = 600;
  
  get holdTimerDisplay() {
    const m = Math.floor(this.holdTimeLeft / 60);
    const s = this.holdTimeLeft % 60;
    return `${m}:${s < 10 ? '0' : ''}${s}`;
  }

  goToTripDetail(trip: any) {
    this.selectedTrip = trip;
    this.passengerCount = 1;
    this.selectedSeats = [];
    this.generateSeatMap();
    this.activeTab = 'trip-detail';
  }

  generateSeatMap() {
    this.seatMap = [];
    const rows = 10;
    const cols = 4; // 2 seats, aisle, 2 seats
    
    for (let r = 0; r < rows; r++) {
      let rowSeats: any[] = [];
      let seatLetter = ['A', 'B', 'C', 'D'];
      for (let c = 0; c < cols; c++) {
        if (c === 2) {
          // Aisle
          rowSeats.push(null);
        }
        
        let colIndex = c < 2 ? c : c - 1;
        let seatId = (r + 1) + seatLetter[colIndex];
        
        // Randomly set some seats as booked or held for demo
        let status = 'available';
        if (Math.random() < 0.2) status = 'booked';
        else if (Math.random() < 0.1) status = 'held';

        rowSeats.push({
          id: seatId,
          type: (c === 0 || c === 3) ? 'Window' : 'Aisle',
          status: status
        });
      }
      this.seatMap.push(rowSeats);
    }
  }

  isSelected(seat: any) {
    return this.selectedSeats.includes(seat);
  }

  toggleSeat(seat: any) {
    if (this.isSelected(seat)) {
      this.selectedSeats = this.selectedSeats.filter(s => s !== seat);
    } else {
      if (this.selectedSeats.length < this.passengerCount) {
        this.selectedSeats.push(seat);
      }
    }
  }

  goToCheckout() {
    this.activeTab = 'checkout';
    this.holdTimeLeft = 600;
    setInterval(() => {
      if (this.holdTimeLeft > 0) this.holdTimeLeft--;
    }, 1000);
  }


  openPaymentModal() {
    this.showPaymentModal = true;
    this.paymentOtpSent = false;
    this.paymentProcessing = false;
    this.paymentSuccess = false;
    this.paymentOtp = '';
    this.paymentTxnId = 'TXN-' + Math.floor(10000000 + Math.random() * 90000000);
    this.paymentReceiptDate = new Date().toLocaleString();
  }

  closePaymentModal() {
    if (!this.paymentProcessing) {
      this.showPaymentModal = false;
    }
  }

  sendPaymentOtp() {
    this.paymentProcessing = true;
    setTimeout(() => {
      this.paymentProcessing = false;
      this.paymentOtpSent = true;
      this.paymentOtp = '4829';
    }, 1200);
  }

  processGatewayPayment() {
    this.paymentProcessing = true;
    setTimeout(() => {
      this.paymentProcessing = false;
      this.paymentSuccess = true;
      this.bookingId = 'AJK-' + Math.floor(100000 + Math.random() * 900000);
      this.ticketCode = 'TKT-' + Math.floor(1000 + Math.random() * 9000);
      
      setTimeout(() => {
        this.showPaymentModal = false;
        this.activeTab = 'booking-confirmed';
      }, 1500);
    }, 1800);
  }

  printTicketReceipt() {
    window.print();
  }
  confirmPayment() {
    this.activeTab = 'booking-confirmed';
    this.bookingId = 'AJK-' + Math.floor(Math.random() * 100000);
    this.ticketCode = 'TKT-' + Math.floor(Math.random() * 10000);
  }

  searchRoutes() {
    this.passengerService.searchTrips(this.searchFrom, this.searchTo, this.searchDate).subscribe({
      next: (data) => {
        this.filteredTrips = data;
        this.loadWeatherForTrips();
      },
      error: (err) => console.error(err)
    });
  }

  smartSearch() {
    const query = this.smartSearchQuery.trim().toLowerCase();
    
    if (!query) {
      this.filteredTrips = this.trips;
      this.generateInitialRecommendations();
      return;
    }

    const searchResults = this.trips.filter(trip => {
      const matchRouteName = trip.routeName.toLowerCase().includes(query);
      const matchStartPoint = trip.startPoint.toLowerCase().includes(query);
      const matchEndPoint = trip.endPoint.toLowerCase().includes(query);
      
      const routeString = `${trip.routeName} ${trip.startPoint} ${trip.endPoint}`.toLowerCase();
      const matchAnywhere = routeString.includes(query);
      
      return matchRouteName || matchStartPoint || matchEndPoint || matchAnywhere;
    });

    this.filteredTrips = searchResults;

    if (searchResults.length > 0) {
      const topResults = searchResults.slice(0, Math.min(3, searchResults.length));
      
      this.recommendations = topResults.map(trip => {
        const recommendation = this.getSmartRecommendation(trip, query);
        
        return {
          ...trip,
          recommendation
        };
      });
    } else {
      this.recommendations = [];
    }
  }

  getSmartRecommendation(trip: any, query: string): string {
    if (trip.startPoint.toLowerCase().includes(query)) {
      return `🌟 Perfect starting point from ${trip.startPoint}! Scenic route with comfortable journey.`;
    } else if (trip.endPoint.toLowerCase().includes(query)) {
      return `🎯 Great destination to ${trip.endPoint}! Popular choice among travelers.`;
    } else {
      return `✨ Highly recommended route with excellent service and safety.`;
    }
  }

  getRecommendation(route: any): string {
    const recommendations = [
      '🌟 Highly recommended route with scenic views',
      '⚡ Fastest route available',
      '💰 Most economical option',
      '🚌 Comfortable journey with modern buses',
      '🎯 Popular choice among travelers',
      '✨ Best rated route this month'
    ];
    return recommendations[Math.floor(Math.random() * recommendations.length)];
  }

  bookRoute(trip: any) {
    this.router.navigate(['/book', trip.tripId]);
  }

  goToBookings() {
    this.router.navigate(['/my-bookings']);
  }

  
  profilePhone = '300 1234567';
  profileLanguage = 'English';
  prefOrigin = 'Muzaffarabad';
  prefSeatType = 'window';
  notifEmail = true;
  notifSms = true;
  changingPhone = false;
  phoneChangeOtp = '';

  saveProfile() {
    alert('Profile settings saved successfully!');
  }

  requestPhoneChange() {
    this.changingPhone = true;
    alert('An OTP has been sent to your CURRENT phone number to authorize this change.');
  }

  
  myComplaints: any[] = [
    {
      id: 'CMP-10492',
      category: 'vehicle condition',
      status: 'resolved',
      date: '2026-09-15',
      adminResponse: 'We apologize for the AC issue. The vehicle has been sent to maintenance. A 20% discount coupon has been added to your account.'
    },
    {
      id: 'CMP-10822',
      category: 'driver behaviour',
      status: 'open',
      date: '2026-09-28',
      adminResponse: 'Pending review by local transport manager.'
    }
  ];

  newComplaint = {
    category: '',
    bookingRef: '',
    description: '',
    resolution: '',
    contact: 'sms'
  };
  complaintError = '';

  
  notifications: any[] = [
    {
      id: 1,
      title: 'Booking Confirmed!',
      message: 'Your trip to Rawalpindi on 2026-10-02 is confirmed. Ticket TKT-1823.',
      type: 'success',
      date: 'Just now',
      unread: true,
      channels: ['In-App', 'SMS ??', 'Email ??']
    },
    {
      id: 2,
      title: 'Road Advisory: Jhelum Valley',
      message: 'Expect 30 min delays due to heavy rain on the Jhelum Valley route tomorrow.',
      type: 'warning',
      date: '2 hours ago',
      unread: true,
      channels: ['In-App']
    },
    {
      id: 3,
      title: 'Trip Cancelled (Refunded)',
      message: 'Admin has cancelled your Mirpur trip. Rs. 1200 has been refunded to your UPI.',
      type: 'danger',
      date: 'Yesterday',
      unread: true,
      channels: ['In-App', 'SMS ??']
    },
    {
      id: 4,
      title: 'Seat Hold Expiring',
      message: 'Your hold on seat 14A expires in 2 minutes. Complete payment to secure it.',
      type: 'info',
      date: '3 days ago',
      unread: false,
      channels: ['In-App']
    }
  ];

  get unreadCount() {
    return this.notifications.filter(n => n.unread).length;
  }

  markAllRead() {
    this.notifications.forEach(n => n.unread = false);
  }

  submitComplaint() {
    if (!this.newComplaint.category || !this.newComplaint.description) {
      this.complaintError = 'Please select a category and provide a description.';
      return;
    }
    if (this.newComplaint.description.length < 20) {
      this.complaintError = 'Description must be at least 20 characters long to provide enough detail.';
      return;
    }

    // Mock successful submission
    const c = {
      id: 'CMP-' + Math.floor(Math.random() * 100000),
      category: this.newComplaint.category,
      status: 'open',
      date: new Date().toISOString().split('T')[0],
      adminResponse: 'Pending review.'
    };
    
    this.myComplaints.unshift(c);
    
    // Reset form
    this.newComplaint = {
      category: '',
      bookingRef: '',
      description: '',
      resolution: '',
      contact: 'sms'
    };
    this.complaintError = '';
    alert('Complaint submitted successfully. Our team will review this shortly.');
  }

  verifyPhoneChange() {
    if (this.phoneChangeOtp === '123456' || this.phoneChangeOtp.length === 6) {
      this.changingPhone = false;
      this.phoneChangeOtp = '';
      alert('Phone number updated successfully.');
    } else {
      alert('Invalid OTP.');
    }
  }

  downloadData() {
    alert('Your data (bookings, tickets, preferences) is being prepared. It will download as a JSON/PDF file shortly.');
  }

  deleteAccount() {
    if (confirm('DANGER: Are you sure you want to delete your account? This is irreversible. All future bookings will be cancelled and refunded.')) {
      if (confirm('Please confirm one more time. This action CANNOT be undone.')) {
        alert('Account deleted. Redirecting to home...');
        this.logout();
      }
    }
  }

  logout() {
    localStorage.removeItem('user');
    this.router.navigate(['/']);
  }

  refundPercentage = 100;
  refundAmount = 0;

  startCancellation(booking: any) {
    this.cancellingBooking = booking;
    
    // Mock refund calculation (Booking fee of 50 is non-refundable)
    // Assuming > 60 mins for this mock
    const baseAmount = booking.amount - 50; 
    this.refundPercentage = 100; 
    this.refundAmount = baseAmount;
    
    this.activeTab = 'cancel-booking';
  }

  confirmCancellation() {
    // Mark as cancelled in local array
    const idx = this.myBookings.findIndex(b => b.id === this.cancellingBooking.id);
    if (idx > -1) {
      this.myBookings[idx].status = 'cancelled';
      this.myBookings[idx].canCancel = false;
      this.myBookings[idx].reason = 'Cancelled by Passenger';
      this.myBookings[idx].refundStatus = "Refunded Rs. " + this.refundAmount + " (Processing 3-5 days)";
    }
    
    alert('Booking cancelled successfully. Your seat has been released.');
    this.cancellingBooking = null;
    this.activeBookingsTab = 'cancelled';
    this.activeTab = 'bookings';
  }

  viewTicket(booking: any) {
    this.selectedTrip = {
      routeName: booking.route,
      startPoint: booking.startPoint,
      endPoint: booking.endPoint,
      travelDate: booking.date,
      departureTime: booking.time,
      registration: 'AJK-4402',
      fare: booking.amount,
      status: booking.status
    };
    this.bookingId = booking.id;
    this.ticketCode = 'TKT-' + Math.floor(Math.random() * 10000);
    this.selectedSeats = booking.seats.split(', ').map((s: string) => ({ id: s }));
    this.paymentMethod = 'Easypaisa';
    this.activeTab = 'ticket';
  }

  toggleSidebar() {
    this.sidebarActive = !this.sidebarActive;
  }

  initLiveMap() {
    setTimeout(() => {
      const mapContainer = document.getElementById("ajk-live-map");
      if (!mapContainer) return;

      if (this.liveMap) {
        this.liveMap.remove();
        this.liveMap = null;
      }

      const startCoord = this.ajkGeoCoords[this.selectedTrip?.startPoint] || [34.3597, 73.4711];
      const endCoord = this.ajkGeoCoords[this.selectedTrip?.endPoint] || [33.5651, 73.0169];
      const midpoint: L.LatLngTuple = [
        (startCoord[0] + endCoord[0]) / 2,
        (startCoord[1] + endCoord[1]) / 2
      ];

      this.liveMap = L.map("ajk-live-map", {
        center: midpoint,
        zoom: 9,
        zoomControl: true
      });

      L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
        attribution: "&copy; OpenStreetMap contributors"
      }).addTo(this.liveMap);

      const startIcon = L.divIcon({
        className: "custom-map-pin start-pin",
        html: '<div style="background:#0D766E;color:white;padding:5px 10px;border-radius:12px;font-weight:700;font-size:11px;border:2px solid white;box-shadow:0 3px 8px rgba(0,0,0,0.3);">🟢 Start: ' + (this.selectedTrip?.startPoint || "Origin") + '</div>',
        iconSize: [120, 30],
        iconAnchor: [60, 15]
      });

      const endIcon = L.divIcon({
        className: "custom-map-pin end-pin",
        html: '<div style="background:#DC2626;color:white;padding:5px 10px;border-radius:12px;font-weight:700;font-size:11px;border:2px solid white;box-shadow:0 3px 8px rgba(0,0,0,0.3);">🔴 End: ' + (this.selectedTrip?.endPoint || "Destination") + '</div>',
        iconSize: [120, 30],
        iconAnchor: [60, 15]
      });

      const busDivIcon = L.divIcon({
        className: "custom-bus-marker",
        html: '<div style="background:#0F172A;color:#10B981;padding:6px 12px;border-radius:20px;font-weight:800;font-size:12px;border:2px solid #10B981;box-shadow:0 4px 12px rgba(16,185,129,0.4);display:flex;align-items:center;gap:6px;">🚐 ' + (this.selectedTrip?.registration || "AJK-4402") + ' (' + this.busSpeedKmH + ' km/h)</div>',
        iconSize: [150, 32],
        iconAnchor: [75, 16]
      });

      L.marker(startCoord, { icon: startIcon }).addTo(this.liveMap);
      L.marker(endCoord, { icon: endIcon }).addTo(this.liveMap);

      const kohalaCoord: L.LatLngTuple = [34.0950, 73.4975];
      const routePoints: L.LatLngTuple[] = [startCoord, kohalaCoord, endCoord];

      this.routePolyline = L.polyline(routePoints, {
        color: "#0D766E",
        weight: 5,
        opacity: 0.85,
        dashArray: "8, 8"
      }).addTo(this.liveMap);

      this.liveMap.fitBounds(this.routePolyline.getBounds(), { padding: [50, 50] });

      const busPos: L.LatLngTuple = [
        startCoord[0] * 0.45 + endCoord[0] * 0.55,
        startCoord[1] * 0.45 + endCoord[1] * 0.55
      ];
      this.busMarker = L.marker(busPos, { icon: busDivIcon }).addTo(this.liveMap);

      this.startBusSimulation(startCoord, endCoord);
    }, 250);
  }

  startBusSimulation(start: L.LatLngTuple, end: L.LatLngTuple) {
    if (this.mapSimInterval) clearInterval(this.mapSimInterval);
    let progress = 0.45;
    let forward = true;

    this.mapSimInterval = setInterval(() => {
      if (forward) {
        progress += 0.02;
        if (progress >= 0.88) forward = false;
      } else {
        progress -= 0.02;
        if (progress <= 0.2) forward = true;
      }

      const curLat = start[0] + (end[0] - start[0]) * progress;
      const curLng = start[1] + (end[1] - start[1]) * progress;
      this.busSpeedKmH = Math.floor(40 + Math.random() * 15);

      if (this.busMarker) {
        this.busMarker.setLatLng([curLat, curLng]);
        const updatedIcon = L.divIcon({
          className: "custom-bus-marker",
          html: '<div style="background:#0F172A;color:#10B981;padding:6px 12px;border-radius:20px;font-weight:800;font-size:12px;border:2px solid #10B981;box-shadow:0 4px 12px rgba(16,185,129,0.4);display:flex;align-items:center;gap:6px;">🚐 ' + (this.selectedTrip?.registration || "AJK-4402") + ' (' + this.busSpeedKmH + ' km/h)</div>',
          iconSize: [150, 32],
          iconAnchor: [75, 16]
        });
        this.busMarker.setIcon(updatedIcon);
      }
      this.lastGpsUpdate = "Just now (GPS Synced)";
    }, 2500);
  }

  ngOnDestroy() {
    if (this.mapSimInterval) clearInterval(this.mapSimInterval);
    if (this.liveMap) this.liveMap.remove();
  }

  setActiveTab(tab: string) {
    this.activeTab = tab;
    if (tab === 'track-bus') { this.initLiveMap(); }
    if (window.innerWidth <= 768) {
      this.sidebarActive = false;
    }
  }

  submitFeedback() {
    if (this.feedbackRating === 0) {
      alert('Please select a rating!');
      return;
    }
    const feedback = {
      userId: this.userId,
      message: this.feedbackMessage || `${this.selectedEmoji} Quick feedback`,
      rating: this.feedbackRating
    };
    this.feedbackService.create(feedback).subscribe(() => {
      this.feedbackSubmitted = true;
      setTimeout(() => {
        this.feedbackMessage = '';
        this.feedbackRating = 0;
        this.selectedEmoji = '';
        this.feedbackSubmitted = false;
      }, 3000);
    });
  }

  setRating(rating: number) {
    this.feedbackRating = rating;
  }

  selectEmoji(emoji: string) {
    this.selectedEmoji = emoji;
  }

  getRatingText(): string {
    const texts: any = {
      0: 'Select your rating',
      1: '😞 Poor - We can do better',
      2: '😕 Below Average - Needs improvement',
      3: '😐 Average - It was okay',
      4: '😊 Good - We\'re glad you enjoyed!',
      5: '🤩 Excellent - Thank you so much!'
    };
    return texts[this.feedbackRating] || '';
  }

  expandedGroups = new Set<string>(['Discover', 'Booking flow', 'My bookings', 'Account']);

  toggleGroup(group: string) {
    if (this.expandedGroups.has(group)) {
      this.expandedGroups.delete(group);
    } else {
      this.expandedGroups.add(group);
    }
  }

  isGroupExpanded(group: string) {
    return this.expandedGroups.has(group);
  }
}
























