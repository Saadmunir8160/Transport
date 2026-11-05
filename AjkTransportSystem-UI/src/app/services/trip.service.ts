import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TripService {
  private apiUrl = 'https://transport-bmt7jsdg.back4.app/api/Trips';

  constructor(private http: HttpClient) { }

  private getHeaders(): HttpHeaders {
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    return new HttpHeaders({
      'Authorization': `Bearer ${user.token || localStorage.getItem('token')}`
    });
  }
  
  getAll(): Observable<any> { return this.http.get(this.apiUrl, { headers: this.getHeaders() }); }
  getLive(): Observable<any> { return this.http.get(`${this.apiUrl}/live`, { headers: this.getHeaders() }); }
  
  create(payload: any): Observable<any> { return this.http.post(this.apiUrl, payload, { headers: this.getHeaders() }); }
  reassign(id: number, payload: any): Observable<any> { return this.http.put(`${this.apiUrl}/${id}/reassign`, payload, { headers: this.getHeaders() }); }
  modify(id: number, payload: any): Observable<any> { return this.http.put(`${this.apiUrl}/${id}/modify`, payload, { headers: this.getHeaders() }); }
  cancel(id: number, reason: string): Observable<any> { return this.http.put(`${this.apiUrl}/${id}/cancel`, JSON.stringify(reason), { headers: this.getHeaders().set('Content-Type', 'application/json') }); }
  updateStatus(id: number, payload: any): Observable<any> { return this.http.put(`${this.apiUrl}/${id}/status`, payload, { headers: this.getHeaders() }); }
}
