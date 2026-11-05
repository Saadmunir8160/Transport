import { environment } from '../../environments/environment';
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class VehicleService {
  private apiUrl = `${environment.apiUrl}/vehicles`;

  constructor(private http: HttpClient) { }

  private getHeaders(): HttpHeaders {
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    return new HttpHeaders({
      'Authorization': `Bearer ${user.token || localStorage.getItem('token')}`
    });
  }
  
  getAll(p1?: any, p2?: any): Observable<any> { return this.http.get(this.apiUrl, { headers: this.getHeaders() }); }
  create(p1?: any, p2?: any, p3?: any): Observable<any> { return this.http.post(this.apiUrl + '/create', p1, { headers: this.getHeaders() }); }
  delete(p1?: any): Observable<any> { return this.http.delete(this.apiUrl + '/' + p1, { headers: this.getHeaders() }); }
}
