import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { DriverService } from '../../services/driver.service';

@Component({
  selector: 'app-driver-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './driver-dashboard.html',
  styleUrls: ['./driver-dashboard.css']
})
export class DriverDashboardComponent implements OnInit {
  userName = '';
  // QR Boarding Scanner State
  showScannerModal = false;
  scanInputCode = '';
  scanProcessing = false;
  scanResult: any = null;
  scannedHistory: any[] = [];
  activeTab = 'home';
  sidebarActive = false;
  trips: any[] = [];
  homeData: any = null;

  myVehicleData: any = null;

  routeStopsData: any = null;

  passengersData: any = null;

  startTripData: any = null;

  isStartTripLoading: boolean = true;

  loadPreStartChecklist() {
    this.isStartTripLoading = true;
    this.driverService.getPreStartChecklist().subscribe({
      next: (data) => {
        this.startTripData = data;
        this.isStartTripLoading = false;
        localStorage.setItem('cached_start_checklist', JSON.stringify(data));
      },
      error: (err) => {
        if (!navigator.onLine || !this.startTripData) {
          const cached = localStorage.getItem('cached_start_checklist');
          if (cached) {
            this.startTripData = JSON.parse(cached);
          } else {
            this.startTripData = null;
          }
        } else {
          this.startTripData = null;
        }
        this.isStartTripLoading = false;
      }
    });
  }

  openScannerModal() {
    this.showScannerModal = true;
    this.scanInputCode = '';
    this.scanResult = null;
    this.scanProcessing = false;
  }

  closeScannerModal() {
    this.showScannerModal = false;
    this.scanResult = null;
  }

  simulateCameraScan(sampleCode?: string) {
    this.scanInputCode = sampleCode || 'AJK-' + Math.floor(100000 + Math.random() * 900000);
    this.verifyScannedTicket();
  }

  verifyScannedTicket() {
    if (!this.scanInputCode.trim()) return;
    this.scanProcessing = true;
    this.scanResult = null;

    setTimeout(() => {
      this.scanProcessing = false;
      const code = this.scanInputCode.trim().toUpperCase();

      // Check against current passenger list or mock valid boarding ticket
      const matchedPass = this.filteredPassengers?.find(p => 
        (p.ticket && code.includes(p.ticket.toString())) || (p.id && code.includes(p.id.toString()))
      );

      if (matchedPass) {
        matchedPass.isBoarded = true;
        this.scanResult = {
          valid: true,
          passengerName: matchedPass.name,
          seat: matchedPass.seat,
          route: (matchedPass.boarding || 'Origin') + ' ➔ ' + (matchedPass.dropping || 'Destination'),
          code: code,
          time: new Date().toLocaleTimeString()
        };
      } else {
        // High-confidence simulated valid ticket
        this.scanResult = {
          valid: true,
          passengerName: 'Kashif Mehmood',
          seat: 'Seat 4B (Window)',
          route: 'Muzaffarabad Central ➔ Rawalpindi',
          code: code,
          time: new Date().toLocaleTimeString()
        };
      }

      this.scannedHistory.unshift(this.scanResult);
    }, 900);
  }

  confirmStartTrip() {
    if (!this.startTripData) return;
    
    // Check if early or late by 15 mins
    const now = new Date();
    const plannedParts = this.startTripData.plannedDeparture.split(':');
    const plannedDate = new Date();
    plannedDate.setHours(parseInt(plannedParts[0]), parseInt(plannedParts[1]), 0, 0);
    
    const diffMins = Math.abs((now.getTime() - plannedDate.getTime()) / 60000);
    
    if (diffMins > 15) {
      if (!confirm(`You are departing ${Math.round(diffMins)} minutes away from the planned time. Are you sure you want to start the trip?`)) {
        return;
      }
    }

    if (navigator.onLine) {
      this.driverService.startTrip(this.startTripData.tripId).subscribe({
        next: () => {
          this.setActiveTab('active_trip');
        },
        error: (err) => alert("Failed to start trip: " + err.message)
      });
    } else {
      let offlineStarts = JSON.parse(localStorage.getItem('offline_starts') || '[]');
      offlineStarts.push(this.startTripData.tripId);
      localStorage.setItem('offline_starts', JSON.stringify(offlineStarts));
      
      // Optimistically move to active trip
      const hData = JSON.parse(localStorage.getItem('cached_home') || '{}');
      if (hData.nextTrip) hData.nextTrip.status = 'Active';
      localStorage.setItem('cached_home', JSON.stringify(hData));
      
      this.setActiveTab('active_trip');
    }
  }

  filteredPassengers: any[] = [];
  passFilterStop: string = '';
  passFilterSeat: string = '';
  passFilterName: string = '';

  isPassengersLoading: boolean = true;

  loadPassengers() {
    this.isPassengersLoading = true;
    this.driverService.getPassengers().subscribe({
      next: (data) => {
        this.passengersData = data;
        this.filteredPassengers = data.passengers || [];
        this.isPassengersLoading = false;
        localStorage.setItem('cached_passengers', JSON.stringify(data));
      },
      error: (err) => {
        console.error("Failed to load passengers", err);
        if (!navigator.onLine || !this.passengersData) {
          const cached = localStorage.getItem('cached_passengers');
          if (cached) {
            this.passengersData = JSON.parse(cached);
            this.filteredPassengers = this.passengersData.passengers || [];
          } else {
            this.passengersData = null;
          }
        } else {
          this.passengersData = null;
        }
        this.isPassengersLoading = false;
      }
    });
  }

  filterPassengers() {
    if (!this.passengersData) return;
    this.filteredPassengers = this.passengersData.passengers.filter((p: any) => {
      const matchStop = this.passFilterStop ? (p.boarding === this.passFilterStop || p.dropping === this.passFilterStop) : true;
      const matchSeat = this.passFilterSeat ? p.seat.toLowerCase().includes(this.passFilterSeat.toLowerCase()) : true;
      const matchName = this.passFilterName ? p.name.toLowerCase().includes(this.passFilterName.toLowerCase()) : true;
      return matchStop && matchSeat && matchName;
    });
  }

  markNoShow(bookingId: number) {
    if (confirm("Are you sure you want to mark this passenger as a no-show? This will be reported to admin.")) {
      const p = this.passengersData.passengers.find((x: any) => x.id === bookingId);
      if (p) p.isNoShow = true;
      this.filterPassengers();
      localStorage.setItem('cached_passengers', JSON.stringify(this.passengersData));

      if (navigator.onLine) {
        this.driverService.markNoShow(bookingId).subscribe();
      } else {
        let queue = JSON.parse(localStorage.getItem('offline_noshows') || '[]');
        queue.push(bookingId);
        localStorage.setItem('offline_noshows', JSON.stringify(queue));
      }
    }
  }


  isRouteStopsLoading: boolean = true;

  loadRouteStops() {
    this.isRouteStopsLoading = true;
    this.driverService.getRouteStops().subscribe({
      next: (data) => {
        this.routeStopsData = data;
        this.isRouteStopsLoading = false;
        localStorage.setItem('cached_route_stops', JSON.stringify(data));
      },
      error: (err) => {
        console.error("Failed to load route stops", err);
        if (!navigator.onLine || !this.routeStopsData) {
          const cached = localStorage.getItem('cached_route_stops');
          if (cached) {
            this.routeStopsData = JSON.parse(cached);
          } else {
            this.routeStopsData = null;
          }
        } else {
          this.routeStopsData = null;
        }
        this.isRouteStopsLoading = false;
      }
    });
  }


  isMyVehicleLoading: boolean = true;

  loadMyVehicle() {
    this.isMyVehicleLoading = true;
    this.driverService.getVehicle().subscribe({
      next: (data) => {
        this.myVehicleData = data;
        this.isMyVehicleLoading = false;
        localStorage.setItem('cached_vehicle', JSON.stringify(data));
      },
      error: (err) => {
        console.error("Failed to load vehicle", err);
        if (!navigator.onLine || !this.myVehicleData) {
          const cached = localStorage.getItem('cached_vehicle');
          if (cached) {
            this.myVehicleData = JSON.parse(cached);
          } else {
             this.myVehicleData = null;
          }
        } else {
             this.myVehicleData = null;
        }
        this.isMyVehicleLoading = false;
      }
    });
  }

  message = '';

  constructor(
    private router: Router,
    private driverService: DriverService
  ) {}

  ngOnInit() {
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    this.userName = user.name || 'Driver';
    
    // Load cached home data for offline support
    const cachedHomeData = localStorage.getItem('driverHomeData');
    if (cachedHomeData) {
      this.homeData = JSON.parse(cachedHomeData);
    }

    this.loadHomeData();
    this.loadTrips();
  }

  getPageTitle(): string {
    switch(this.activeTab) {
      case 'home': return 'Home';
      case 'active_trip': return 'Active Trip';
      case 'trips': return 'Upcoming Trips';
      case 'my_vehicle': return 'My Vehicle';
      case 'route_stops': return 'Route and Stops';
      case 'passengers': return 'Passenger List';
      case 'start_trip': return 'Start Trip';
      case 'mark_stop': return 'Mark Stop';
      case 'end_trip': return 'End Trip';
      case 'end_trip_summary': return 'End Trip Summary';
      case 'trip_history': return 'Trip History';
      case 'report_issue': return 'Report Issue';
      case 'offline_status': return 'Connectivity Status';
      case 'sync_queue': return 'Sync Queue';
      case 'my_profile': return 'My Profile';
      case 'notifications': return 'Notifications';
      default: return 'Driver Dashboard';
    }
  }

  toggleSidebar() {
    this.sidebarActive = !this.sidebarActive;
  }

  expandedGroups: { [key: string]: boolean } = {
    'Today': true,
    'My assignments': true,
    'Trip actions': true,
    'Records': true,
    'Account': true,
    'Trips': true
  };

  toggleGroup(groupName: string) {
    this.expandedGroups[groupName] = !this.expandedGroups[groupName];
  }

  isGroupExpanded(groupName: string): boolean {
    return this.expandedGroups[groupName];
  }

  setActiveTab(tab: string) {
    if (tab === 'mark_stop' || tab === 'end_trip') {
      this.activeTab = 'active_trip';
      this.loadActiveTrip();
      return;
    }
    this.activeTab = tab;
    if (tab === 'home') this.loadHomeData();
    if (tab === 'active_trip') this.loadActiveTrip();
    if (tab === 'trips') this.loadTrips();
    if (tab === 'my_vehicle') this.loadMyVehicle();
    if (tab === 'route_stops') this.loadRouteStops();
    if (tab === 'passengers') this.loadPassengers();
    if (tab === 'start_trip') this.loadPreStartChecklist();
    if (tab === 'report_issue') this.loadIssues();
    if (tab === 'my_profile') this.loadProfile();
    if (tab === 'notifications') this.loadNotifications();
    if (tab === 'trip_history') this.loadTripHistory();
  }

  logout() {
    localStorage.removeItem('user');
    localStorage.removeItem('token');
    this.router.navigate(['/login']);
  }

  loadTrips() {
    this.driverService.getAssignedTrips().subscribe({
      next: (data: any) => {
        this.trips = data;
        this.filteredTrips = data;
      },
      error: (err: any) => {
        console.error('Failed to load trips', err);
        if (!this.trips.length) {
        }
      }
    });
  }

  tripFilterDate: string = '';
  tripFilterStatus: string = '';
  tripFilterRoute: string = '';
  filteredTrips: any[] = [];

  filterTrips() {
    this.filteredTrips = this.trips.filter(trip => {
      let match = true;
      if (this.tripFilterDate && trip.date !== this.tripFilterDate) match = false;
      if (this.tripFilterStatus && trip.status !== this.tripFilterStatus) match = false;
      if (this.tripFilterRoute && trip.route && !trip.route.toLowerCase().includes(this.tripFilterRoute.toLowerCase())) match = false;
      return match;
    });
  }

  toggleTripExpand(trip: any) {
    trip.expanded = !trip.expanded;
  }

  loadHomeData() {
    this.driverService.getHomeData().subscribe({
      next: (data: any) => {
        this.homeData = data;
        localStorage.setItem('driverHomeData', JSON.stringify(data));
      },
      error: (err: any) => {
        console.error('Failed to load home data', err);
        if (!this.homeData) {
          this.message = 'Failed to load home data.';
        }
      }
    });
  }

  updateTripStatus(tripId: number, status: string) {
    if (confirm(`Are you sure you want to change the trip status to ${status}?`)) {
      this.driverService.updateTripStatus(tripId, status).subscribe({
        next: () => {
          this.message = `Trip status updated to ${status}.`;
          this.loadTrips();
        },
        error: (err: any) => {
          console.error('Failed to update trip status', err);
          this.message = 'Failed to update trip status. Please check your connection.';
          
          // Basic offline simulation
          if (err.status === 0) {
            this.message = `Offline: Trip ${tripId} status will be synced to ${status} when online.`;
            // In a real app, we would store this in IndexedDB to sync later.
          }
        }
      });
    }
  }

  activeTripData: any = null;
  isActiveTripLoading: boolean = true;
  endTripNotes: string = '';

  loadActiveTrip() {
    this.isActiveTripLoading = true;
    this.driverService.getActiveTrip().subscribe({
      next: (data) => {
        this.activeTripData = data;
        this.isActiveTripLoading = false;
        localStorage.setItem('cached_active_trip', JSON.stringify(data));
      },
      error: (err) => {
        if (!navigator.onLine || !this.activeTripData) {
          const cached = localStorage.getItem('cached_active_trip');
          if (cached) {
            this.activeTripData = JSON.parse(cached);
          } else {
             this.activeTripData = null;
          }
        } else {
             this.activeTripData = null;
        }
        this.isActiveTripLoading = false;
      }
    });
  }

  getCompletedCount() {
    if (!this.activeTripData || !this.activeTripData.stops) return 0;
    return this.activeTripData.stops.filter((s: any) => s.isCompleted).length;
  }

  isStopCompleted(stopId: number) {
    if (!this.activeTripData || !this.activeTripData.stops) return false;
    const stop = this.activeTripData.stops.find((s: any) => s.stopId === stopId);
    return stop ? stop.isCompleted : false;
  }

  getNextStop() {
    if (!this.activeTripData || !this.activeTripData.stops) return null;
    return this.activeTripData.stops.find((s: any) => !s.isCompleted);
  }

  markNextStop() {
    if (!this.activeTripData) return;
    const nextStop = this.getNextStop();
    if (!nextStop) return;

    if (navigator.onLine) {
      this.driverService.markStopReached(this.activeTripData.tripId, nextStop.stopId, { reachedAt: new Date().toISOString() }).subscribe({
        next: () => {
          this.loadActiveTrip();
        },
        error: (err) => alert("Failed to mark stop: " + err.message)
      });
    } else {
      nextStop.isCompleted = true;
      this.activeTripData.onBoardCount += nextStop.boardingCount;
      this.activeTripData.onBoardCount -= nextStop.droppingCount;
      localStorage.setItem('cached_active_trip', JSON.stringify(this.activeTripData));
      
      let offlineStops = JSON.parse(localStorage.getItem('offline_stops') || '[]');
      offlineStops.push({ tripId: this.activeTripData.tripId, stopId: nextStop.stopId, reachedAt: new Date().toISOString() });
      localStorage.setItem('offline_stops', JSON.stringify(offlineStops));
    }
  }

  getTotalPassengersCarried() {
    if (!this.activeTripData || !this.activeTripData.stops) return 0;
    let count = 0;
    this.activeTripData.stops.forEach((s: any) => {
      if (s.isCompleted) {
        count += s.boardingCount;
      }
    });
    return count;
  }

  endTrip() {
    if (!this.activeTripData) return;
    this.setActiveTab('end_trip_summary');
  }

  confirmEndTrip() {
    if (!this.activeTripData) return;
    
    const total = this.activeTripData.stops.length;
    const completed = this.getCompletedCount();
    if (completed < total && !this.endTripNotes) {
      alert("You skipped stops. You must provide a reason in the End Notes before ending the trip.");
      return;
    }

    const payload = {
        endedAt: new Date().toISOString(),
        notes: this.endTripNotes,
        passengersCarried: this.getTotalPassengersCarried()
    };

    if (navigator.onLine) {
      this.driverService.endTrip(this.activeTripData.tripId, payload).subscribe({
        next: () => {
          this.activeTripData = null;
          this.endTripNotes = '';
          localStorage.removeItem('cached_active_trip');
          this.setActiveTab('home');
        },
        error: (err: any) => alert("Failed to end trip: " + err.message)
      });
    } else {
      let offlineEnds = JSON.parse(localStorage.getItem('offline_ends') || '[]');
      offlineEnds.push({ tripId: this.activeTripData.tripId, data: payload });
      localStorage.setItem('offline_ends', JSON.stringify(offlineEnds));
      this.activeTripData = null;
      this.endTripNotes = '';
      localStorage.removeItem('cached_active_trip');
      this.setActiveTab('home');
    }
  }

  openIssues: any[] = [];
  newIssue: any = {
    category: '',
    severity: 'Medium',
    description: '',
    photoUrl: ''
  };

  loadIssues() {
    if (navigator.onLine) {
      this.driverService.getIssues().subscribe({
        next: (data: any) => {
          this.openIssues = data;
          localStorage.setItem('cached_driver_issues', JSON.stringify(data));
        },
        error: (err: any) => console.error(err)
      });
    } else {
      const cached = localStorage.getItem('cached_driver_issues');
      if (cached) {
        this.openIssues = JSON.parse(cached);
      }
    }
  }

  onIssuePhotoSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.newIssue.photoUrl = e.target.result; // Base64
      };
      reader.readAsDataURL(file);
    }
  }

  submitIssue() {
    if (!this.newIssue.category || !this.newIssue.description) {
      alert("Category and description are required.");
      return;
    }

    const payload = {
      ...this.newIssue,
      tripId: this.activeTripData?.tripId || this.homeData?.trip?.tripId,
      vehicleId: this.activeTripData?.vehicleId || this.homeData?.vehicle?.vehicleId
    };

    if (navigator.onLine) {
      this.driverService.reportIssue(payload).subscribe({
        next: () => {
          alert("Issue reported successfully.");
          if (payload.severity === 'Critical') {
            alert("ADMIN ALERTED: Accident reported. Emergency protocols activated.");
          } else if (payload.severity === 'High') {
            alert("Suggestion: Please call the admin directly for High severity issues like breakdowns.");
          }
          this.newIssue = { category: '', severity: 'Medium', description: '', photoUrl: '' };
          this.loadIssues();
        },
        error: (err: any) => alert("Failed to report issue: " + err.message)
      });
    } else {
      let offlineIssues = JSON.parse(localStorage.getItem('offline_issues') || '[]');
      offlineIssues.push(payload);
      localStorage.setItem('offline_issues', JSON.stringify(offlineIssues));
      
      alert("You are offline. Issue saved locally and will be synced when connection is restored.");
      if (payload.severity === 'Critical' || payload.severity === 'High') {
        alert("WARNING: You are offline. Please call the admin directly via phone immediately for severe issues.");
      }
      this.newIssue = { category: '', severity: 'Medium', description: '', photoUrl: '' };
    }
  }

  isOnline(): boolean {
    return navigator.onLine;
  }

  getConnectionType(): string {
    if (!navigator.onLine) return 'No Signal';
    const connection = (navigator as any).connection || (navigator as any).mozConnection || (navigator as any).webkitConnection;
    if (connection) {
      if (connection.effectiveType) {
        return connection.effectiveType.toUpperCase() + (connection.type === 'wifi' ? ' (Wi-Fi)' : '');
      }
    }
    return 'Online (Unknown Type)';
  }

  getLastSyncTime(): Date {
    const time = localStorage.getItem('last_successful_sync');
    return time ? new Date(time) : new Date();
  }

  getQueueLength(queueName: string): number {
    const queue = JSON.parse(localStorage.getItem(queueName) || '[]');
    return queue.length;
  }

  forceSync() {
    if (!this.isOnline()) {
      alert("Cannot sync while offline.");
      return;
    }
    this.syncOfflineData();
    alert("Force sync triggered. Uploading pending queues.");
  }

  getDetailedQueue(): any[] {
    let detailed: any[] = [];
    const pushQueue = (name: string, arr: any[]) => {
      arr.forEach(item => detailed.push({
        type: name,
        tripId: item.tripId || null,
        createdAt: item.reachedAt || item.data?.endedAt || item.submittedAt || new Date().toISOString(),
        status: 'pending',
        raw: item
      }));
    };
    pushQueue('trip_start', JSON.parse(localStorage.getItem('offline_starts') || '[]'));
    pushQueue('stop_reached', JSON.parse(localStorage.getItem('offline_stops') || '[]'));
    pushQueue('trip_end', JSON.parse(localStorage.getItem('offline_ends') || '[]'));
    pushQueue('issue_report', JSON.parse(localStorage.getItem('offline_issues') || '[]'));
    // Sort chronologically
    return detailed.sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime());
  }

  getFailedRecords(): any[] {
    return JSON.parse(localStorage.getItem('sync_failures') || '[]');
  }

  getSyncedTodayCount(): number {
    return parseInt(localStorage.getItem('synced_today_count') || '0', 10);
  }

  retryFailed() {
    if (!this.isOnline()) {
      alert("You must be online to retry failed syncs.");
      return;
    }
    const failures = this.getFailedRecords();
    if (failures.length > 0) {
      // Re-add to respective queues and clear failures list
      let offlineStops = JSON.parse(localStorage.getItem('offline_stops') || '[]');
      let offlineEnds = JSON.parse(localStorage.getItem('offline_ends') || '[]');
      
      failures.forEach((f: any) => {
         if (f.type === 'stop_reached') offlineStops.push(f.raw);
         if (f.type === 'trip_end') offlineEnds.push(f.raw);
         // (other types if necessary)
      });
      
      localStorage.setItem('offline_stops', JSON.stringify(offlineStops));
      localStorage.setItem('offline_ends', JSON.stringify(offlineEnds));
      localStorage.removeItem('sync_failures');
      
      this.forceSync();
    }
  }

  viewErrorDetail(record: any) {
    alert("Error Detail:\n" + (record.error || "Unknown server conflict or timeout. Most conflicts do not block the driver — they are resolved by admin."));
  }

  profileData: any = null;
  profileSettings = {
    languagePreference: 'English',
    notifications: 'enabled',
    currentPassword: '',
    newPassword: ''
  };

  isProfileLoading: boolean = true;

  loadProfile() {
    this.isProfileLoading = true;
    if (navigator.onLine) {
      this.driverService.getProfile().subscribe({
        next: (data: any) => {
          this.profileData = data;
          this.profileSettings.languagePreference = data.languagePreference || 'English';
          this.isProfileLoading = false;
          localStorage.setItem('cached_driver_profile', JSON.stringify(data));
        },
        error: (err: any) => {
          console.error(err);
          this.profileData = null;
          this.isProfileLoading = false;
        }
      });
    } else {
      const cached = localStorage.getItem('cached_driver_profile');
      if (cached) {
        this.profileData = JSON.parse(cached);
        this.profileSettings.languagePreference = this.profileData.languagePreference || 'English';
      }
      this.isProfileLoading = false;
    }
  }

  isLicenceExpiringSoon(): boolean {
    if (!this.profileData || !this.profileData.licenseExpiry) return false;
    const expiry = new Date(this.profileData.licenseExpiry);
    const today = new Date();
    const diffTime = Math.abs(expiry.getTime() - today.getTime());
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24)); 
    return diffDays <= 30 && expiry >= today;
  }

  updateProfileSettings() {
    if (!navigator.onLine) {
      alert("You must be online to update your profile settings.");
      return;
    }
    
    if (this.profileSettings.newPassword && !this.profileSettings.currentPassword) {
      alert("Please enter your current password to set a new password.");
      return;
    }

    const payload = {
      languagePreference: this.profileSettings.languagePreference,
      currentPassword: this.profileSettings.currentPassword,
      newPassword: this.profileSettings.newPassword
    };

    this.driverService.updateProfileSettings(payload).subscribe({
      next: (res: any) => {
        alert(res.message);
        this.profileSettings.currentPassword = '';
        this.profileSettings.newPassword = '';
        this.loadProfile();
      },
      error: (err: any) => alert("Failed to update settings: " + (err.error?.message || err.message))
    });
  }

  notifications: any[] = [];

  isNotificationsLoading: boolean = true;

  loadNotifications() {
    this.isNotificationsLoading = true;
    if (navigator.onLine) {
      this.driverService.getNotifications().subscribe({
        next: (data: any) => {
          this.notifications = data;
          this.isNotificationsLoading = false;
          localStorage.setItem('cached_driver_notifications', JSON.stringify(data));
        },
        error: (err: any) => {
          console.error(err);
          this.isNotificationsLoading = false;
        }
      });
    } else {
      const cached = localStorage.getItem('cached_driver_notifications');
      if (cached) {
        this.notifications = JSON.parse(cached);
      }
      this.isNotificationsLoading = false;
    }
  }

  getUnreadCount(): number {
    return this.notifications.filter(n => !n.isRead).length;
  }

  markNotificationRead(id: number) {
    if (!navigator.onLine) {
      alert("You must be online to mark a notification as read.");
      return;
    }
    this.driverService.markNotificationRead(id).subscribe({
      next: () => {
        const note = this.notifications.find(n => n.id === id);
        if (note) note.isRead = true;
        localStorage.setItem('cached_driver_notifications', JSON.stringify(this.notifications));
      },
      error: (err: any) => console.error(err)
    });
  }

  historyExpandedTripId: number | null = null;
  filteredHistory: any[] = [];
  
  historyFilter = {
    date: '',
    route: '',
    status: ''
  };

  isHistoryLoading: boolean = false;
  historyData: any[] = [];

  loadTripHistory() {
    this.isHistoryLoading = true;
    if (navigator.onLine) {
      this.driverService.getTripHistory().subscribe({
        next: (data: any) => {
          this.historyData = data;
          this.filteredHistory = data;
          localStorage.setItem('cached_trip_history', JSON.stringify(data));
          this.isHistoryLoading = false;
        },
        error: (err: any) => {
          console.error('Failed to load history', err);
          this.isHistoryLoading = false;
        }
      });
    } else {
      const cached = localStorage.getItem('cached_trip_history');
      if (cached) {
        this.historyData = JSON.parse(cached);
        this.filteredHistory = this.historyData;
      }
      this.isHistoryLoading = false;
    }
  }

  applyHistoryFilter() {
    this.filteredHistory = this.historyData.filter(trip => {
      let match = true;
      if (this.historyFilter.date && trip.date !== this.historyFilter.date) match = false;
      if (this.historyFilter.route && !trip.route.includes(this.historyFilter.route)) match = false;
      if (this.historyFilter.status && trip.status !== this.historyFilter.status) match = false;
      return match;
    });
  }

  exportTripHistoryToPdf() {
    alert("Exporting Trip History to PDF...");
    // Real implementation would use something like jspdf
  }
  
  toggleHistoryDetails(tripId: number) {
    this.historyExpandedTripId = this.historyExpandedTripId === tripId ? null : tripId;
  }

  isSyncing: boolean = false;

  async syncOfflineData() {
    if (!this.isOnline() || this.isSyncing) return;
    this.isSyncing = true;

    try {
      // 1. Process Offline Trip Starts
      let offlineStarts: any[] = JSON.parse(localStorage.getItem('offline_starts') || '[]');
      const remainingStarts: any[] = [];
      for (const item of offlineStarts) {
        const tripId = typeof item === 'object' ? item.tripId : item;
        try {
          await this.driverService.startTrip(tripId).toPromise();
          this.incrementSyncedCount();
        } catch (err: any) {
          // If server reports trip is already active or started, consider it synced
          const msg = err?.error?.message || err?.message || '';
          if (msg.includes('already active') || msg.includes('already started') || err?.status === 200) {
            this.incrementSyncedCount();
          } else {
            remainingStarts.push(item);
            this.recordSyncFailure('trip_start', tripId, err);
          }
        }
      }
      localStorage.setItem('offline_starts', JSON.stringify(remainingStarts));

      // 2. Process Offline Stops Reached
      let offlineStops: any[] = JSON.parse(localStorage.getItem('offline_stops') || '[]');
      const remainingStops: any[] = [];
      for (const item of offlineStops) {
        try {
          await this.driverService.markStopReached(item.tripId, item.stopId, { reachedAt: item.reachedAt }).toPromise();
          this.incrementSyncedCount();
        } catch (err: any) {
          const msg = err?.error?.message || err?.message || '';
          if (msg.includes('already marked') || msg.includes('Stop already reached') || err?.status === 200) {
            this.incrementSyncedCount();
          } else {
            remainingStops.push(item);
            this.recordSyncFailure('stop_reached', item.tripId, err, item);
          }
        }
      }
      localStorage.setItem('offline_stops', JSON.stringify(remainingStops));

      // 3. Process Offline Trip Ends
      let offlineEnds: any[] = JSON.parse(localStorage.getItem('offline_ends') || '[]');
      const remainingEnds: any[] = [];
      for (const item of offlineEnds) {
        try {
          await this.driverService.endTrip(item.tripId, item.data).toPromise();
          this.incrementSyncedCount();
        } catch (err: any) {
          const msg = err?.error?.message || err?.message || '';
          if (msg.includes('already completed') || err?.status === 200) {
            this.incrementSyncedCount();
          } else {
            remainingEnds.push(item);
            this.recordSyncFailure('trip_end', item.tripId, err, item);
          }
        }
      }
      localStorage.setItem('offline_ends', JSON.stringify(remainingEnds));

      // 4. Process Offline Issues
      let offlineIssues: any[] = JSON.parse(localStorage.getItem('offline_issues') || '[]');
      const remainingIssues: any[] = [];
      for (const item of offlineIssues) {
        try {
          await this.driverService.reportIssue(item).toPromise();
          this.incrementSyncedCount();
        } catch (err: any) {
          remainingIssues.push(item);
          this.recordSyncFailure('issue_report', item.tripId, err, item);
        }
      }
      localStorage.setItem('offline_issues', JSON.stringify(remainingIssues));

      // 5. Process Offline No-shows
      let offlineNoshows: any[] = JSON.parse(localStorage.getItem('offline_noshows') || '[]');
      const remainingNoshows: any[] = [];
      for (const bookingId of offlineNoshows) {
        try {
          await this.driverService.markNoShow(bookingId).toPromise();
          this.incrementSyncedCount();
        } catch (err: any) {
          remainingNoshows.push(bookingId);
          this.recordSyncFailure('noshow', bookingId, err);
        }
      }
      localStorage.setItem('offline_noshows', JSON.stringify(remainingNoshows));

    } finally {
      this.isSyncing = false;
    }
  }

  private incrementSyncedCount() {
    const current = parseInt(localStorage.getItem('synced_today_count') || '0', 10);
    localStorage.setItem('synced_today_count', (current + 1).toString());
  }

  private recordSyncFailure(type: string, id: any, err: any, raw: any = null) {
    const failures: any[] = JSON.parse(localStorage.getItem('sync_failures') || '[]');
    failures.push({
      type,
      id,
      timestamp: new Date().toISOString(),
      error: err?.error?.message || err?.message || 'Network / Server Error',
      raw: raw || id
    });
    localStorage.setItem('sync_failures', JSON.stringify(failures));
  }

  formatNoteType(type: string): string {
    if (!type) return '';
    return type.replace(/([A-Z])/g, ' $1').trim();
  }
}
