import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TransportAdminService {
  private baseUrl = 'https://transport-bmt7jsdg.back4.app/api';

  constructor(private http: HttpClient) { }

  private getHeaders(): HttpHeaders {
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    return new HttpHeaders({
      'Authorization': `Bearer ${user.token || localStorage.getItem('token')}`
    });
  }
  
  getAll(p1?: any, p2?: any): Observable<any> { return this.http.get(this.baseUrl + '/dashboard', { headers: this.getHeaders() }); }
  getDashboardStats(p1?: any, p2?: any): Observable<any> { return this.http.get(this.baseUrl + '/dashboard/stats', { headers: this.getHeaders() }); }
  getRoutes(p1?: any, p2?: any): Observable<any> { return this.http.get(this.baseUrl + '/routes', { headers: this.getHeaders() }); }
  addRoute(p1?: any, p2?: any, p3?: any): Observable<any> { return this.http.post(this.baseUrl + '/routes', p1, { headers: this.getHeaders() }); }
  getVehicles(p1?: any, p2?: any): Observable<any> { return this.http.get(this.baseUrl + '/vehicles', { headers: this.getHeaders() }); }
  addVehicle(p1?: any, p2?: any, p3?: any): Observable<any> { return this.http.post(this.baseUrl + '/vehicles', p1, { headers: this.getHeaders() }); }
  getDrivers(p1?: any, p2?: any): Observable<any> { return this.http.get(this.baseUrl + '/transportadmin/drivers', { headers: this.getHeaders() }); }
  addDriver(p1?: any, p2?: any, p3?: any): Observable<any> { return this.http.post(this.baseUrl + '/transportadmin/drivers', p1, { headers: this.getHeaders() }); }
  getSchedules(p1?: any, p2?: any): Observable<any> { return this.http.get(this.baseUrl + '/timings', { headers: this.getHeaders() }); }
  addSchedule(p1?: any, p2?: any, p3?: any): Observable<any> { return this.http.post(this.baseUrl + '/timings', p1, { headers: this.getHeaders() }); }
  getTrips(p1?: any, p2?: any): Observable<any> { return this.http.get(this.baseUrl + '/trips', { headers: this.getHeaders() }); }
  addTrip(p1?: any, p2?: any, p3?: any): Observable<any> { return this.http.post(this.baseUrl + '/trips', p1, { headers: this.getHeaders() }); }
  getTripBookings(p1?: any, p2?: any): Observable<any> { return this.http.get(this.baseUrl + '/bookings', { headers: this.getHeaders() }); }
}
