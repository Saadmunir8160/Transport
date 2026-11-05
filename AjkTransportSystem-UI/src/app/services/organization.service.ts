import { environment } from '../../environments/environment';
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class OrganizationService {
  private apiUrl = `${environment.apiUrl}/Organization`;

  constructor(private http: HttpClient) { }

  private getHeaders(): HttpHeaders {
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    return new HttpHeaders({
      'Authorization': `Bearer ${user.token || localStorage.getItem('token')}`
    });
  }
  
  getProfile(): Observable<any> { return this.http.get(`${this.apiUrl}/profile`, { headers: this.getHeaders() }); }
  
  updateProfile(data: any): Observable<any> { return this.http.put(`${this.apiUrl}/profile`, data, { headers: this.getHeaders() }); }
  
  updateSuperAdminFields(data: any): Observable<any> { return this.http.put(`${this.apiUrl}/super-admin/profile`, data, { headers: this.getHeaders() }); }
}
