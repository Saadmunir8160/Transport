import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ETAInsightsService {
  private apiUrl = 'http://localhost:5242/api/ETAInsights';

  constructor(private http: HttpClient) { }

  private getHeaders(): HttpHeaders {
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    return new HttpHeaders({
      'Authorization': `Bearer ${user.token || localStorage.getItem('token')}`
    });
  }
  
  getPredictedVsActual(routeId?: number): Observable<any> { 
      const url = routeId ? `${this.apiUrl}/predicted-vs-actual?routeId=${routeId}` : `${this.apiUrl}/predicted-vs-actual`;
      return this.http.get(url, { headers: this.getHeaders() }); 
  }
  
  getAverageDelay(): Observable<any> { return this.http.get(`${this.apiUrl}/average-delay`, { headers: this.getHeaders() }); }
  getDelayFactors(): Observable<any> { return this.http.get(`${this.apiUrl}/delay-factors`, { headers: this.getHeaders() }); }
  getModelAccuracy(): Observable<any> { return this.http.get(`${this.apiUrl}/model-accuracy`, { headers: this.getHeaders() }); }
}
