import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PassengerService {
  private apiUrl = 'https://localhost:7194/api'; // Match your backend port

  constructor(private http: HttpClient) { }

  searchTrips(originId: string, destinationId: string, date: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/passenger/trips/search?originId=${originId}&destinationId=${destinationId}&date=${date}`);
  }

  getTripSeats(tripId: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/passenger/trips/${tripId}/seats`);
  }

  createBooking(booking: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/passenger/bookings`, booking);
  }

  getMyBookings(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/passenger/bookings/my-bookings`);
  }
}
