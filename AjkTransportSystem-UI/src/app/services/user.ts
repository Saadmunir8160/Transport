import { environment } from '../../environments/environment';
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = `${environment.apiUrl}/Auth`;

  constructor(private http: HttpClient) { }

  private getHeaders(): HttpHeaders {
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    return new HttpHeaders({
      'Authorization': `Bearer ${user.token || localStorage.getItem('token')}`
    });
  }
  
  getAll(p1?: any, p2?: any): Observable<any> { return this.http.get(this.apiUrl, { headers: this.getHeaders() }); }
  getAllUsers(p1?: any, p2?: any): Observable<any> { return this.http.get(this.apiUrl, { headers: this.getHeaders() }); }
  deleteUser(p1?: any): Observable<any> { return this.http.delete(this.apiUrl + '/' + p1, { headers: this.getHeaders() }); }
}
