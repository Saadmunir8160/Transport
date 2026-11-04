import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PassengerService {
  private apiUrl = 'http://localhost:5242/api/passenger';

  constructor(private http: HttpClient) { }

  private getHeaders(): HttpHeaders {
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    return new HttpHeaders({
      'Authorization': `Bearer ${user.token || localStorage.getItem('token')}`
    });
  }

  searchTrips(startPoint: string, endPoint: string, date: string): Observable<any> {
    let url = `${this.apiUrl}/trips/search?`;
    if (startPoint) url += `startPoint=${startPoint}&`;
    if (endPoint) url += `endPoint=${endPoint}&`;
    if (date) url += `travelDate=${date}`;
    return this.http.get(url, { headers: this.getHeaders() });
  }

  getTripSeats(tripId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/trips/${tripId}/seats`, { headers: this.getHeaders() });
  }

  bookTrip(bookingData: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/bookings`, bookingData, { headers: this.getHeaders() });
  }

  getMyBookings(): Observable<any> {
    return this.http.get(`${this.apiUrl}/bookings`, { headers: this.getHeaders() });
  }

  getTicket(bookingId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/bookings/${bookingId}/ticket`, { headers: this.getHeaders() });
  }

  cancelTrip(bookingId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/bookings/${bookingId}`, { headers: this.getHeaders() });
  }
}
