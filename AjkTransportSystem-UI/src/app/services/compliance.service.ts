import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ComplianceService {
  private apiUrl = 'https://transport-bmt7jsdg.back4.app/api/Compliance';

  constructor(private http: HttpClient) { }

  private getHeaders(): HttpHeaders {
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    return new HttpHeaders({
      'Authorization': `Bearer ${user.token || localStorage.getItem('token')}`
    });
  }
  
  getDashboard(): Observable<any> { return this.http.get(`${this.apiUrl}/dashboard`, { headers: this.getHeaders() }); }
  uploadDocument(data: any): Observable<any> { return this.http.post(`${this.apiUrl}/upload`, data, { headers: this.getHeaders() }); }
  exportComplianceReport(): Observable<any> { return this.http.get(`${this.apiUrl}/export`, { headers: this.getHeaders() }); }
}
