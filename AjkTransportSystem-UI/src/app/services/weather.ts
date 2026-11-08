import { environment } from '../../environments/environment';
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

export interface MountainWeatherAlert {
  location: string;
  temperature: number;
  condition: string;
  windSpeed: number;
  precipitation: number;
  severity: 'Safe' | 'Advisory' | 'Severe Hazard';
  alertMessage: string;
  hazardType: string;
}

@Injectable({
  providedIn: 'root'
})
export class WeatherService {
  private apiUrl = `${environment.apiUrl}/weather`;

  // Coordinates for all 10 AJK Districts & Strategic Mountain Passes
  public ajkDistrictCoords: { [key: string]: { lat: number, lon: number } } = {
    'Muzaffarabad': { lat: 34.3597, lon: 73.4711 },
    'Rawalakot': { lat: 33.8584, lon: 73.7604 },
    'Mirpur': { lat: 33.1484, lon: 73.7519 },
    'Kotli': { lat: 33.5156, lon: 73.9019 },
    'Bagh': { lat: 33.9816, lon: 73.7760 },
    'Bhimber': { lat: 32.9741, lon: 74.0785 },
    'Sudhanoti': { lat: 33.7148, lon: 73.6876 },
    'Neelum Valley': { lat: 34.5855, lon: 73.9073 },
    'Haveli': { lat: 33.8686, lon: 74.1031 },
    'Hattian Bala': { lat: 34.1691, lon: 73.7432 },
    'Kohala Pass': { lat: 34.0950, lon: 73.4975 }
  };

  constructor(private http: HttpClient) { }

  private getHeaders(): HttpHeaders {
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    return new HttpHeaders({
      'Authorization': `Bearer ${user.token || localStorage.getItem('token')}`
    });
  }
  
  getAll(p1?: any, p2?: any): Observable<any> { 
    return this.http.get(this.apiUrl, { headers: this.getHeaders() }); 
  }

  getRouteWeather(p1?: any, p2?: any): Observable<any> { 
    return this.http.get(this.apiUrl, { headers: this.getHeaders() }); 
  }

  /**
   * Free Open-Meteo Mountain Weather & Landslide Early Warning
   */
  getLiveMountainWeather(districtName: string): Observable<MountainWeatherAlert> {
    const coords = this.ajkDistrictCoords[districtName] || this.ajkDistrictCoords['Muzaffarabad'];
    const url = `https://api.open-meteo.com/v1/forecast?latitude=${coords.lat}&longitude=${coords.lon}&current=temperature_2m,relative_humidity_2m,precipitation,weather_code,wind_speed_10m&timezone=Asia%2FKarachi`;

    return this.http.get<any>(url).pipe(
      map(res => {
        const cur = res.current || {};
        const code = cur.weather_code || 0;
        const temp = Math.round(cur.temperature_2m ?? 18);
        const precip = cur.precipitation ?? 0;
        const wind = cur.wind_speed_10m ?? 8;

        let condition = 'Clear Skies';
        let severity: 'Safe' | 'Advisory' | 'Severe Hazard' = 'Safe';
        let hazardType = 'Clear Road';
        let alertMessage = `Roads dry and clear in ${districtName}. Normal mountain transit conditions.`;

        if (code >= 95) {
          condition = 'Severe Thunderstorm';
          severity = 'Severe Hazard';
          hazardType = 'Landslide & Rockfall Warning';
          alertMessage = `CRITICAL: Thunderstorm active in ${districtName}. Landslide buffer applied: +45 min delay.`;
        } else if (code >= 71) {
          condition = 'Snowfall / Ice';
          severity = 'Severe Hazard';
          hazardType = 'Snow & Slippery Slopes';
          alertMessage = `High altitude freeze: Tyre chains or slow driving required across passes.`;
        } else if (code >= 61 || precip > 2.0) {
          condition = 'Heavy Rain';
          severity = 'Advisory';
          hazardType = 'Slippery Mountain Bends';
          alertMessage = `Continuous precipitation detected. Drive with fog lights near river gorges.`;
        } else if (code >= 45) {
          condition = 'Dense Fog';
          severity = 'Advisory';
          hazardType = 'Low Visibility';
          alertMessage = `Dense fog observed along the river corridor. Maintain safe braking gap.`;
        }

        return {
          location: districtName,
          temperature: temp,
          condition,
          windSpeed: wind,
          precipitation: precip,
          severity,
          alertMessage,
          hazardType
        };
      }),
      catchError(() => {
        return of({
          location: districtName,
          temperature: 19,
          condition: 'Partly Cloudy',
          windSpeed: 10,
          precipitation: 0,
          severity: 'Safe' as const,
          alertMessage: `Standard mountain transit operations active across ${districtName}.`,
          hazardType: 'Normal Route'
        });
      })
    );
  }
}
