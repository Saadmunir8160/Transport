import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class OrganizationService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7111/api/Organizations'; // Replace with actual backend port

  getOrganizations(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl);
  }

  getOrganization(id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }

  createOrganization(data: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, data);
  }

  updateOrganization(id: string, data: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, data);
  }

  deleteOrganization(id: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`);
  }

  createAdmin(orgId: string, data: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${orgId}/admin`, data);
  }
}
