import { environment } from '../../environments/environment';
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DriverService {
  private apiUrl = `${environment.apiUrl}/Drivers`;

  constructor(private http: HttpClient) {}
  
  getVehicle(): Observable<any> { return this.http.get(`${environment.apiUrl}/driver/vehicle`, { headers: this.getHeaders() }); }
  getRouteStops(): Observable<any> { return this.http.get(`${environment.apiUrl}/driver/route`, { headers: this.getHeaders() }); }
  getPassengers(): Observable<any> { return this.http.get(`${environment.apiUrl}/driver/passengers`, { headers: this.getHeaders() }); }
  getPreStartChecklist(): Observable<any> { return this.http.get(`${environment.apiUrl}/driver/prestart`, { headers: this.getHeaders() }); }
  startTrip(tripId: number): Observable<any> { return this.http.post(`${environment.apiUrl}/driver/${tripId}/start`, {}, { headers: this.getHeaders() }); }
  markNoShow(bookingId: number): Observable<any> { return this.http.post(`${environment.apiUrl}/driver/passengers/${bookingId}/noshow`, {}, { headers: this.getHeaders() }); }


  private getHeaders(): HttpHeaders {
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    return new HttpHeaders({
      'Authorization': `Bearer ${user.token || localStorage.getItem('token')}`
    });
  }
  
  getAll(): Observable<any> { return this.http.get(`${this.apiUrl}`, { headers: this.getHeaders() }); }
  add(data: any): Observable<any> { return this.http.post(`${this.apiUrl}`, data, { headers: this.getHeaders() }); }
  update(id: number, data: any): Observable<any> { return this.http.put(`${this.apiUrl}/${id}`, data, { headers: this.getHeaders() }); }
  changeStatus(id: number, status: string): Observable<any> { return this.http.put(`${this.apiUrl}/${id}/status`, { status }, { headers: this.getHeaders() }); }
  getHistory(id: number): Observable<any> { return this.http.get(`${this.apiUrl}/${id}/history`, { headers: this.getHeaders() }); }
  getTripHistory(): Observable<any> { return this.http.get(`${environment.apiUrl}/driver/history`, { headers: this.getHeaders() }); }
  getAssignedTrips(): Observable<any> { return this.http.get(`${environment.apiUrl}/driver/trips`, { headers: this.getHeaders() }); }
  updateTripStatus(tripId: number, status: string): Observable<any> { return this.http.put(`${environment.apiUrl}/driver/trips/${tripId}/status`, { status }, { headers: this.getHeaders() }); }
  getHomeData(): Observable<any> { return this.http.get(`${environment.apiUrl}/driver/home`, { headers: this.getHeaders() }); }
  
  getActiveTrip(): Observable<any> { return this.http.get(`${environment.apiUrl}/driver/active-trip`, { headers: this.getHeaders() }); }
  markStopReached(tripId: number, stopId: number, data: any): Observable<any> { return this.http.post(`${environment.apiUrl}/driver/${tripId}/stops/${stopId}/reach`, data, { headers: this.getHeaders() }); }
  endTrip(tripId: number, data: any): Observable<any> { return this.http.post(`${environment.apiUrl}/driver/${tripId}/end`, data, { headers: this.getHeaders() }); }
  getIssues(): Observable<any> { return this.http.get(`${environment.apiUrl}/driver/issues`, { headers: this.getHeaders() }); }
  reportIssue(data: any): Observable<any> { return this.http.post(`${environment.apiUrl}/driver/issues`, data, { headers: this.getHeaders() }); }
  getProfile(): Observable<any> { return this.http.get(`${environment.apiUrl}/driver/profile`, { headers: this.getHeaders() }); }
  updateProfileSettings(data: any): Observable<any> { return this.http.put(`${environment.apiUrl}/driver/profile/settings`, data, { headers: this.getHeaders() }); }
  getNotifications(): Observable<any> { return this.http.get(`${environment.apiUrl}/driver/notifications`, { headers: this.getHeaders() }); }
  markNotificationRead(id: number): Observable<any> { return this.http.put(`${environment.apiUrl}/driver/notifications/${id}/read`, {}, { headers: this.getHeaders() }); }
}
