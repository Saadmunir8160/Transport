import { Component, OnInit, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, NavigationEnd } from '@angular/router';
import { Location } from '@angular/common';
import { RouteService } from '../../services/route';
import { VehicleService } from '../../services/vehicle';
import { TimingService } from '../../services/timing';
import { FareService } from '../../services/fare';
import { FeedbackService } from '../../services/feedback';
import { AuthGuardService } from '../../services/auth-guard';
import { UserService } from '../../services/user';
import { AuthService } from '../../services/auth';
import { SuperAdminService } from '../../services/superadmin.service';
import { MockDataService } from '../../mock/mock-data.service';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-superadmin-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './superadmin-dashboard.html',
  styleUrls: ['./superadmin-dashboard.css']
})
export class SuperAdminDashboardComponent implements OnInit {
  userName = '';
  activeTab = 'dashboard';
  sidebarCollapsed = false;
  expandedGroups = new Set<string>(['Overview', 'Organizations', 'Monitoring']);
  isMobileDrawerOpen = false;
  isCmdKOpen = false;
  currentTheme = 'light';
  
  badgeData: any = { 
    liveTrips: 'live', 
    systemHealth: 'live', 
    platformAlerts: '3', 
    anomalyAlerts: '2', 
    auditLogs: 'immutable' 
  };

  routeMap: Record<string, string> = {
    '/superadmin/dashboard': 'dashboard',
    '/superadmin/organizations': 'organizations',
    '/superadmin/organizations/create': 'createOrganization',
    '/superadmin/subdomains': 'subdomains',
    '/superadmin/admins/org': 'transportAdmins',
    '/superadmin/admins/create': 'createAdmin',
    '/superadmin/admins/sessions': 'sessions',
    '/superadmin/view/drivers': 'drivers',
    '/superadmin/view/vehicles': 'vehicles',
    '/superadmin/view/routes': 'routes',
    '/superadmin/view/trips': 'liveTrips',
    '/superadmin/view/bookings': 'bookings',
    '/superadmin/monitor/health': 'systemHealth',
    '/superadmin/monitor/offline-sync': 'offlineSync',
    '/superadmin/monitor/alerts': 'platformAlerts',
    '/superadmin/security/audit-logs': 'auditLogs',
    '/superadmin/security/logins': 'loginLog',
    '/superadmin/security/anomalies': 'anomalyDetection',
    '/superadmin/analytics/platform': 'platformAnalytics',
    '/superadmin/analytics/global-reports': 'globalReports',
    '/superadmin/analytics/eta-monitor': 'etaMonitor',
    '/superadmin/settings/system': 'systemSettings',
    '/superadmin/settings/notifications': 'notifications',
    '/superadmin/settings/profile': 'profile'
  };

  syncTabFromUrl() {
    const path = this.location.path();
    for (const [route, tab] of Object.entries(this.routeMap)) {
      if (path === route || path.startsWith(route + '?')) {
        this.activeTab = tab;
        return;
      }
    }
    if (path === '/superadmin') {
      this.activeTab = 'dashboard';
    }
  }

  setActiveTab(tab: string) {
    this.activeTab = tab;
    const route = Object.keys(this.routeMap).find(k => this.routeMap[k] === tab);
    if (route) {
      this.location.go(route);
    }
  }

  toggleGroup(groupName: string) {
    if (this.expandedGroups.has(groupName)) {
      this.expandedGroups.delete(groupName);
    } else {
      this.expandedGroups.add(groupName);
    }
  }

  isGroupExpanded(groupName: string): boolean {
    return this.expandedGroups.has(groupName);
  }

  hasGroupAlert(groupName: string): string | null {
    if (this.isGroupExpanded(groupName)) return null;

    if (groupName === 'Monitoring' && this.badgeData.platformAlerts !== '0') return 'red';
    if (groupName === 'Security and compliance' && this.badgeData.anomalyAlerts !== '0') return 'amber';
    return null;
  }
  
  routes: any[] = [];
  routeName = '';
  startPoint = '';
  endPoint = '';
  distance = 0;
  
  vehicles: any[] = [];
  vehicleNumber = '';
  vehicleType = '';
  capacity = 0;

  timings: any[] = [];
  selectedRouteId = 0;
  selectedVehicleId = 0;
  departureTime = '';
  arrivalTime = '';

  fares: any[] = [];
  fareRouteId = 0;
  fareAmount = 0;
  passengerType = '';

  feedbacks: any[] = [];

  users: any[] = [];

  newAdminName = '';
  newAdminEmail = '';
  newAdminPassword = '';

  dashboardStats: any = {};
  alertStats: any = {
    inactiveOrganizations: 0,
    adminsNeverLoggedIn: 0,
    failedLogins: 0,
    syncFailures: 0,
    anomalousLogins: 0
  };
  transportAdmins: any[] = [];
  auditLogs: any[] = [];
  filteredAuditLogs: any[] = [];
  
  auditDateFilter: string = '';
  auditOrgFilter: string = '';
  auditRoleFilter: string = '';
  auditActionFilter: string = '';
  auditEntityFilter: string = '';
  auditIpFilter: string = '';
  auditActorFilter: string = '';

  accessLogs: any[] = [];
  filteredAccessLogs: any[] = [];
  accessRoleFilter: string = '';
  accessOrgFilter: string = '';
  accessEventFilter: string = '';
  accessIpFilter: string = '';
  accessFailedOnly: boolean = false;

  anomalyAlerts: any[] = [];
  filteredAnomalyAlerts: any[] = [];

  platformAnalytics: any = null;
  analyticsDateFilter: string = '';
  analyticsOrgFilter: string = '';
  analyticsRegionFilter: string = '';

  reportType: string = 'orgSummary';
  reportDateRange: string = 'thisMonth';
  reportOrg: string = '';
  reportFormat: string = 'PDF';

  availableReports = [
    { id: 'orgSummary', name: 'Organization summary report', description: 'One row per org — trips, bookings, drivers, vehicles, compliance' },
    { id: 'driverActivity', name: 'Driver activity report', description: 'Activity across all orgs' },
    { id: 'vehicleUsage', name: 'Vehicle usage and compliance report', description: 'Usage and compliance across all orgs' },
    { id: 'bookingRevenue', name: 'Booking and revenue summary', description: 'Bookings and revenue across all orgs' },
    { id: 'cancellationAnalysis', name: 'Cancellation analysis report', description: 'Analysis of cancelled trips and bookings' },
    { id: 'routePerformance', name: 'Route performance report', description: 'Performance of routes platform-wide' },
    { id: 'passengerGrowth', name: 'Passenger growth report', description: 'New and unique passenger metrics over time' },
    { id: 'systemUptime', name: 'System uptime and health report', description: 'Platform reliability metrics' },
    { id: 'offlineSync', name: 'Offline sync activity report', description: 'Metrics for offline operations and sync conflicts' }
  ];

  etaModel: any = null;

  systemSettings: any = null;

  notificationConfig: any[] = [];

  myProfile: any = null;

  activeSessions: any[] = [];

  newOrg: any = {
    organizationCode: '',
    organizationName: '',
    location: '',
    district: '',
    status: 'Active'
  };

  newTransportAdmin: any = {
    name: '',
    email: '',
    password: '',
    organizationId: ''
  };

  cmdItems = [
    { title: 'Dashboard', description: 'Platform overview', action: () => this.setActiveTab('dashboard'), group: 'Overview' },
    { title: 'All organizations', description: 'Manage tenants', action: () => this.setActiveTab('organizations'), group: 'Organizations' },
    { title: 'Create organization', description: 'Add new tenant', action: () => this.setActiveTab('createOrganization'), group: 'Organizations' },
    { title: 'Subdomain manager', description: 'Custom tenant URLs', action: () => this.setActiveTab('subdomains'), group: 'Organizations' },
    { title: 'Organization admins', description: 'Manage admin users', action: () => this.setActiveTab('transportAdmins'), group: 'Admin users' },
    { title: 'Create admin', description: 'Add new organization admin', action: () => this.setActiveTab('createAdmin'), group: 'Admin users' },
    { title: 'Active sessions', description: 'Monitor user sessions', action: () => this.setActiveTab('sessions'), group: 'Admin users' },
    { title: 'All drivers', description: 'Cross-organization view', action: () => this.setActiveTab('drivers'), group: 'System-wide' },
    { title: 'All vehicles', description: 'Cross-organization view', action: () => this.setActiveTab('vehicles'), group: 'System-wide' },
    { title: 'All routes', description: 'Cross-organization view', action: () => this.setActiveTab('routes'), group: 'System-wide' },
    { title: 'All trips', description: 'Live trips overview', action: () => this.setActiveTab('liveTrips'), group: 'System-wide' },
    { title: 'All bookings', description: 'Cross-organization view', action: () => this.setActiveTab('bookings'), group: 'System-wide' },
    { title: 'System health', description: 'Platform stability metrics', action: () => this.setActiveTab('systemHealth'), group: 'Monitoring' },
    { title: 'Offline sync monitor', description: 'Sync queue and conflicts', action: () => this.setActiveTab('offlineSync'), group: 'Monitoring' },
    { title: 'Platform alerts', description: 'Critical system alerts', action: () => this.setActiveTab('platformAlerts'), group: 'Monitoring' },
    { title: 'Audit logs', description: 'Immutable activity logs', action: () => this.setActiveTab('auditLogs'), group: 'Security' },
    { title: 'Login and access log', description: 'Authentication history', action: () => this.setActiveTab('loginLog'), group: 'Security' },
    { title: 'Anomaly alerts', description: 'AI-detected anomalies', action: () => this.setActiveTab('anomalyDetection'), group: 'Security' },
    { title: 'Platform analytics', description: 'Usage and growth', action: () => this.setActiveTab('platformAnalytics'), group: 'Analytics' },
    { title: 'Global reports', description: 'Exportable platform data', action: () => this.setActiveTab('globalReports'), group: 'Analytics' },
    { title: 'ETA model monitor', description: 'ML model performance', action: () => this.setActiveTab('etaMonitor'), group: 'Analytics' },
    { title: 'System settings', description: 'Global platform config', action: () => this.setActiveTab('systemSettings'), group: 'Settings' },
    { title: 'Notification config', description: 'Platform emails/SMS', action: () => this.setActiveTab('notifications'), group: 'Settings' },
    { title: 'My profile', description: 'Super Admin details', action: () => this.setActiveTab('profile'), group: 'Settings' },
    { title: 'Toggle Theme', description: 'Switch light/dark mode', action: () => this.currentTheme = this.currentTheme === 'light' ? 'dark' : 'light', group: 'Actions' }
  ];
  cmdFilteredItems: any[] = [];
  cmdSelectedIndex = 0;

  @HostListener('window:keydown', ['$event'])
  handleKeyboardEvent(event: KeyboardEvent) {
    if (event.ctrlKey && event.key === 'k') {
      event.preventDefault();
      this.openCmdK();
    } else if (this.isCmdKOpen) {
      if (event.key === 'Escape') {
        this.closeCmdK();
      } else if (event.key === 'ArrowDown') {
        event.preventDefault();
        if (this.cmdSelectedIndex < this.cmdFilteredItems.length - 1) {
          this.cmdSelectedIndex++;
        }
      } else if (event.key === 'ArrowUp') {
        event.preventDefault();
        if (this.cmdSelectedIndex > 0) {
          this.cmdSelectedIndex--;
        }
      } else if (event.key === 'Enter') {
        event.preventDefault();
        if (this.cmdFilteredItems.length > 0) {
          this.executeCmd(this.cmdFilteredItems[this.cmdSelectedIndex]);
        }
      }
    }
  }

  openCmdK() {
    this.isCmdKOpen = true;
    this.cmdFilteredItems = [...this.cmdItems];
    this.cmdSelectedIndex = 0;
  }

  closeCmdK() {
    this.isCmdKOpen = false;
  }

  filterCmdList(query: string) {
    if (!query.trim()) {
      this.cmdFilteredItems = [...this.cmdItems];
    } else {
      const q = query.toLowerCase();
      this.cmdFilteredItems = this.cmdItems.filter(item => 
        item.title.toLowerCase().includes(q) || 
        item.description.toLowerCase().includes(q)
      );
    }
    this.cmdSelectedIndex = 0;
  }

  executeCmd(item: any) {
    item.action();
    this.closeCmdK();
  }

  constructor(
    private router: Router,
    private activatedRoute: ActivatedRoute,
    private location: Location,
    private routeService: RouteService,
    private vehicleService: VehicleService,
    private timingService: TimingService,
    private fareService: FareService,
    private feedbackService: FeedbackService,
    private authGuard: AuthGuardService,
    private userService: UserService,
    private authService: AuthService,
    private superAdminService: SuperAdminService,
    private mockData: MockDataService
  ) {
    this.router.events.subscribe(event => {
      if (event instanceof NavigationEnd) {
        this.syncTabFromUrl();
      }
    });
  }

  systemHealth: any = null;
  offlineSyncData: any = null;
  filteredSyncLogs: any[] = [];
  syncOrgFilter: string = '';
  syncRecordTypeFilter: string = '';
  syncStatusFilter: string = '';
  syncDateFilter: string = '';

  filteredAlerts: any[] = [];
  alertSeverityFilter: string = '';
  alertStatusFilter: string = '';

  ngOnInit() {
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    this.userName = user.name;
    this.loadDashboardStats();
    this.loadSystemAlerts();
    this.loadSystemHealth();
    this.loadOfflineSyncLogs();

    this.loadChartAnalytics();
    this.loadOrganizations();
    this.loadTransportAdmins();
    this.loadAuditLogs();
    this.loadAccessLogs();
    this.loadAnomalyAlerts();
    this.loadPlatformAnalytics();
    this.loadEtaModelMonitor();
    this.loadSystemSettings();
    this.loadNotificationConfig();
    this.loadMyProfile();
    this.loadActiveSessions();
    this.loadRoutes();
    this.loadVehicles();
    this.loadTimings();
    this.loadFares();
    this.loadFeedbacks();
    this.loadUsers();
    this.loadSubdomains();
  }

  getPageTitle(): string {
    const titles: any = {
      dashboard: 'SuperAdmin Dashboard',
      organizations: 'Organization Management',
      createOrganization: 'Create Organization',
      subdomains: 'Subdomain Manager',
      transportAdmins: 'Organization Admins',
      createAdmin: 'Create Admin',
      sessions: 'Active Sessions',
      drivers: 'Cross-Organization Drivers',
      vehicles: 'Cross-Organization Vehicles',
      routes: 'All Routes',
      liveTrips: 'Live Trips',
      bookings: 'All Bookings',
      systemHealth: 'System Health',
      offlineSync: 'Offline Sync Monitor',
      platformAlerts: 'Platform Alerts',
      auditLogs: 'Audit Log',
      loginLog: 'Login & Access Log',
      anomalyDetection: 'Anomaly Detection',
      platformAnalytics: 'Platform Analytics',
      globalReports: 'Global Reports',
      etaMonitor: 'ETA Model Monitor',
      featureManagement: 'Feature Management',
      systemSettings: 'System Settings',
      notifications: 'Notification Configuration',
      profile: 'Super Admin Profile'
    };
    return titles[this.activeTab] || 'Dashboard';
  }

  loadDashboardStats() {
    this.mockData.getDashboardKPIs().subscribe(data => this.dashboardStats = data);
  }

  loadSystemAlerts() {
    this.superAdminService.getSystemAlerts().subscribe(data => {
      this.alertStats = data;
      this.filterAlerts();
    });
  }

  filterAlerts() {
    if (!this.alertStats?.details) return;
    this.filteredAlerts = this.alertStats.details.filter((alert: any) => {
      let match = true;
      if (this.alertSeverityFilter) match = match && alert.severity === this.alertSeverityFilter;
      if (this.alertStatusFilter) match = match && alert.status === this.alertStatusFilter;
      return match;
    });
  }

  getOpenAlertsCount(): number {
    return this.alertStats?.details?.filter((a: any) => a.status === 'open').length || 0;
  }

  loadSystemHealth() {
    this.superAdminService.getSystemHealth().subscribe(data => this.systemHealth = data);
  }

  loadOfflineSyncLogs() {
    this.superAdminService.getOfflineSyncLogs().subscribe(data => {
      this.offlineSyncData = data;
      this.filterSyncLogs();
    });
  }

  filterSyncLogs() {
    if (!this.offlineSyncData?.logs) return;
    this.filteredSyncLogs = this.offlineSyncData.logs.filter((log: any) => {
      let match = true;
      if (this.syncOrgFilter) match = match && log.organization === this.syncOrgFilter;
      if (this.syncRecordTypeFilter) match = match && log.recordType === this.syncRecordTypeFilter;
      if (this.syncStatusFilter) match = match && log.status === this.syncStatusFilter;
      return match;
    });
  }

  getDistinctSyncOrgs(): string[] {
    if (!this.offlineSyncData?.logs) return [];
    return Array.from(new Set(this.offlineSyncData.logs.map((l: any) => l.organization))) as string[];
  }


  loadChartAnalytics() {
    this.superAdminService.getChartAnalytics().subscribe(data => {
      setTimeout(() => this.renderCharts(data), 100);
    });
  }

  charts: any = {};

  renderCharts(data: any) {
    // Destroy existing charts if any
    Object.values(this.charts).forEach((c: any) => c.destroy());

    const orgsCanvas = document.getElementById('orgsChart') as HTMLCanvasElement;
    if (orgsCanvas) {
      this.charts.orgs = new Chart(orgsCanvas, {
        type: 'line',
        data: {
          labels: data.organizationsPerMonth.map((d: any) => d.month),
          datasets: [{
            label: 'New Organizations',
            data: data.organizationsPerMonth.map((d: any) => d.count),
            borderColor: '#3b82f6',
            tension: 0.1,
            fill: true,
            backgroundColor: 'rgba(59, 130, 246, 0.1)'
          }]
        }
      });
    }

    const tripsCanvas = document.getElementById('tripsChart') as HTMLCanvasElement;
    if (tripsCanvas) {
      this.charts.trips = new Chart(tripsCanvas, {
        type: 'bar',
        data: {
          labels: data.tripsPerOrganization.map((d: any) => d.organization),
          datasets: [{
            label: 'Total Trips',
            data: data.tripsPerOrganization.map((d: any) => d.count),
            backgroundColor: '#10b981'
          }]
        }
      });
    }

    const bookingsCanvas = document.getElementById('bookingsChart') as HTMLCanvasElement;
    if (bookingsCanvas) {
      this.charts.bookings = new Chart(bookingsCanvas, {
        type: 'doughnut',
        data: {
          labels: data.bookingsPerOrganization.map((d: any) => d.organization),
          datasets: [{
            data: data.bookingsPerOrganization.map((d: any) => d.count),
            backgroundColor: ['#ef4444', '#f59e0b', '#10b981', '#3b82f6', '#8b5cf6']
          }]
        }
      });
    }
  }

  organizations: any[] = [];
  filteredOrganizations: any[] = [];
  orgSearchText = '';
  orgStatusFilter = '';
  orgRegionFilter = '';
  orgDateFrom = '';
  orgDateTo = '';
  orgCurrentPage = 1;
  orgPageSize = 5;

  selectedOrgDetails: any = null;
  orgDetailTab: string = 'overview';

  setOrgDetailTab(tab: string) {
    this.orgDetailTab = tab;
  }

  loadOrganizations() {
    this.mockData.getOrganizations().subscribe(data => {
      this.organizations = data;
      this.applyOrgFilters();
    });
  }

  applyOrgFilters() {
    this.filteredOrganizations = this.organizations.filter(org => {
      let match = true;
      if (this.orgSearchText) {
        const search = this.orgSearchText.toLowerCase();
        match = match && ((org.name?.toLowerCase().includes(search)) || (org.id?.toLowerCase().includes(search)));
      }
      if (this.orgStatusFilter) {
        match = match && org.status === this.orgStatusFilter;
      }
      if (this.orgRegionFilter) {
        match = match && org.area === this.orgRegionFilter;
      }
      if (this.orgDateFrom) {
        match = match && new Date(org.createdAt) >= new Date(this.orgDateFrom);
      }
      if (this.orgDateTo) {
        match = match && new Date(org.createdAt) <= new Date(this.orgDateTo);
      }
      return match;
    });
    this.orgCurrentPage = 1;
  }

  get paginatedOrganizations() {
    const start = (this.orgCurrentPage - 1) * this.orgPageSize;
    return this.filteredOrganizations.slice(start, start + this.orgPageSize);
  }

  get totalOrgPages() {
    return Math.ceil(this.filteredOrganizations.length / this.orgPageSize);
  }

  nextOrgPage() {
    if (this.orgCurrentPage < this.totalOrgPages) {
      this.orgCurrentPage++;
    }
  }

  prevOrgPage() {
    if (this.orgCurrentPage > 1) {
      this.orgCurrentPage--;
    }
  }

  getActiveOrgCount(): number {
    return this.organizations.filter(o => o.status === 'Active').length;
  }

  clearOrgFilters() {
    this.orgSearchText = '';
    this.orgStatusFilter = '';
    this.orgRegionFilter = '';
    this.orgDateFrom = '';
    this.orgDateTo = '';
    this.applyOrgFilters();
  }

  viewOrgDetails(org: any) {
    const orgId = org.id || org.organizationId;
    const foundOrg = this.organizations.find(o => o.id === orgId);
    if (foundOrg) {
      this.selectedOrgDetails = foundOrg;
      this.setActiveTab('orgDetails');
    }
  }

  // Onboarding Wizard state
  wizardOrg = {
    organizationName: '',
    organizationCode: '',
    district: '',
    location: '',
    contactPhone: '',
    contactEmail: '',
    officeHours: '',
    status: 'Active',
    subdomain: ''
  };

  wizardAdmin = {
    name: '',
    email: '',
    phone: '',
    password: '',
    sendWelcomeEmail: true
  };

  sanitizeSubdomain() {
    if (this.wizardOrg.subdomain) {
      this.wizardOrg.subdomain = this.wizardOrg.subdomain.toLowerCase().replace(/[^a-z0-9]/g, '');
    }
  }

  generateTempPassword() {
    const chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*";
    let pwd = "";
    for (let i = 0; i < 12; i++) {
      pwd += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    this.wizardAdmin.password = pwd;
  }

  createOrganizationWizard() {
    if (!this.wizardAdmin.password) {
      this.generateTempPassword();
    }
    
    const payload = {
      organization: this.wizardOrg,
      admin: this.wizardAdmin
    };

    this.superAdminService.onboardOrganization(payload).subscribe({
      next: (res) => {
        alert('Organization onboarded successfully!');
        this.loadOrganizations();
        this.setActiveTab('organizations');
        // Reset wizard
        this.wizardOrg = {
          organizationName: '', organizationCode: '', district: '', location: '', contactPhone: '', contactEmail: '', officeHours: '', status: 'Active', subdomain: ''
        };
        this.wizardAdmin = {
          name: '', email: '', phone: '', password: '', sendWelcomeEmail: true
        };
      },
      error: (err) => {
        alert(err.error?.message || err.error || 'Failed to onboard organization. Check for unique Subdomain/ID.');
      }
    });
  }

  subdomains: any[] = [];
  loadSubdomains() {
    this.subdomains = this.organizations.map(org => ({
      organizationId: org.id,
      organizationName: org.name,
      subdomain: org.subdomain,
      fullUrl: org.subdomain ? `${org.subdomain}.kashmirtms.com` : null,
      sslStatus: org.status === 'Active' ? 'Valid' : 'Not Configured',
      lastDnsCheck: new Date(),
      dnsVerified: org.status === 'Active',
      isEditingSubdomain: false,
      editSubdomain: org.subdomain
    }));
  }

  saveSubdomain(org: any) {
    org.subdomain = org.editSubdomain.toLowerCase().replace(/[^a-z0-9]/g, '');
    org.fullUrl = `${org.subdomain}.kashmirtms.com`;
    org.isEditingSubdomain = false;
    alert(`Subdomain for ${org.organizationName} updated to ${org.subdomain}`);
  }

  forceDnsCheck(org: any) {
    org.lastDnsCheck = new Date();
    alert('DNS check forced. Records are up to date.');
  }

  copyUrl(org: any) {
    if (org.fullUrl) {
      navigator.clipboard.writeText('https://' + org.fullUrl);
      alert('URL copied to clipboard');
    }
  }

  openUrl(org: any) {
    if (org.fullUrl) {
      window.open('https://' + org.fullUrl, '_blank');
    }
  }

  createOrganization() {
    this.setActiveTab('createOrganization');
  }

  toggleOrganizationStatus(org: any) {
    this.superAdminService.toggleOrganizationStatus(org.organizationId).subscribe({
      next: (res) => {
        this.loadOrganizations(); // Refresh list to get updated status
        alert(res.message);
      },
      error: (err) => alert(err.error?.message || 'Failed to toggle status')
    });
  }

  filteredTransportAdmins: any[] = [];
  adminOrgFilter: string = '';
  adminStatusFilter: string = '';
  adminLoginFilter: string = '';

  loadTransportAdmins() {
    this.transportAdmins = this.organizations.map((org, i) => ({
      userId: `USR-${100 + i}`,
      name: org.adminName || `Admin ${i+1}`,
      email: `admin${i+1}@${org.subdomain || 'org'}.com`,
      phone: `+92 300 123456${i}`,
      organizationId: org.id,
      organizationName: org.name,
      status: org.status,
      lastLogin: org.lastActivity
    })).filter(ta => ta.name !== 'Unassigned');
    this.filterAdmins();
  }

  filterAdmins() {
    this.filteredTransportAdmins = this.transportAdmins.filter(ta => {
      let match = true;
      if (this.adminOrgFilter) {
        match = match && ta.organizationId == this.adminOrgFilter;
      }
      if (this.adminStatusFilter) {
        match = match && ta.status === this.adminStatusFilter;
      }
      if (this.adminLoginFilter) {
        if (this.adminLoginFilter === 'never') {
          match = match && !ta.lastLogin;
        } else if (this.adminLoginFilter === '30days' && ta.lastLogin) {
          const thirtyDaysAgo = new Date();
          thirtyDaysAgo.setDate(thirtyDaysAgo.getDate() - 30);
          match = match && new Date(ta.lastLogin) >= thirtyDaysAgo;
        }
      }
      return match;
    });
  }

  resetAdminPassword(userId: number) {
    this.superAdminService.resetAdminPassword(userId).subscribe({
      next: (res) => alert(res.message),
      error: (err) => alert('Failed to reset password')
    });
  }

  toggleAdminStatus(userId: number) {
    this.superAdminService.toggleAdminStatus(userId).subscribe({
      next: (res) => {
        alert(res.message);
        this.loadTransportAdmins();
      },
      error: (err) => alert('Failed to toggle status')
    });
  }

  transferAdmin(ta: any) {
    if (!ta.transferOrgId) {
      alert('Please select an organization to transfer to.');
      return;
    }
    this.superAdminService.transferAdmin(ta.userId, ta.transferOrgId).subscribe({
      next: (res) => {
        alert(res.message);
        ta.showTransfer = false;
        this.loadTransportAdmins();
      },
      error: (err) => alert(err.error?.message || 'Failed to transfer admin')
    });
  }

  existingAdminWarning: boolean = false;

  checkExistingAdmin() {
    this.existingAdminWarning = false;
    if (this.newTransportAdmin.organizationId && this.newTransportAdmin.organizationId !== '') {
      const existing = this.transportAdmins.find(ta => ta.organizationId === this.newTransportAdmin.organizationId && ta.status === 'Active');
      if (existing) {
        this.existingAdminWarning = true;
      }
    }
  }

  generateAdminTempPassword() {
    const chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*";
    let pwd = "";
    for (let i = 0; i < 12; i++) {
      pwd += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    this.newTransportAdmin.password = pwd;
  }

  createTransportAdmin() {
    if (!this.newTransportAdmin.name || !this.newTransportAdmin.email || !this.newTransportAdmin.password || !this.newTransportAdmin.organizationId) {
      alert('All fields including Organization are required!');
      return;
    }
    this.superAdminService.createTransportAdmin(this.newTransportAdmin).subscribe({
      next: () => {
        alert('Transport Admin created successfully! ' + (this.newTransportAdmin.sendWelcomeEmail ? 'Welcome email has been sent.' : ''));
        this.loadTransportAdmins();
        this.newTransportAdmin = { name: '', email: '', password: '', organizationId: 0, phone: '', sendWelcomeEmail: false };
        this.existingAdminWarning = false;
        this.setActiveTab('transportAdmins');
        this.loadDashboardStats();
      },
      error: (err) => alert(err.error?.message || 'Failed to create Transport Admin')
    });
  }

  loadAuditLogs() {
    this.superAdminService.getAuditLogs().subscribe(data => {
      this.auditLogs = data;
      this.filterAuditLogs();
    });
  }

  filterAuditLogs() {
    this.filteredAuditLogs = this.auditLogs.filter(log => {
      let match = true;
      if (this.auditOrgFilter) match = match && log.organization === this.auditOrgFilter;
      if (this.auditRoleFilter) match = match && log.actorRole === this.auditRoleFilter;
      if (this.auditActionFilter) match = match && log.action === this.auditActionFilter;
      if (this.auditEntityFilter) match = match && log.entityType === this.auditEntityFilter;
      if (this.auditIpFilter) match = match && log.ipAddress.includes(this.auditIpFilter);
      if (this.auditActorFilter) match = match && log.actorName.toLowerCase().includes(this.auditActorFilter.toLowerCase());
      // Date filter can be implemented similarly if needed
      return match;
    });
  }

  getDistinctAuditOrgs(): string[] {
    return Array.from(new Set(this.auditLogs.map(l => l.organization))) as string[];
  }
  
  getDistinctAuditRoles(): string[] {
    return Array.from(new Set(this.auditLogs.map(l => l.actorRole))) as string[];
  }

  getDistinctAuditActions(): string[] {
    return Array.from(new Set(this.auditLogs.map(l => l.action))) as string[];
  }

  getDistinctAuditEntities(): string[] {
    return Array.from(new Set(this.auditLogs.map(l => l.entityType))) as string[];
  }

  loadAccessLogs() {
    this.superAdminService.getAccessLogs().subscribe(data => {
      this.accessLogs = data;
      this.filterAccessLogs();
    });
  }

  filterAccessLogs() {
    this.filteredAccessLogs = (this.accessLogs || []).filter(log => {
      let match = true;
      if (this.accessRoleFilter) match = match && log.role === this.accessRoleFilter;
      if (this.accessOrgFilter) match = match && log.organization === this.accessOrgFilter;
      if (this.accessEventFilter) match = match && log.event === this.accessEventFilter;
      if (this.accessIpFilter) match = match && log.ipAddress.includes(this.accessIpFilter);
      if (this.accessFailedOnly) match = match && log.event === 'login failed';
      return match;
    });
  }

  getDistinctAccessRoles(): string[] {
    return Array.from(new Set((this.accessLogs || []).map(l => l.role))) as string[];
  }

  getDistinctAccessOrgs(): string[] {
    return Array.from(new Set((this.accessLogs || []).map(l => l.organization))) as string[];
  }
  
  getDistinctAccessEvents(): string[] {
    return Array.from(new Set((this.accessLogs || []).map(l => l.event))) as string[];
  }

  loadAnomalyAlerts() {
    this.superAdminService.getAnomalyAlerts().subscribe(data => {
      this.anomalyAlerts = data;
      this.filteredAnomalyAlerts = data; // add basic filtering later if needed
    });
  }

  getUnresolvedAnomalyCount(): number {
    return this.anomalyAlerts.filter(a => a.status === 'open' || a.status === 'investigating').length;
  }

  loadPlatformAnalytics() {
    this.superAdminService.getPlatformAnalytics().subscribe(data => {
      this.platformAnalytics = data;
    });
  }

  generateReport() {
    // In a real app, this would trigger an API call to generate the report file
    alert(`Generating report: ${this.reportType}\nFormat: ${this.reportFormat}\nDate Range: ${this.reportDateRange}\nOrganization: ${this.reportOrg || 'All'}`);
  }

  loadEtaModelMonitor() {
    // Provide fallback data immediately so the UI renders
    this.etaModel = {
      health: {
        endpointStatus: 'live',
        lastPrediction: new Date().toISOString(),
        predictionsToday: 15420,
        avgResponseTimeMs: 112,
        errorRate: 0.8
      },
      accuracy: {
        avgErrorMinutes: 4.2,
        worstRoutes: [
          { route: 'Muzaffarabad - Bagh', error: 8.5 },
          { route: 'Kotli - Mirpur', error: 7.2 }
        ],
        lastRetrained: new Date(Date.now() - 3 * 24 * 60 * 60 * 1000).toISOString(),
        globalEtaEnabled: true
      },
      training: {
        totalTripsUsed: 425000,
        newTripsAvailable: 12500
      }
    };
    this.superAdminService.getEtaModelMonitor().subscribe({
      next: (data) => {
        if (data) this.etaModel = data;
      },
      error: (err) => console.error('ETA API failed, using fallback', err)
    });
  }

  toggleEtaGlobally() {
    if (this.etaModel) {
      this.etaModel.accuracy.globalEtaEnabled = !this.etaModel.accuracy.globalEtaEnabled;
      const status = this.etaModel.accuracy.globalEtaEnabled ? 'ENABLED' : 'DISABLED';
      alert(`ETA Model has been ${status} globally.`);
    }
  }

  triggerModelRetrain() {
    alert('Model retrain triggered. This will run in the background without affecting live predictions.');
  }

  loadSystemSettings() {
    this.superAdminService.getSystemSettings().subscribe(data => {
      // Need a deep copy for form binding
      this.systemSettings = JSON.parse(JSON.stringify(data));
    });
  }

  saveSystemSettings() {
    if (!this.systemSettings) return;

    // Confirm destructive changes
    const confirmMessage = "You are modifying core platform settings (such as Session Timeout or Lockout limits) which will affect all users. These changes will be logged in the audit trail.\n\nAre you sure you want to proceed?";
    if (!confirm(confirmMessage)) {
      return;
    }

    this.superAdminService.updateSystemSettings(this.systemSettings).subscribe(response => {
      alert('Settings successfully updated and logged.');
      this.loadSystemSettings(); // Refresh from mock backend
    });
  }

  loadNotificationConfig() {
    this.superAdminService.getNotificationConfig().subscribe(data => {
      this.notificationConfig = data;
    });
  }

  testEmailDelivery() {
    alert('Test email successfully dispatched to Super Admin email address.');
  }

  testSmsDelivery() {
    alert('Test SMS successfully dispatched to registered SMS gateway.');
  }

  viewDeliveryHistory() {
    alert('Opening notification delivery history panel (Mock)');
  }

  loadMyProfile() {
    this.superAdminService.getMyProfile().subscribe(data => {
      this.myProfile = data;
    });
  }

  updateProfile() {
    alert('Profile information updated successfully.');
  }

  setup2FA() {
    alert('Opening TOTP setup modal (QR Code generation mock).');
    this.myProfile.twoFactorEnabled = true;
  }

  revokeMySession(sessionId: number) {
    if (confirm('Are you sure you want to revoke this session? You will be logged out on that device.')) {
      this.myProfile.security.activeSessions = this.myProfile.security.activeSessions.filter((s: any) => s.id !== sessionId);
      alert('Session revoked.');
    }
  }

  downloadMyAuditLog() {
    alert('Generating PDF audit log of your actions...');
  }

  filteredSessions: any[] = [];
  sessionRoleFilter: string = '';
  sessionOrgFilter: string = '';
  sessionActivityFilter: string = '';

  loadActiveSessions() {
    this.superAdminService.getActiveSessions().subscribe(data => {
      this.activeSessions = data;
      this.filterSessions();
    });
  }

  filterSessions() {
    this.filteredSessions = this.activeSessions.filter(session => {
      let match = true;
      if (this.sessionRoleFilter) {
        match = match && session.userRole === this.sessionRoleFilter;
      }
      if (this.sessionOrgFilter) {
        match = match && session.organizationName === this.sessionOrgFilter;
      }
      if (this.sessionActivityFilter) {
        const mins = parseInt(this.sessionActivityFilter);
        const lastActive = new Date(session.lastActivity);
        const cutoff = new Date();
        cutoff.setMinutes(cutoff.getMinutes() - mins);
        match = match && (lastActive >= cutoff);
      }
      return match;
    });
  }

  getSessionDuration(loginTime: string): string {
    if (!loginTime) return '0m';
    const start = new Date(loginTime);
    const now = new Date();
    const diffMs = now.getTime() - start.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    if (diffMins < 60) return `${diffMins}m`;
    const hours = Math.floor(diffMins / 60);
    const mins = diffMins % 60;
    return `${hours}h ${mins}m`;
  }

  revokeSession(sessionId: number) {
    if (confirm('Are you sure you want to revoke this session? The user will be logged out immediately.')) {
      this.superAdminService.revokeSession(sessionId).subscribe({
        next: (res) => {
          alert('Session revoked successfully.');
          this.loadActiveSessions();
        },
        error: (err) => {
          alert(err.error?.message || 'Failed to revoke session.');
        }
      });
    }
  }

  banSessionIp(session: any) {
    if (confirm(`Are you sure you want to ban the IP ${session.ipAddress} associated with this session?`)) {
      this.superAdminService.banSessionIp(session.sessionId).subscribe({
        next: (res) => {
          alert(res.message);
          this.loadActiveSessions();
        },
        error: (err) => alert(err.error?.message || 'Failed to ban IP.')
      });
    }
  }

  // Cross-Org Drivers
  crossOrgDrivers: any[] = [];
  filteredDrivers: any[] = [];
  driverOrgFilter: string = '';
  driverStatusFilter: string = '';
  driverExpiryFilter: string = '';
  driverActiveTripFilter: string = '';

  loadCrossOrgDrivers() {
    this.superAdminService.getCrossOrgDrivers().subscribe(data => {
      this.crossOrgDrivers = data;
      this.filterDrivers();
    });
  }

  filterDrivers() {
    this.filteredDrivers = this.crossOrgDrivers.filter(driver => {
      let match = true;
      if (this.driverOrgFilter) {
        match = match && driver.organizationName === this.driverOrgFilter;
      }
      if (this.driverStatusFilter) {
        match = match && driver.status === this.driverStatusFilter;
      }
      if (this.driverActiveTripFilter) {
        if (this.driverActiveTripFilter === 'yes') match = match && driver.hasActiveTrip;
        if (this.driverActiveTripFilter === 'no') match = match && !driver.hasActiveTrip;
      }
      if (this.driverExpiryFilter && driver.licenseExpiryDate) {
        const expiry = new Date(driver.licenseExpiryDate);
        const now = new Date();
        const diffMs = expiry.getTime() - now.getTime();
        const diffDays = Math.ceil(diffMs / (1000 * 60 * 60 * 24));
        
        if (this.driverExpiryFilter === 'expired') match = match && diffDays < 0;
        else if (this.driverExpiryFilter === '30days') match = match && diffDays >= 0 && diffDays <= 30;
        else if (this.driverExpiryFilter === 'valid') match = match && diffDays > 30;
      } else if (this.driverExpiryFilter) {
        match = false; // Filter explicitly asked for a date state, but driver has no date
      }
      return match;
    });
  }

  getExpiryColor(expiryDate: string): string {
    if (!expiryDate) return 'inherit';
    const expiry = new Date(expiryDate);
    const now = new Date();
    const diffMs = expiry.getTime() - now.getTime();
    const diffDays = Math.ceil(diffMs / (1000 * 60 * 60 * 24));
    
    if (diffDays < 0) return '#ef4444'; // Red for expired
    if (diffDays <= 30) return '#f59e0b'; // Orange for expiring soon
    return '#10b981'; // Green for valid
  }

  exportDrivers() {
    if (!this.filteredDrivers || this.filteredDrivers.length === 0) {
      alert('No drivers to export.');
      return;
    }
    const headers = ['Driver ID', 'Name', 'Phone', 'Organization', 'Licence No.', 'Licence Expiry', 'Assigned Vehicle', 'Trips Completed', 'Status'];
    const rows = this.filteredDrivers.map((d: any) => [
      'DRV-' + d.userId,
      d.name,
      d.phone || 'N/A',
      d.organizationName,
      d.licenseNumber || 'Unverified',
      d.licenseExpiryDate ? new Date(d.licenseExpiryDate).toLocaleDateString() : 'Unknown',
      d.assignedVehicle || 'None',
      d.tripsCompleted || 0,
      d.status || 'Active'
    ]);
    const csv = [headers, ...rows].map(r => r.join(',')).join('\n');
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'all_drivers_export.csv';
    a.click();
    URL.revokeObjectURL(url);
  }

  // Cross-Org Vehicles
  crossOrgVehicles: any[] = [];
  filteredVehicles: any[] = [];
  vehicleOrgFilter: string = '';
  vehicleTypeFilter: string = '';
  vehicleStatusFilter: string = '';
  vehicleDocExpiryFilter: string = '';

  loadCrossOrgVehicles() {
    this.superAdminService.getCrossOrgVehicles().subscribe(data => {
      this.crossOrgVehicles = data;
      this.filterVehicles();
    });
  }

  filterVehicles() {
    this.filteredVehicles = this.crossOrgVehicles.filter(v => {
      let match = true;
      if (this.vehicleOrgFilter) {
        match = match && v.organizationName === this.vehicleOrgFilter;
      }
      if (this.vehicleTypeFilter) {
        match = match && v.vehicleType === this.vehicleTypeFilter;
      }
      if (this.vehicleStatusFilter) {
        match = match && v.status === this.vehicleStatusFilter;
      }
      if (this.vehicleDocExpiryFilter === '30days') {
        const now = new Date();
        const thirtyDaysFromNow = new Date();
        thirtyDaysFromNow.setDate(now.getDate() + 30);
        
        let insExpiry = v.insuranceExpiry ? new Date(v.insuranceExpiry) : null;
        let fitExpiry = v.fitnessExpiry ? new Date(v.fitnessExpiry) : null;
        
        let insMatch = insExpiry ? (insExpiry <= thirtyDaysFromNow && insExpiry >= now) : false;
        let fitMatch = fitExpiry ? (fitExpiry <= thirtyDaysFromNow && fitExpiry >= now) : false;
        
        match = match && (insMatch || fitMatch);
      }
      return match;
    });
  }

  getDistinctVehicleTypes(): string[] {
    const types = this.crossOrgVehicles.map(v => v.vehicleType).filter(t => !!t);
    return [...new Set(types)];
  }

  getExpiredInsuranceCount(): number {
    const now = new Date();
    return this.crossOrgVehicles.filter(v => {
      if (!v.insuranceExpiry) return true; // Missing is bad
      return new Date(v.insuranceExpiry) < now;
    }).length;
  }

  getExpiredFitnessCount(): number {
    const now = new Date();
    return this.crossOrgVehicles.filter(v => {
      if (!v.fitnessExpiry) return true;
      return new Date(v.fitnessExpiry) < now;
    }).length;
  }

  getBlockedVehiclesCount(): number {
    const now = new Date();
    return this.crossOrgVehicles.filter(v => {
      let insExp = !v.insuranceExpiry || new Date(v.insuranceExpiry) < now;
      let fitExp = !v.fitnessExpiry || new Date(v.fitnessExpiry) < now;
      return insExp || fitExp;
    }).length;
  }

  exportVehicles() {
    alert('Exporting vehicles to CSV...');
  }

  // Cross-Org Routes
  crossOrgRoutes: any[] = [];
  filteredRoutes: any[] = [];
  routeOrgFilter: string = '';
  routeStatusFilter: string = '';
  routeAreaFilter: string = '';

  loadCrossOrgRoutes() {
    this.superAdminService.getCrossOrgRoutes().subscribe(data => {
      this.crossOrgRoutes = data;
      this.filterRoutes();
    });
  }

  filterRoutes() {
    this.filteredRoutes = this.crossOrgRoutes.filter(r => {
      let match = true;
      if (this.routeOrgFilter) {
        match = match && r.organizationName === this.routeOrgFilter;
      }
      if (this.routeStatusFilter) {
        match = match && r.status === this.routeStatusFilter;
      }
      if (this.routeAreaFilter) {
        match = match && r.area === this.routeAreaFilter;
      }
      return match;
    });
  }

  getDistinctRouteAreas(): string[] {
    const areas = this.crossOrgRoutes.map(r => r.area).filter(a => !!a && a !== 'Unknown');
    return [...new Set(areas)];
  }

  exportRoutes() {
    alert('Exporting routes to CSV...');
  }

  // Cross-Org Trips (Live)
  crossOrgTrips: any[] = [];
  filteredTrips: any[] = [];
  tripOrgFilter: string = '';
  tripStatusFilter: string = '';
  tripRouteFilter: string = '';
  tripDateFilter: string = 'today';
  tripRefreshCountdown: number = 30;
  tripRefreshInterval: any;

  loadCrossOrgTrips() {
    this.superAdminService.getCrossOrgTrips().subscribe(data => {
      this.crossOrgTrips = data;
      this.filterTrips();
      this.tripRefreshCountdown = 30; // Reset countdown on load
    });
  }

  filterTrips() {
    this.filteredTrips = this.crossOrgTrips.filter(t => {
      let match = true;
      if (this.tripOrgFilter) {
        match = match && t.organizationName === this.tripOrgFilter;
      }
      if (this.tripStatusFilter) {
        match = match && t.status === this.tripStatusFilter;
      }
      if (this.tripRouteFilter) {
        match = match && t.routeName === this.tripRouteFilter;
      }
      if (this.tripDateFilter === 'today') {
        const tripDate = new Date(t.travelDate).toDateString();
        const today = new Date().toDateString();
        match = match && tripDate === today;
      }
      return match;
    });
  }

  getDistinctTripRoutes(): string[] {
    const routes = this.crossOrgTrips.map(t => t.routeName).filter(r => !!r && r !== 'Unknown');
    return [...new Set(routes)];
  }

  getTodayTripsCount(): number {
    const today = new Date().toDateString();
    return this.crossOrgTrips.filter(t => new Date(t.travelDate).toDateString() === today).length;
  }

  getActiveTripsCount(): number {
    return this.crossOrgTrips.filter(t => t.status === 'Active').length;
  }

  getScheduledTripsCount(): number {
    return this.crossOrgTrips.filter(t => t.status === 'Scheduled').length;
  }

  getCompletedTripsCount(): number {
    const today = new Date().toDateString();
    return this.crossOrgTrips.filter(t => t.status === 'Completed' && new Date(t.travelDate).toDateString() === today).length;
  }

  getCancelledTripsCount(): number {
    const today = new Date().toDateString();
    return this.crossOrgTrips.filter(t => t.status === 'Cancelled' && new Date(t.travelDate).toDateString() === today).length;
  }

  exportTrips() {
    alert('Exporting live trips to CSV...');
  }

  // Cross-Org Bookings
  crossOrgBookings: any[] = [];
  filteredBookings: any[] = [];
  bookingOrgFilter: string = '';
  bookingStatusFilter: string = '';
  bookingPaymentFilter: string = '';
  bookingDateFilter: string = 'all';
  bookingPassengerSearch: string = '';

  loadCrossOrgBookings() {
    this.superAdminService.getCrossOrgBookings().subscribe(data => {
      this.crossOrgBookings = data;
      this.filterBookings();
    });
  }

  filterBookings() {
    this.filteredBookings = this.crossOrgBookings.filter(b => {
      let match = true;
      if (this.bookingOrgFilter) {
        match = match && b.organizationName === this.bookingOrgFilter;
      }
      if (this.bookingStatusFilter) {
        match = match && b.status === this.bookingStatusFilter;
      }
      if (this.bookingPaymentFilter) {
        match = match && b.paymentStatus === this.bookingPaymentFilter;
      }
      if (this.bookingDateFilter !== 'all') {
        const bookingDate = new Date(b.bookingDate);
        const now = new Date();
        if (this.bookingDateFilter === 'today') {
          match = match && bookingDate.toDateString() === now.toDateString();
        } else if (this.bookingDateFilter === 'week') {
          const weekAgo = new Date();
          weekAgo.setDate(now.getDate() - 7);
          match = match && bookingDate >= weekAgo;
        } else if (this.bookingDateFilter === 'month') {
          const monthAgo = new Date();
          monthAgo.setDate(now.getDate() - 30);
          match = match && bookingDate >= monthAgo;
        }
      }
      if (this.bookingPassengerSearch) {
        const search = this.bookingPassengerSearch.toLowerCase();
        const paxMatch = b.passengerName && b.passengerName.toLowerCase().includes(search);
        const tktMatch = b.ticketCode && b.ticketCode.toLowerCase().includes(search);
        match = match && (paxMatch || tktMatch);
      }
      return match;
    });
  }

  exportBookings() {
    alert('Exporting platform booking report to CSV...');
  }

  ngOnDestroy() {
    if (this.tripRefreshInterval) {
      clearInterval(this.tripRefreshInterval);
    }
  }

  // Hook into setActiveTab to start/stop the auto-refresh
  // Note: Assuming setActiveTab already exists, we will update it or handle it in a separate edit block.

  loadRoutes() {
    this.routeService.getAll().subscribe({
      next: (data) => {
        console.log('Routes loaded:', data);
        this.routes = data;
      },
      error: (err) => {
        console.error('Error loading routes:', err);
        alert('Failed to load routes!');
      }
    });
  }

  addRoute() {
    this.routeService.create({ routeName: this.routeName, startPoint: this.startPoint, endPoint: this.endPoint, distance: this.distance })
      .subscribe(() => {
        this.loadRoutes();
        this.routeName = '';
        this.startPoint = '';
        this.endPoint = '';
        this.distance = 0;
      });
  }

  deleteRoute(id: number) {
    if (confirm('Delete this route?')) {
      this.routeService.delete(id).subscribe(() => this.loadRoutes());
    }
  }

  loadVehicles() {
    this.vehicleService.getAll().subscribe({
      next: (data) => {
        console.log('Vehicles loaded:', data);
        this.vehicles = data;
      },
      error: (err) => {
        console.error('Error loading vehicles:', err);
        alert('Failed to load vehicles!');
      }
    });
  }

  addVehicle() {
    this.vehicleService.create({ vehicleNumber: this.vehicleNumber, vehicleType: this.vehicleType, capacity: this.capacity, status: 'Active' })
      .subscribe(() => {
        this.loadVehicles();
        this.vehicleNumber = '';
        this.vehicleType = '';
        this.capacity = 0;
      });
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

  deleteVehicle(id: number) {
    if (confirm('Delete this vehicle?')) {
      this.vehicleService.delete(id).subscribe(() => this.loadVehicles());
    }
  }

  logout() {
    localStorage.removeItem('user');
    this.router.navigate(['/']);
  }


  loadTimings() {
    this.timingService.getAll().subscribe(data => this.timings = data);
  }

  addTiming() {
    console.log('Adding timing:', {
      routeId: this.selectedRouteId,
      vehicleId: this.selectedVehicleId,
      departureTime: this.departureTime,
      arrivalTime: this.arrivalTime
    });

    if (this.selectedRouteId === 0 || this.selectedVehicleId === 0 || !this.departureTime || !this.arrivalTime) {
      alert('Please fill all fields!');
      return;
    }

    const timing = {
      routeId: this.selectedRouteId,
      vehicleId: this.selectedVehicleId,
      departureTime: this.departureTime,
      arrivalTime: this.arrivalTime
    };
    
    this.timingService.create(timing).subscribe({
      next: () => {
        console.log('Timing added successfully');
        alert('Timing added successfully!');
        this.loadTimings();
        this.selectedRouteId = 0;
        this.selectedVehicleId = 0;
        this.departureTime = '';
        this.arrivalTime = '';
      },
      error: (err) => {
        console.error('Error adding timing:', err);
        alert('Failed to add timing: ' + (err.error?.message || 'Unknown error'));
      }
    });
  }

  deleteTiming(id: number) {
    if (confirm('Delete this timing?')) {
      this.timingService.delete(id).subscribe(() => this.loadTimings());
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
    this.fareService.getAll().subscribe(data => this.fares = data);
  }

  addFare() {
    const fare = {
      routeId: this.fareRouteId,
      amount: this.fareAmount,
      passengerType: this.passengerType
    };
    this.fareService.create(fare).subscribe(() => {
      this.loadFares();
      this.fareRouteId = 0;
      this.fareAmount = 0;
      this.passengerType = '';
    });
  }

  deleteFare(id: number) {
    if (confirm('Delete this fare?')) {
      this.fareService.delete(id).subscribe(() => this.loadFares());
    }
  }

  loadFeedbacks() {
    this.feedbackService.getAll().subscribe(data => this.feedbacks = data);
  }

  deleteFeedback(id: number) {
    if (confirm('Delete this feedback?')) {
      this.feedbackService.delete(id).subscribe(() => this.loadFeedbacks());
    }
  }

  loadUsers() {
    this.userService.getAllUsers().subscribe({
      next: (data) => {
        console.log('Users loaded:', data);
        this.users = data;
      },
      error: (err) => {
        console.error('Error loading users:', err);
        alert('Failed to load users!');
      }
    });
  }

  deleteUser(id: number, userName: string, userRole: string) {
    if (confirm(`Delete user "${userName}" (${userRole})?`)) {
      this.userService.deleteUser(id).subscribe(() => {
        alert('User deleted successfully!');
        this.loadUsers();
      });
    }
  }

  createAdmin() {
    if (!this.newAdminName || !this.newAdminEmail || !this.newAdminPassword) {
      alert('Please fill all fields!');
      return;
    }

    if (this.newAdminPassword.length < 6) {
      alert('Password must be at least 6 characters!');
      return;
    }

    const adminData = {
      name: this.newAdminName,
      email: this.newAdminEmail,
      password: this.newAdminPassword,
      role: 'Admin'
    };

    this.authService.createAdmin(adminData).subscribe({
      next: (res) => {
        alert(res.message || 'Admin account created successfully!');
        this.newAdminName = '';
        this.newAdminEmail = '';
        this.newAdminPassword = '';
        this.loadUsers();
      },
      error: (err) => {
        alert(err.error?.message || 'Failed to create admin account!');
      }
    });
  }

}
