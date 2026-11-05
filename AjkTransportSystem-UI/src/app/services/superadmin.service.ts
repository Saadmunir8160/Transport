import { environment } from '../../environments/environment';
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SuperAdminService {
  private apiUrl = `${environment.apiUrl}/admin`;

  constructor(private http: HttpClient) { }

  private getHeaders(): HttpHeaders {
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    return new HttpHeaders({
      'Authorization': `Bearer ${user.token || localStorage.getItem('token')}`
    });
  }

  getCrossOrgDrivers(p?: any): Observable<any> { return this.http.get(`${this.apiUrl}/drivers`, { headers: this.getHeaders() }); }
  getCrossOrgVehicles(p?: any): Observable<any> { return this.http.get(`${this.apiUrl}/vehicles`, { headers: this.getHeaders() }); }
  getCrossOrgRoutes(p?: any): Observable<any> { return this.http.get(`${this.apiUrl}/routes`, { headers: this.getHeaders() }); }
  getCrossOrgTrips(p?: any): Observable<any> { return this.http.get(`${this.apiUrl}/trips`, { headers: this.getHeaders() }); }
  getCrossOrgBookings(p?: any): Observable<any> { return this.http.get(`${this.apiUrl}/bookings`, { headers: this.getHeaders() }); }
  
  getSystemAlerts(p?: any): Observable<any> { return this.http.get(`${this.apiUrl}/alerts`, { headers: this.getHeaders() }); }
  getSystemHealth(p?: any): Observable<any> { return this.http.get(`${this.apiUrl}/health`, { headers: this.getHeaders() }); }
  getOfflineSyncLogs(p?: any): Observable<any> { return this.http.get(`${this.apiUrl}/sync-logs`, { headers: this.getHeaders() }); }
  getChartAnalytics(p?: any): Observable<any> { return this.http.get(`${this.apiUrl}/analytics`, { headers: this.getHeaders() }); }
  onboardOrganization(p?: any): Observable<any> { return this.http.post(`${this.apiUrl}/onboard`, p, { headers: this.getHeaders() }); }
  toggleOrganizationStatus(p?: any): Observable<any> { return this.http.post(`${this.apiUrl}/toggle-org`, p, { headers: this.getHeaders() }); }
  resetAdminPassword(p?: any): Observable<any> { return this.http.post(`${this.apiUrl}/reset-password`, p, { headers: this.getHeaders() }); }
  toggleAdminStatus(p?: any): Observable<any> { return this.http.post(`${this.apiUrl}/toggle-admin`, p, { headers: this.getHeaders() }); }
  transferAdmin(p1?: any, p2?: any): Observable<any> { return this.http.post(`${this.apiUrl}/transfer-admin`, p1, { headers: this.getHeaders() }); }
  createTransportAdmin(p?: any): Observable<any> { return this.http.post(`${this.apiUrl}/create-ta`, p, { headers: this.getHeaders() }); }
  getAuditLogs(p?: any): Observable<any> { return this.http.get(`${this.apiUrl}/audit-logs`, { headers: this.getHeaders() }); }
  getAccessLogs(p?: any): Observable<any> { return this.http.get(`${this.apiUrl}/access-logs`, { headers: this.getHeaders() }); }
  getAnomalyAlerts(p?: any): Observable<any> { return this.http.get(`${this.apiUrl}/anomaly-alerts`, { headers: this.getHeaders() }); }
  getPlatformAnalytics(p?: any): Observable<any> { return this.http.get(`${this.apiUrl}/platform-analytics`, { headers: this.getHeaders() }); }
  getEtaModelMonitor(p?: any): Observable<any> { return this.http.get(`${this.apiUrl}/eta-monitor`, { headers: this.getHeaders() }); }
  getSystemSettings(p?: any): Observable<any> { return this.http.get(`${this.apiUrl}/settings`, { headers: this.getHeaders() }); }
  updateSystemSettings(p?: any): Observable<any> { return this.http.post(`${this.apiUrl}/settings`, p, { headers: this.getHeaders() }); }
  getNotificationConfig(p?: any): Observable<any> { return this.http.get(`${this.apiUrl}/notif-config`, { headers: this.getHeaders() }); }
  getMyProfile(p?: any): Observable<any> { return this.http.get(`${this.apiUrl}/profile`, { headers: this.getHeaders() }); }
  getActiveSessions(p?: any): Observable<any> { return this.http.get(`${this.apiUrl}/sessions`, { headers: this.getHeaders() }); }
  revokeSession(p?: any): Observable<any> { return this.http.post(`${this.apiUrl}/revoke-session`, p, { headers: this.getHeaders() }); }
  banSessionIp(p?: any): Observable<any> { return this.http.post(`${this.apiUrl}/ban-ip`, p, { headers: this.getHeaders() }); }
}
