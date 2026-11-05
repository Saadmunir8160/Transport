import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class VehicleService {
  private apiUrl = 'https://transport-bmt7jsdg.back4.app/api/Vehicles';

  constructor(private http: HttpClient) { }

  private getHeaders(): HttpHeaders {
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    return new HttpHeaders({
      'Authorization': `Bearer ${user.token || localStorage.getItem('token')}`
    });
  }
  
  getAll(): Observable<any> { return this.http.get(`${this.apiUrl}`, { headers: this.getHeaders() }); }
  add(data: any): Observable<any> { return this.http.post(`${this.apiUrl}`, data, { headers: this.getHeaders() }); }
  update(id: number, data: any): Observable<any> { return this.http.put(`${this.apiUrl}/${id}`, data, { headers: this.getHeaders() }); }
  changeStatus(id: number, status: string): Observable<any> { return this.http.put(`${this.apiUrl}/${id}/status`, { status }, { headers: this.getHeaders() }); }
  getUsage(id: number): Observable<any> { return this.http.get(`${this.apiUrl}/${id}/usage`, { headers: this.getHeaders() }); }
}
