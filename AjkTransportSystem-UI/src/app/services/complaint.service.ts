import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ComplaintService {
  private apiUrl = 'https://transport-bmt7jsdg.back4.app/api/Complaints';

  constructor(private http: HttpClient) { }

  private getHeaders(): HttpHeaders {
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    return new HttpHeaders({
      'Authorization': `Bearer ${user.token || localStorage.getItem('token')}`
    });
  }
  
  getPassengerComplaints(): Observable<any> { return this.http.get(`${this.apiUrl}/passenger`, { headers: this.getHeaders() }); }
  getDriverIssues(): Observable<any> { return this.http.get(`${this.apiUrl}/driver`, { headers: this.getHeaders() }); }
  
  resolve(id: number, payload: any): Observable<any> { return this.http.put(`${this.apiUrl}/${id}/resolve`, payload, { headers: this.getHeaders() }); }
  getAttachments(id: number): Observable<any> { return this.http.get(`${this.apiUrl}/${id}/attachments`, { headers: this.getHeaders() }); }
}
