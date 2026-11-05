import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface MountainDelayPrediction {
  routeName: string;
  terrainType: string;
  weatherCondition: string;
  baseDurationMinutes: number;
  predictedDelayMinutes: number;
  totalEstimatedMinutes: number;
  severityLevel: string;
  recommendation: string;
  hazardFactors: string[];
}

@Injectable({
  providedIn: 'root'
})
export class AiService {
  private apiUrl = 'https://transport-bmt7jsdg.back4.app/api/AI';

  constructor(private http: HttpClient) { }

  private getHeaders(): HttpHeaders {
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    return new HttpHeaders({
      'Authorization': `Bearer ${user.token || localStorage.getItem('token')}`
    });
  }

  getMountainDelayPrediction(startPoint: string, endPoint: string, distance?: number, weather?: string): Observable<MountainDelayPrediction> {
    const params = `?startPoint=${encodeURIComponent(startPoint)}&endPoint=${encodeURIComponent(endPoint)}&distance=${distance || 110}&weather=${encodeURIComponent(weather || 'Rainy')}`;
    return this.http.get<MountainDelayPrediction>(`${this.apiUrl}/mountain-delay-prediction${params}`, { headers: this.getHeaders() });
  }

  getRecommendations(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/recommendations`, { headers: this.getHeaders() });
  }
}
