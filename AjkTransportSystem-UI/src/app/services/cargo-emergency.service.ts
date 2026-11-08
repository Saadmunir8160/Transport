import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface CargoShipment {
  cargoId: number;
  trackingNumber: string;
  senderName: string;
  senderPhone: string;
  receiverName: string;
  receiverPhone: string;
  originCity: string;
  destinationCity: string;
  weightKg: number;
  packageType: string;
  itemDescription: string;
  assignedVehicleNo: string;
  driverName: string;
  driverPhone: string;
  status: string;
  currentLocation: string;
  estimatedDelivery?: string;
  charges: number;
  paymentStatus: string;
  createdAt: string;
}

export interface EmergencyBroadcastRequest {
  senderName?: string;
  senderRole?: string;
  senderPhone?: string;
  emergencyType: string;
  latitude: number;
  longitude: number;
  locationDescription?: string;
  vehicleNumber?: string;
  routeName?: string;
}

@Injectable({
  providedIn: 'root'
})
export class CargoEmergencyService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    const token = user.token || localStorage.getItem('token');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      ...(token ? { 'Authorization': `Bearer ${token}` } : {})
    });
  }

  trackCargo(trackingNumber: string): Observable<CargoShipment> {
    return this.http.get<CargoShipment>(`${this.apiUrl}/Cargo/track/${encodeURIComponent(trackingNumber)}`, {
      headers: this.getHeaders()
    });
  }

  broadcastEmergency(request: EmergencyBroadcastRequest): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/Emergency/broadcast`, request, {
      headers: this.getHeaders()
    });
  }

  // Free Open-Meteo live mountain weather for AJK coordinates
  getMountainWeather(lat: number, lon: number): Observable<any> {
    const url = `https://api.open-meteo.com/v1/forecast?latitude=${lat}&longitude=${lon}&current=temperature_2m,relative_humidity_2m,precipitation,weather_code,wind_speed_10m&timezone=Asia%2FKarachi`;
    return this.http.get<any>(url);
  }
}

