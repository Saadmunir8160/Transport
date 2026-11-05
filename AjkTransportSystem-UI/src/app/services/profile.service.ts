import { environment } from '../../environments/environment';
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ProfileService {
  private apiUrl = `${environment.apiUrl}/Profile`;

  constructor(private http: HttpClient) { }

  private getHeaders(): HttpHeaders {
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    return new HttpHeaders({
      'Authorization': `Bearer ${user.token || localStorage.getItem('token')}`
    });
  }
  
  getProfile(): Observable<any> { return this.http.get(`${this.apiUrl}`, { headers: this.getHeaders() }); }
  updateProfile(data: any): Observable<any> { return this.http.put(`${this.apiUrl}`, data, { headers: this.getHeaders() }); }
  changePassword(data: any): Observable<any> { return this.http.put(`${this.apiUrl}/change-password`, data, { headers: this.getHeaders() }); }
  
  getNotifications(type?: string): Observable<any> { 
      const url = type ? `${this.apiUrl}/notifications?type=${type}` : `${this.apiUrl}/notifications`;
      return this.http.get(url, { headers: this.getHeaders() }); 
  }
  markNotificationAsRead(id: number): Observable<any> { return this.http.put(`${this.apiUrl}/notifications/${id}/read`, {}, { headers: this.getHeaders() }); }
  chooseChannels(data: any): Observable<any> { return this.http.post(`${this.apiUrl}/notifications/channels`, data, { headers: this.getHeaders() }); }
}
