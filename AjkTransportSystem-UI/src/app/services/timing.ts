import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TimingService {
  private apiUrl = 'http://localhost:5242/api/Timings';

  constructor(private http: HttpClient) { }

  private getHeaders(): HttpHeaders {
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    return new HttpHeaders({
      'Authorization': `Bearer ${user.token || localStorage.getItem('token')}`
    });
  }
  
  getAll(p1?: any, p2?: any): Observable<any> { return this.http.get(this.apiUrl, { headers: this.getHeaders() }); }
  create(payload: any): Observable<any> { return this.http.post(this.apiUrl, payload, { headers: this.getHeaders() }); }
  update(id: number, payload: any): Observable<any> { return this.http.put(`${this.apiUrl}/${id}`, payload, { headers: this.getHeaders() }); }
  updateStatus(id: number, status: string): Observable<any> { return this.http.put(`${this.apiUrl}/${id}/status`, JSON.stringify(status), { headers: this.getHeaders().set('Content-Type', 'application/json') }); }
  generateTrips(id: number, days: number = 7): Observable<any> { return this.http.post(`${this.apiUrl}/${id}/generate-trips?days=${days}`, {}, { headers: this.getHeaders() }); }
  delete(p1?: any): Observable<any> { return this.http.delete(this.apiUrl + '/' + p1, { headers: this.getHeaders() }); }
}
