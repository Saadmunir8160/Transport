import { environment } from '../../environments/environment';
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class RouteService {
  private apiUrl = `${environment.apiUrl}/routes`;

  constructor(private http: HttpClient) { }

  private getHeaders(): HttpHeaders {
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    return new HttpHeaders({
      'Authorization': `Bearer ${user.token || localStorage.getItem('token')}`
    });
  }
  
  getAll(p1?: any, p2?: any): Observable<any> { return this.http.get(this.apiUrl, { headers: this.getHeaders() }); }
  create(p1?: any, p2?: any, p3?: any): Observable<any> { return this.http.post(this.apiUrl, p1, { headers: this.getHeaders() }); }
  update(id: number, payload: any): Observable<any> { return this.http.put(`${this.apiUrl}/${id}`, payload, { headers: this.getHeaders() }); }
  delete(p1?: any): Observable<any> { return this.http.delete(this.apiUrl + '/' + p1, { headers: this.getHeaders() }); }
  
  duplicateReturn(id: number): Observable<any> { return this.http.post(`${environment.apiUrl}/RoutesStopsExtensions/duplicate-return/${id}`, {}, { headers: this.getHeaders() }); }
  updateStatus(id: number, status: string): Observable<any> { return this.http.put(`${environment.apiUrl}/RoutesStopsExtensions/${id}/status`, JSON.stringify(status), { headers: this.getHeaders().set('Content-Type', 'application/json') }); }
  
  getRouteStops(routeId: number): Observable<any> { return this.http.get(`${environment.apiUrl}/RouteStops/route/${routeId}`, { headers: this.getHeaders() }); }
  addStop(routeId: number, stop: any): Observable<any> { return this.http.post(`${environment.apiUrl}/RoutesStopsExtensions/${routeId}/stops`, stop, { headers: this.getHeaders() }); }
  updateStop(stopId: number, stop: any): Observable<any> { return this.http.put(`${environment.apiUrl}/RoutesStopsExtensions/stops/${stopId}`, stop, { headers: this.getHeaders() }); }
  reorderStops(routeId: number, stopIds: number[]): Observable<any> { return this.http.post(`${environment.apiUrl}/RoutesStopsExtensions/${routeId}/stops/reorder`, stopIds, { headers: this.getHeaders() }); }
}
