import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class BookingService {
  private apiUrl = 'https://transport-osasaem1.back4.app/api/Bookings';

  constructor(private http: HttpClient) { }

  private getHeaders(): HttpHeaders {
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    return new HttpHeaders({
      'Authorization': `Bearer ${user.token || localStorage.getItem('token')}`
    });
  }
  
  getAll(): Observable<any> { return this.http.get(this.apiUrl, { headers: this.getHeaders() }); }
  
  cancel(id: number, reason: string, adminId: number): Observable<any> { 
      return this.http.put(`${this.apiUrl}/${id}/cancel`, { reason: reason, adminUserId: adminId }, { headers: this.getHeaders() }); 
  }
  
  move(id: number, payload: any): Observable<any> { 
      return this.http.put(`${this.apiUrl}/${id}/move`, payload, { headers: this.getHeaders() }); 
  }
  
  export(id: number): Observable<any> { return this.http.get(`${this.apiUrl}/${id}/export`, { headers: this.getHeaders() }); }
}
