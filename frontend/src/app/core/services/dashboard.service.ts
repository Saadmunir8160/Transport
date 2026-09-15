import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7111/api/Dashboard';

  getSuperAdminStats(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/superadmin-stats`);
  }
}
