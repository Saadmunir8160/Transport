import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { RouteService } from '../../services/route';
import { VehicleService } from '../../services/vehicle';
import { TimingService } from '../../services/timing';
import { FareService } from '../../services/fare';
import { AuthGuardService } from '../../services/auth-guard';
import { TransportAdminService } from '../../services/transport-admin.service';
import { AdvisoryService } from '../../services/advisory';
import { TripService } from '../../services/trip.service';
import { BookingService } from '../../services/booking.service';
import { ComplaintService } from '../../services/complaint.service';
import { ReportService } from '../../services/report.service';
import { ETAInsightsService } from '../../services/eta-insights.service';
import { OrganizationService } from '../../services/organization.service';
import { ProfileService } from '../../services/profile.service';
import { ComplianceService } from '../../services/compliance.service';
import { DriverService } from '../../services/driver.service';
import { DashboardService } from '../../services/dashboard.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-dashboard.html',
  styleUrls: ['./admin-dashboard.css']
})
export class AdminDashboardComponent implements OnInit {
  userName = '';
  activeTab = 'dashboard';
  sidebarActive = false;
  sidebarCollapsed = false;
  isMobileDrawerOpen = false;
  currentTheme = 'light';
  
  expandedGroups: { [key: string]: boolean } = {
    'Overview': true,
    'Fleet and people': false,
    'Network': false,
    'Operations': false,
    'Insights': false,
    'Settings': false
  };

  toggleSidebar() {
    this.sidebarCollapsed = !this.sidebarCollapsed;
    this.sidebarActive = !this.sidebarActive;
  }

  toggleGroup(group: string) {
    this.expandedGroups[group] = !this.expandedGroups[group];
  }

  isGroupExpanded(group: string): boolean {
    return this.expandedGroups[group];
  }

  hasGroupAlert(group: string): string | null {
    if (group === 'Fleet and people') return 'orange';
    if (group === 'Network') return 'orange';
    return null;
  }
  
  routes: any[] = [];
  routeName = '';
  startPoint = '';
  endPoint = '';
  distance = 0;
  activeRouteSubTab = 'all'; // For Routes sub-navigation
  routeDirection = 'Outbound';
  routeStatus = 'Active';
  routeSeasonal = 'No';
  routeBaseFare = 500;
  routeDuration = 120;
  
  vehicles: any[] = [];
  activeVehicleSubTab = 'all'; // For Vehicles sub-navigation
  vehicleNumber = '';
  vehicleType = '';
  capacity = 0;
  
  // New Vehicle Form Fields
  vehicleMakeModel = '';
  vehicleFuelType = 'Diesel';
  vehicleSeatLayout = '2x2';
  vehicleDocInsurance = '';
  vehicleDocFitness = '';
  vehicleDocPermit = '';
  vehicleDocPUC = '';

  drivers: any[] = [];
  activeDriverSubTab = 'all'; // For Drivers sub-navigation
  driverName = '';
  driverEmail = '';
  driverPhone = '';
  driverLicense = '';
  
  // New Driver Form Fields
  driverIdNumber = '';
  driverAddress = '';
  driverEmergencyContact = '';
  driverLicenseType = 'LTV';
  driverLicenseIssueDate = '';
  driverLicenseExpiryDate = '';
  driverPassword = '';
  driverJoiningDate = '';
  driverStatus = 'Active';
  
  activeComplianceSubTab = 'expiring';

  timings: any[] = [];
  activeScheduleSubTab = 'all'; // For Schedules sub-navigation
  selectedRouteId = 0;
  selectedVehicleId = 0;
  scheduleDriverId = 0;
  departureTime = '';
  arrivalTime = '';
  scheduleValidFrom = '';
  scheduleValidTo = '';
  scheduleStatus = 'Active';

  // Advisories
  advisories: any[] = [];
  activeAdvisorySubTab = 'active'; // For Advisories sub-navigation
  advRoad = '';
  advReason = 'landslide';
  advStartedAt = '';
  advExpectedReopening = '';
  advAffectedRoutes = '';
  advSource = '';
  
  fares: any[] = [];
  fareRouteId = 0;
  fareAmount = 0;
  passengerType = '';

  trips: any[] = [];
  activeTripSubTab = 'upcoming'; // For Trips sub-navigation
  activeBookingSubTab = 'all'; // For Bookings sub-navigation
  activeComplaintSubTab = 'passengers'; // For Complaints sub-navigation
  tripRouteId = 0;
  tripTimingId = 0;
  tripDriverId = 0;
  tripVehicleId = 0;
  tripDate = '';
  tripDeparture = '';

  selectedTripId: number | null = null;
  selectedTripBookings: any[] = [];
  showBookingsModal = false;

  dashboardStats: any = {};

  constructor(
    private router: Router,
    private routeService: RouteService,
    private vehicleService: VehicleService,
    private timingService: TimingService,
    private fareService: FareService,
    private transportAdminService: TransportAdminService,
    private advisoryService: AdvisoryService,
    private tripService: TripService,
    private bookingService: BookingService,
    private complaintService: ComplaintService,
    private reportService: ReportService,
    private etaInsightsService: ETAInsightsService,
    private orgService: OrganizationService,
    private profileService: ProfileService,
    private complianceService: ComplianceService,
    private driverService: DriverService,
    private dashboardService: DashboardService,
    private authGuard: AuthGuardService
  ) {}

  ngOnInit() {
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    this.userName = user.name || 'Admin';
    this.loadDashboardStats();
    this.loadRoutes();
    this.loadVehicles();
    this.loadDrivers();
    this.loadTimings();
    this.loadFares();
    this.loadTrips();
    this.loadAdvisories();
    this.loadBookings();
    this.loadComplaints();
    this.loadETAInsights();
    this.loadOrganizationProfile();
    this.loadProfileData();
    this.loadNotifications();
    this.loadComplianceDashboard();
    this.loadVehicles();
    this.loadDrivers();
  }

  loadDashboardStats() {
    this.dashboardService.getStats().subscribe({
      next: (data) => this.dashboardStats = data,
      error: (err) => console.error('Failed to load stats', err)
    });
  }

  getPageTitle(): string {
    const titles: any = {
      dashboard: 'Dashboard Overview',
      routes: 'Routes Management',
      vehicles: 'Vehicles Management',
      drivers: 'Drivers Management',
      timings: 'Timings Management',
      trips: 'Trips Management',
      fares: 'Fares Management'
    };
    return titles[this.activeTab] || 'Dashboard';
  }

  loadRoutes() {
    this.transportAdminService.getRoutes().subscribe({
      next: (data) => {
        console.log('Routes loaded for fares:', data);
        this.routes = data;
      },
      error: (err) => {
        console.error('Error loading routes:', err);
        // Remove the alert so it stops annoying the user if backend is down
        this.routes = [];
      }
    });
  }

  addRoute() {
    const payload = { 
      routeName: this.routeName, 
      startPoint: this.startPoint, 
      endPoint: this.endPoint, 
      distance: this.distance,
      direction: this.routeDirection,
      baseFare: this.routeBaseFare,
      baselineDuration: this.routeDuration,
      isSeasonal: this.routeSeasonal === 'Yes',
      status: this.routeStatus
    };
    
    this.routeService.create(payload).subscribe(() => {
        this.loadRoutes();
        this.routeName = '';
        this.startPoint = '';
        this.endPoint = '';
        this.distance = 0;
        this.routeBaseFare = 500;
        this.routeDuration = 120;
        this.activeRouteSubTab = 'all';
      });
  }

  handleRouteAction(event: any, routeId: number) {
    const action = event.target.value;
    if (!action) return;

    if (action === 'deactivate') {
      if (confirm('Are you sure you want to deactivate this route? Active schedules will be affected.')) {
        this.routeService.updateStatus(routeId, 'Inactive').subscribe(() => {
          this.loadRoutes();
        });
      }
    } else if (action === 'duplicate') {
      if (confirm('Duplicate this as a return route?')) {
        this.routeService.duplicateReturn(routeId).subscribe(() => {
          this.loadRoutes();
        });
      }
    } else {
      console.log('Action selected:', action, 'for route', routeId);
    }
    
    event.target.value = '';
  }

  deleteRoute(id: number) {
    if (confirm('Delete this route?')) {
      this.routeService.delete(id).subscribe(() => this.loadRoutes());
    }
  }

  loadVehicles() {
    this.transportAdminService.getVehicles().subscribe(data => this.vehicles = data);
  }

  addVehicle() {
    this.transportAdminService.addVehicle({ vehicleNumber: this.vehicleNumber, vehicleType: this.vehicleType, capacity: this.capacity, status: 'Active' })
      .subscribe(() => {
        this.loadVehicles();
        this.vehicleNumber = '';
        this.vehicleType = '';
        this.capacity = 0;
        this.vehicleMakeModel = '';
        this.vehicleDocInsurance = '';
        this.vehicleDocFitness = '';
        this.vehicleDocPermit = '';
        this.vehicleDocPUC = '';
        this.activeVehicleSubTab = 'all';
      });
  }

  handleVehicleAction(event: any, vehicleId: number) {
    const action = event.target.value;
    if (!action) return;

    if (action === 'deactivate') {
      if (confirm('Are you sure you want to deactivate this vehicle? It cannot have upcoming booked trips.')) {
        console.log('Deactivating vehicle', vehicleId);
      }
    } else {
      console.log('Action selected:', action, 'for vehicle', vehicleId);
      alert(`Action '${action}' selected for vehicle ${vehicleId}`);
    }
    
    event.target.value = '';
  }

  onVehicleTypeChange() {
    const capacityMap: any = {
      'Coaster': 29,
      'Hiace': 15
    };
    this.capacity = capacityMap[this.vehicleType] || 0;
  }

  calculateDistance() {
    if (!this.startPoint || !this.endPoint) {
      this.distance = 0;
      return;
    }

    const distanceMap: any = {
      'Muzaffarabad-Rawalpindi': 138, 'Rawalpindi-Muzaffarabad': 138,
      'Muzaffarabad-Islamabad': 145, 'Islamabad-Muzaffarabad': 145,
      'Muzaffarabad-Mirpur': 95, 'Mirpur-Muzaffarabad': 95,
      'Muzaffarabad-Bagh': 85, 'Bagh-Muzaffarabad': 85,
      'Muzaffarabad-Neelum': 120, 'Neelum-Muzaffarabad': 120,
      'Mirpur-Rawalpindi': 115, 'Rawalpindi-Mirpur': 115,
      'Mirpur-Islamabad': 125, 'Islamabad-Mirpur': 125,
      'Mirpur-Kotli': 35, 'Kotli-Mirpur': 35,
      'Mirpur-Bhimber': 45, 'Bhimber-Mirpur': 45,
      'Bagh-Rawalpindi': 165, 'Rawalpindi-Bagh': 165,
      'Bagh-Islamabad': 175, 'Islamabad-Bagh': 175,
      'Bagh-Rawalakot': 45, 'Rawalakot-Bagh': 45,
      'Bagh-Kotli': 75, 'Kotli-Bagh': 75,
      'Rawalakot-Rawalpindi': 125, 'Rawalpindi-Rawalakot': 125,
      'Rawalakot-Islamabad': 135, 'Islamabad-Rawalakot': 135,
      'Rawalakot-Kotli': 65, 'Kotli-Rawalakot': 65,
      'Rawalakot-Sudhnoti': 30, 'Sudhnoti-Rawalakot': 30,
      'Kotli-Rawalpindi': 95, 'Rawalpindi-Kotli': 95,
      'Kotli-Islamabad': 105, 'Islamabad-Kotli': 105,
      'Bhimber-Rawalpindi': 85, 'Rawalpindi-Bhimber': 85,
      'Bhimber-Islamabad': 95, 'Islamabad-Bhimber': 95,
      'Bhimber-Kotli': 55, 'Kotli-Bhimber': 55,
      'Neelum-Rawalpindi': 258, 'Rawalpindi-Neelum': 258,
      'Neelum-Islamabad': 265, 'Islamabad-Neelum': 265,
      'Hattian-Muzaffarabad': 65, 'Muzaffarabad-Hattian': 65,
      'Hattian-Rawalpindi': 203, 'Rawalpindi-Hattian': 203,
      'Hattian-Islamabad': 210, 'Islamabad-Hattian': 210,
      'Hattian-Bagh': 95, 'Bagh-Hattian': 95,
      'Haveli-Rawalpindi': 145, 'Rawalpindi-Haveli': 145,
      'Haveli-Islamabad': 155, 'Islamabad-Haveli': 155,
      'Haveli-Kotli': 55, 'Kotli-Haveli': 55,
      'Haveli-Bagh': 85, 'Bagh-Haveli': 85,
      'Sudhnoti-Rawalpindi': 155, 'Rawalpindi-Sudhnoti': 155,
      'Sudhnoti-Islamabad': 165, 'Islamabad-Sudhnoti': 165,
      'Sudhnoti-Kotli': 75, 'Kotli-Sudhnoti': 75
    };

    const key = `${this.startPoint}-${this.endPoint}`;
    this.distance = distanceMap[key] || 0;
  }

  loadDrivers() {
    this.transportAdminService.getDrivers().subscribe(data => this.drivers = data);
  }

  addDriver() {
    const driver = {
      name: this.driverName,
      email: this.driverEmail,
      phone: this.driverPhone,
      licenseNumber: this.driverLicense
      // In a real app we'd pass all the new fields to the backend here, and the backend would create the User account with role Driver
    };
    this.transportAdminService.addDriver(driver).subscribe(() => {
      this.loadDrivers();
      this.driverName = '';
      this.driverEmail = '';
      this.driverPhone = '';
      this.driverLicense = '';
      this.driverIdNumber = '';
      this.driverPassword = '';
      this.activeDriverSubTab = 'all';
    });
  }

  handleDriverAction(event: any, driverId: number) {
    const action = event.target.value;
    if (!action) return;

    if (action === 'deactivate') {
      if (confirm('Are you sure you want to deactivate this driver? They cannot have upcoming trips.')) {
        // Call deactivate API
        console.log('Deactivating driver', driverId);
      }
    } else {
      console.log('Action selected:', action, 'for driver', driverId);
      alert(`Action '${action}' selected for driver ${driverId}`);
    }
    
    // Reset dropdown
    event.target.value = '';
  }

  deleteVehicle(id: number) {
    if (confirm('Delete this vehicle?')) {
      // API doesn't support delete yet in the controller, so log it
      console.log('Delete vehicle not implemented in controller', id);
    }
  }

  setActiveTab(tab: string) {
    this.activeTab = tab;
    if (window.innerWidth <= 768) {
      this.sidebarActive = false;
    }
  }

  loadTimings() {
    this.transportAdminService.getSchedules().subscribe(data => this.timings = data);
  }

  addTiming() {
    if (this.selectedRouteId === 0 || this.selectedVehicleId === 0 || !this.departureTime || !this.arrivalTime) {
      alert('Please fill all required fields!');
      return;
    }

    const payload = {
      routeId: Number(this.selectedRouteId),
      vehicleId: Number(this.selectedVehicleId),
      defaultDriverId: this.scheduleDriverId ? Number(this.scheduleDriverId) : null,
      departureTime: this.departureTime + (this.departureTime.split(':').length === 2 ? ':00' : ''),
      arrivalTime: this.arrivalTime + (this.arrivalTime.split(':').length === 2 ? ':00' : ''),
      operatingDays: 'Mon,Tue,Wed,Thu,Fri,Sat,Sun', // Hardcoded default for simplicity, can bind to UI checkboxes later
      validFrom: this.scheduleValidFrom || null,
      validTo: this.scheduleValidTo || null,
      status: this.scheduleStatus
    };
    
    this.timingService.create(payload).subscribe({
      next: (response) => {
        alert('Schedule added successfully!');
        this.loadTimings();
        this.selectedRouteId = 0;
        this.selectedVehicleId = 0;
        this.scheduleDriverId = 0;
        this.departureTime = '';
        this.arrivalTime = '';
        this.scheduleValidFrom = '';
        this.scheduleValidTo = '';
        this.activeScheduleSubTab = 'all';
      },
      error: (err) => {
        const errorMsg = err.error?.message || 'Unknown error';
        alert('Failed to add schedule: ' + errorMsg);
      }
    });
  }

  handleScheduleAction(event: any, timingId: number) {
    const action = event.target.value;
    if (!action) return;

    if (action === 'pause') {
      if (confirm('Pause this schedule? It will stop generating new trips.')) {
        this.timingService.updateStatus(timingId, 'Paused').subscribe(() => this.loadTimings());
      }
    } else if (action === 'activate') {
      if (confirm('Activate this schedule?')) {
        this.timingService.updateStatus(timingId, 'Active').subscribe(() => this.loadTimings());
      }
    } else if (action === 'generate') {
      if (confirm('Generate trips for the next 7 days based on this schedule?')) {
        this.timingService.generateTrips(timingId, 7).subscribe({
          next: (res) => alert(res.message || 'Trips generated successfully!'),
          error: (err) => alert('Error generating trips: ' + (err.error?.message || 'Unknown'))
        });
      }
    } else {
      console.log('Action selected:', action, 'for schedule', timingId);
    }
    
    event.target.value = '';
  }

  deleteTiming(id: number) {
    if (confirm('Delete this timing?')) {
      console.log('Delete not implemented in controller');
    }
  }

  getRouteName(routeId: number): string {
    const route = this.routes.find(r => r.routeId === routeId);
    return route ? route.routeName : 'N/A';
  }

  getVehicleNumber(vehicleId: number): string {
    const vehicle = this.vehicles.find(v => v.vehicleId === vehicleId);
    return vehicle ? vehicle.vehicleNumber : 'N/A';
  }

  loadFares() {
    this.fareService.getAll().subscribe({
      next: (data) => {
        console.log('Fares loaded:', data);
        this.fares = data;
      },
      error: (err) => {
        console.error('Error loading fares:', err);
      }
    });
  }

  addFare() {
    console.log('Adding fare:', {
      routeId: this.fareRouteId,
      amount: this.fareAmount,
      passengerType: this.passengerType
    });

    if (this.fareRouteId === 0 || !this.passengerType || this.fareAmount === 0) {
      alert('Please fill all fields!');
      return;
    }

    const fare = {
      routeId: this.fareRouteId,
      amount: this.fareAmount,
      passengerType: this.passengerType
    };
    
    this.fareService.create(fare).subscribe({
      next: () => {
        console.log('Fare added successfully');
        alert('Fare added successfully!');
        this.loadFares();
        this.fareRouteId = 0;
        this.fareAmount = 0;
        this.passengerType = '';
      },
      error: (err) => {
        console.error('Error adding fare:', err);
        alert('Failed to add fare!');
      }
    });
  }

  deleteFare(id: number) {
    if (confirm('Delete this fare?')) {
      this.fareService.delete(id).subscribe(() => this.loadFares());
    }
  }

  loadAdvisories() {
    this.advisoryService.getAll().subscribe({
      next: (data) => this.advisories = data,
      error: (err) => console.error('Error loading advisories', err)
    });
  }

  addAdvisory() {
    if (!this.advRoad || !this.advReason || !this.advStartedAt) {
      alert('Fill required advisory fields!');
      return;
    }
    const payload = {
      roadOrSegment: this.advRoad,
      reason: this.advReason,
      startedAt: this.advStartedAt,
      expectedReopening: this.advExpectedReopening || null,
      affectedRoutes: this.advAffectedRoutes,
      source: this.advSource
    };
    this.advisoryService.create(payload).subscribe(() => {
      alert('Advisory published!');
      this.loadAdvisories();
      this.advRoad = '';
      this.advReason = 'landslide';
      this.advStartedAt = '';
      this.advExpectedReopening = '';
      this.advAffectedRoutes = '';
      this.advSource = '';
      this.activeAdvisorySubTab = 'active';
    });
  }

  handleAdvisoryAction(event: any, advisoryId: number) {
    const action = event.target.value;
    if (!action) return;
    
    if (action === 'close') {
      if (confirm('Close this advisory? Operations on this route will resume normal status.')) {
        this.advisoryService.close(advisoryId).subscribe(() => this.loadAdvisories());
      }
    } else if (action === 'notify') {
      alert('Passengers booked on affected trips have been notified via WhatsApp/Email.');
    } else if (action === 'flag') {
      alert('Affected trips flagged. The ETA model has been fed the advisory data.');
    }
    event.target.value = '';
  }

  loadTrips() {
    this.tripService.getAll().subscribe(data => this.trips = data);
  }

  handleTripAction(event: any, tripId: number) {
    const action = event.target.value;
    if (!action) return;
    
    if (action === 'cancel') {
      const reason = prompt('Please provide a reason for cancelling this trip:');
      if (reason) {
        this.tripService.cancel(tripId, reason).subscribe({
          next: () => {
            alert('Trip cancelled and passengers notified.');
            this.loadTrips();
          },
          error: (err) => alert('Failed to cancel trip: ' + err.error?.message)
        });
      }
    } else if (action === 'reassign') {
      const driverId = prompt('Enter new Driver ID (or leave blank to keep current):');
      const vehicleId = prompt('Enter new Vehicle ID (or leave blank to keep current):');
      
      const payload: any = {};
      if (driverId) payload.driverId = Number(driverId);
      if (vehicleId) payload.vehicleId = Number(vehicleId);
      
      if (Object.keys(payload).length > 0) {
        this.tripService.reassign(tripId, payload).subscribe({
          next: () => {
            alert('Reassigned successfully!');
            this.loadTrips();
          },
          error: (err) => alert('Reassignment failed: ' + err.error?.message)
        });
      }
    }
    
    event.target.value = '';
  }

  addTrip() {
    if (!this.tripRouteId || !this.tripDate || !this.tripDeparture) {
      alert('Fill all required fields!');
      return;
    }
    const trip = {
      routeId: Number(this.tripRouteId),
      timingId: this.tripTimingId ? Number(this.tripTimingId) : null,
      driverId: this.tripDriverId ? Number(this.tripDriverId) : null,
      vehicleId: this.tripVehicleId ? Number(this.tripVehicleId) : null,
      travelDate: this.tripDate,
      departureTime: this.tripDeparture + (this.tripDeparture.split(':').length === 2 ? ':00' : ''),
      status: 'Scheduled'
    };
    this.tripService.create(trip).subscribe({
      next: () => {
        alert('Trip added successfully!');
        this.loadTrips();
        this.tripRouteId = 0;
        this.tripDriverId = 0;
        this.tripVehicleId = 0;
        this.tripDate = '';
        this.tripDeparture = '';
        this.activeTripSubTab = 'upcoming';
      },
      error: (err) => alert('Failed to create trip: ' + err.error?.message)
    });
  }


  bookings: any[] = [];
  
  loadBookings() {
    this.bookingService.getAll().subscribe({
      next: (data) => this.bookings = data,
      error: (err) => console.error('Failed to load bookings:', err)
    });
  }

  handleBookingAction(event: any, bookingId: number) {
    const action = event.target.value;
    if (!action) return;

    if (action === 'cancel') {
      const reason = prompt('Cancel this booking? Seats will be restored. Enter reason:');
      if (reason) {
        // Mock Admin ID = 1 for audit logging (in a real app, pull from auth service)
        this.bookingService.cancel(bookingId, reason, 1).subscribe({
          next: () => {
             alert('Booking cancelled and logged.');
             this.loadBookings();
          },
          error: (err) => alert('Failed to cancel: ' + err.error?.message)
        });
      }
    } else if (action === 'move') {
        const tripId = prompt('Enter new Trip ID (leave blank to keep current):');
        const seatNum = prompt('Enter new Seat Number (leave blank to keep current):');
        const reason = prompt('Enter reason for moving:');
        
        const payload: any = { adminUserId: 1 };
        if (reason) payload.adminReason = reason;
        if (tripId) payload.newTripId = Number(tripId);
        if (seatNum) payload.newSeatNumber = seatNum;
        
        if (tripId || seatNum) {
            this.bookingService.move(bookingId, payload).subscribe({
                next: () => {
                    alert('Booking moved and logged.');
                    this.loadBookings();
                },
                error: (err) => alert('Failed to move: ' + err.error?.message)
            });
        }
    } else if (action === 'export') {
        this.bookingService.export(bookingId).subscribe((res: any) => {
             alert('Export generated: ' + res.url);
        });
    } else {
      console.log('Action selected:', action, 'for booking', bookingId);
    }
    
    event.target.value = '';
  }

  passengerComplaints: any[] = [];
  driverIssues: any[] = [];

  loadComplaints() {
    this.complaintService.getPassengerComplaints().subscribe({
        next: (data) => this.passengerComplaints = data,
        error: (err) => console.error(err)
    });
    this.complaintService.getDriverIssues().subscribe({
        next: (data) => {
            this.driverIssues = data;
            // Check for immediate notification for critical issues
            const critical = data.find((d: any) => d.category?.toLowerCase().includes('accident') || d.category?.toLowerCase().includes('breakdown'));
            if (critical && critical.status === 'open') {
                alert(`CRITICAL ALERT: Driver issue reported for ${critical.category}! Immediate action required.`);
            }
        },
        error: (err) => console.error(err)
    });
  }

  handleComplaintAction(event: any, id: number, type: string) {
    const action = event.target.value;
    if (!action) return;

    if (action === 'resolve') {
      const note = prompt('Enter resolution note (Required for resolving):');
      if (note) {
        this.complaintService.resolve(id, { status: 'Resolved', resolutionNote: note }).subscribe({
            next: () => {
                alert('Issue resolved.');
                this.loadComplaints();
            },
            error: (err) => alert('Failed to resolve: ' + err.error?.message)
        });
      } else {
        alert('Resolution note is required!');
      }
    } else if (action === 'in_progress') {
        this.complaintService.resolve(id, { status: 'In progress', resolutionNote: '' }).subscribe({
            next: () => this.loadComplaints(),
            error: (err) => alert('Failed: ' + err.error?.message)
        });
    } else if (action === 'view_attachments') {
        this.complaintService.getAttachments(id).subscribe((res: any) => {
            window.open(res.url, '_blank');
        });
    } else if (action === 'link_trip') {
        alert('Linked to trip action.');
    } else {
      console.log('Action selected:', action, 'for', type, 'issue', id);
    }
    
    event.target.value = '';
  }

  viewTripBookings(tripId: number) {
    this.selectedTripId = tripId;
    this.transportAdminService.getTripBookings(tripId).subscribe({
      next: (data) => {
        this.selectedTripBookings = data;
        this.showBookingsModal = true;
      },
      error: (err) => {
        console.error('Error fetching trip bookings:', err);
        alert('Failed to load bookings.');
      }
    });
  }

  closeBookingsModal() {
    this.showBookingsModal = false;
    this.selectedTripId = null;
    this.selectedTripBookings = [];
  }
  
  reportType = 'trips';
  reportStartDate = '';
  reportEndDate = '';
  reportRouteId: number | null = null;
  reportDriverId: number | null = null;
  reportVehicleId: number | null = null;
  reportData: any[] = [];

  generateReport() {
    const filter = {
      startDate: this.reportStartDate || null,
      endDate: this.reportEndDate || null,
      routeId: this.reportRouteId ? Number(this.reportRouteId) : null,
      driverId: this.reportDriverId ? Number(this.reportDriverId) : null,
      vehicleId: this.reportVehicleId ? Number(this.reportVehicleId) : null,
    };

    let reportObservable;
    switch(this.reportType) {
        case 'trips': reportObservable = this.reportService.getTrips(filter); break;
        case 'bookings': reportObservable = this.reportService.getBookings(filter); break;
        case 'cancellations': reportObservable = this.reportService.getCancellations(filter); break;
        case 'popular-routes': reportObservable = this.reportService.getPopularRoutes(filter); break;
        case 'vehicle-usage': reportObservable = this.reportService.getVehicleUsage(filter); break;
        case 'driver-activity': reportObservable = this.reportService.getDriverActivity(filter); break;
        default: return;
    }

    reportObservable.subscribe({
        next: (data) => this.reportData = data,
        error: (err) => alert('Failed to load report: ' + err.message)
    });
  }

  exportReport(format: string) {
    const filter = {
      startDate: this.reportStartDate || null,
      endDate: this.reportEndDate || null,
      routeId: this.reportRouteId ? Number(this.reportRouteId) : null,
      driverId: this.reportDriverId ? Number(this.reportDriverId) : null,
      vehicleId: this.reportVehicleId ? Number(this.reportVehicleId) : null,
    };

    if (format === 'pdf') {
        this.reportService.exportPdf(filter).subscribe(res => window.open(res.url, '_blank'));
    } else if (format === 'excel') {
        this.reportService.exportExcel(filter).subscribe(res => window.open(res.url, '_blank'));
    } else if (format === 'print') {
        window.print();
    }
  }
  
  // ETA Insights
  etaPredVsActual: any = null;
  etaAvgDelay: any = null;
  etaDelayFactors: any = null;
  etaModelAccuracy: any = null;

  loadETAInsights(routeId?: number) {
    this.etaInsightsService.getPredictedVsActual(routeId).subscribe({
        next: (data) => this.etaPredVsActual = data,
        error: (err) => console.error('ETA PredVsActual err:', err)
    });

    this.etaInsightsService.getAverageDelay().subscribe({
        next: (data) => this.etaAvgDelay = data,
        error: (err) => console.error('ETA AvgDelay err:', err)
    });

    this.etaInsightsService.getDelayFactors().subscribe({
        next: (data) => this.etaDelayFactors = data,
        error: (err) => console.error('ETA DelayFactors err:', err)
    });

    this.etaInsightsService.getModelAccuracy().subscribe({
        next: (data) => this.etaModelAccuracy = data,
        error: (err) => console.error('ETA ModelAccuracy err:', err)
    });
  }
  
  // Organization
  orgProfile: any = null;

  loadOrganizationProfile() {
    this.orgService.getProfile().subscribe({
        next: (data) => this.orgProfile = data,
        error: (err) => {
            console.error('Failed to load org profile:', err);
            this.orgProfile = {
                organizationId: '101',
                organizationName: 'AJK Transport HQ',
                location: 'Muzaffarabad, AJK',
                subdomain: 'demo',
                status: 'Active',
                contactPhone: '+92 5822 000000',
                contactEmail: 'hq@ajktransport.com',
                address: 'Main Secretariat Road',
                logoUrl: '',
                officeHours: '9 AM - 6 PM',
                defaultLanguage: 'English'
            };
        }
    });
  }

  updateOrganizationProfile() {
    if (!this.orgProfile) return;
    
    const dto = {
        contactPhone: this.orgProfile.contactPhone,
        contactEmail: this.orgProfile.contactEmail,
        address: this.orgProfile.address,
        logoUrl: this.orgProfile.logoUrl,
        officeHours: this.orgProfile.officeHours,
        defaultLanguage: this.orgProfile.defaultLanguage
    };

    this.orgService.updateProfile(dto).subscribe({
        next: () => alert('Organization profile updated!'),
        error: (err) => alert('Failed to update: ' + err.message)
    });
  }

  updateSuperAdminFields() {
    const isSuperAdmin = confirm('Are you a Super Admin? (Mock check)');
    if (!isSuperAdmin) return;
    
    const subdomain = prompt('Enter new Subdomain:', this.orgProfile.subdomain || '');
    const status = prompt('Enter new Status (Active/Inactive):', this.orgProfile.status || '');
    
    if (subdomain || status) {
        this.orgService.updateSuperAdminFields({ subdomain, status }).subscribe({
            next: (data) => {
                alert('Super Admin fields updated!');
                this.orgProfile = data;
            },
            error: (err) => alert('Failed: ' + (err.error?.message || err.message))
        });
    }
  }
  
  // Profile & Notifications
  myProfile: any = null;
  notifications: any[] = [];
  notificationTypeFilter: string = '';

  loadProfileData() {
    this.profileService.getProfile().subscribe({
        next: (data) => this.myProfile = data,
        error: (err) => console.error('Failed to load profile:', err)
    });
  }

  updateMyProfile() {
    if (!this.myProfile) return;
    this.profileService.updateProfile(this.myProfile).subscribe({
        next: () => alert('Profile updated!'),
        error: (err) => alert('Failed to update profile: ' + err.message)
    });
  }

  changeMyPassword() {
    const oldPassword = prompt('Enter Old Password:');
    const newPassword = prompt('Enter New Password:');
    if (oldPassword && newPassword) {
        this.profileService.changePassword({ oldPassword, newPassword }).subscribe({
            next: () => alert('Password changed successfully!'),
            error: (err) => alert('Failed to change password: ' + (err.error?.message || err.message))
        });
    }
  }

  logout() {
    localStorage.removeItem('user');
    localStorage.removeItem('token');
    // Mock routing to login
    alert('Logged out securely. Redirecting to login...');
    this.router.navigate(['/']);
  }

  loadNotifications() {
    this.profileService.getNotifications(this.notificationTypeFilter).subscribe({
        next: (data) => this.notifications = data,
        error: (err) => console.error('Failed to load notifications:', err)
    });
  }

  markNotificationAsRead(id: number) {
    this.profileService.markNotificationAsRead(id).subscribe({
        next: () => this.loadNotifications(),
        error: (err) => console.error('Failed to mark read:', err)
    });
  }

  chooseChannels() {
    const channel = prompt('Choose preferred channel (Email / SMS / Push):', 'Email');
    if (channel) {
        this.profileService.chooseChannels({ channel }).subscribe(() => {
            alert(`Channel preferences updated to: ${channel}`);
        });
    }
  }

  // Compliance
  complianceDashboard: any = null;

  loadComplianceDashboard() {
    this.complianceService.getDashboard().subscribe({
      next: (data) => this.complianceDashboard = data,
      error: (err) => console.error('Failed to load compliance:', err)
    });
  }

  uploadDocument() {
    const entityType = prompt('Entity Type (Vehicle / Driver):', 'Vehicle');
    if (!entityType) return;
    const entityId = prompt(`Enter ${entityType} ID:`);
    if (!entityId) return;
    const docType = prompt('Document Type (e.g. Registration, Insurance, Licence):');
    const expiry = prompt('Expiry Date (YYYY-MM-DD):');
    const reminders = prompt('Reminder Days (comma separated):', '30,15,7');
    
    if (docType && expiry) {
      this.complianceService.uploadDocument({
        entityType: entityType,
        entityId: Number(entityId),
        documentType: docType,
        expiryDate: expiry,
        reminderDays: reminders,
        fileUrl: 'https://example.com/doc.pdf'
      }).subscribe({
        next: () => {
          alert('Document uploaded/replaced successfully!');
          this.loadComplianceDashboard();
        },
        error: (err) => alert('Failed: ' + err.message)
      });
    }
  }

  exportCompliance() {
    this.complianceService.exportComplianceReport().subscribe(res => window.open(res.url, '_blank'));
  }

}
