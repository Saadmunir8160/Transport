import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'https://transport-osasaem1.back4.app/api/Auth';

  constructor(private http: HttpClient) { }

  private getHeaders(): HttpHeaders {
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    return new HttpHeaders({
      'Authorization': `Bearer ${user.token || localStorage.getItem('token')}`
    });
  }
  
  login(p1?: any, p2?: any, p3?: any): Observable<any> { return this.http.post(this.apiUrl + '/login', p1, { headers: this.getHeaders() }); }
  register(p1?: any, p2?: any, p3?: any): Observable<any> { return this.http.post(this.apiUrl + '/register', p1, { headers: this.getHeaders() }); }
  createAdmin(p1?: any, p2?: any, p3?: any): Observable<any> { return this.http.post(this.apiUrl + '/createAdmin', p1, { headers: this.getHeaders() }); }
}
