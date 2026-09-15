import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TransportAdminService {
  private apiUrl = 'https://localhost:7194/api'; // Match your backend port

  constructor(private http: HttpClient) { }

  // Drivers
  getDrivers(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/drivers`);
  }
  createDriver(driver: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/drivers`, driver);
  }

  // Vehicles
  getVehicles(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/vehicles`);
  }
  createVehicle(vehicle: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/vehicles`, vehicle);
  }

  // Routes
  getRoutes(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/routes`);
  }
  createRoute(route: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/routes`, route);
  }

  // Trips
  getTrips(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/trips`);
  }
  createTrip(trip: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/trips`, trip);
  }
}
