import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ReportService {
  private apiUrl = 'https://transport-bmt7jsdg.back4.app/api/Reports';

  constructor(private http: HttpClient) { }

  private getHeaders(): HttpHeaders {
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    return new HttpHeaders({
      'Authorization': `Bearer ${user.token || localStorage.getItem('token')}`
    });
  }
  
  getTrips(filter: any): Observable<any> { return this.http.post(`${this.apiUrl}/trips`, filter, { headers: this.getHeaders() }); }
  getBookings(filter: any): Observable<any> { return this.http.post(`${this.apiUrl}/bookings`, filter, { headers: this.getHeaders() }); }
  getCancellations(filter: any): Observable<any> { return this.http.post(`${this.apiUrl}/cancellations`, filter, { headers: this.getHeaders() }); }
  getPopularRoutes(filter: any): Observable<any> { return this.http.post(`${this.apiUrl}/popular-routes`, filter, { headers: this.getHeaders() }); }
  getVehicleUsage(filter: any): Observable<any> { return this.http.post(`${this.apiUrl}/vehicle-usage`, filter, { headers: this.getHeaders() }); }
  getDriverActivity(filter: any): Observable<any> { return this.http.post(`${this.apiUrl}/driver-activity`, filter, { headers: this.getHeaders() }); }
  
  exportPdf(filter: any): Observable<any> { return this.http.post(`${this.apiUrl}/export/pdf`, filter, { headers: this.getHeaders() }); }
  exportExcel(filter: any): Observable<any> { return this.http.post(`${this.apiUrl}/export/excel`, filter, { headers: this.getHeaders() }); }
}
