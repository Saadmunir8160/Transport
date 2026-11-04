import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AdvisoryService {
  private apiUrl = 'https://transport-osasaem1.back4.app/api/Advisories';

  constructor(private http: HttpClient) { }

  private getHeaders(): HttpHeaders {
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    return new HttpHeaders({
      'Authorization': `Bearer ${user.token || localStorage.getItem('token')}`
    });
  }
  
  getAll(): Observable<any> { return this.http.get(this.apiUrl, { headers: this.getHeaders() }); }
  create(payload: any): Observable<any> { return this.http.post(this.apiUrl, payload, { headers: this.getHeaders() }); }
  close(id: number): Observable<any> { return this.http.put(`${this.apiUrl}/${id}/close`, {}, { headers: this.getHeaders() }); }
}
