import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Observable, Subject } from 'rxjs';

export interface LocationTelemetry {
  tripId: string;
  latitude: number;
  longitude: number;
  speed: number;
  currentStop: string;
  timestamp: string;
}

export interface StopAlert {
  tripId: string;
  stopName: string;
  etaNext: string;
  timestamp: string;
}

@Injectable({
  providedIn: 'root'
})
export class RealtimeTrackingService {
  private hubConnection: signalR.HubConnection | null = null;
  private locationSubject = new Subject<LocationTelemetry>();
  private stopAlertSubject = new Subject<StopAlert>();
  private hubUrl = 'https://transport-bmt7jsdg.back4.app/hubs/tracking';

  public location$ = this.locationSubject.asObservable();
  public stopAlert$ = this.stopAlertSubject.asObservable();
  public isConnected: boolean = false;

  public startConnection(tripId: string = '1'): Promise<void> {
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      return Promise.resolve();
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000])
      .configureLogging(signalR.LogLevel.None)
      .build();

    this.registerServerEvents();

    return this.hubConnection
      .start()
      .then(() => {
        this.isConnected = true;
        this.hubConnection?.invoke('JoinTripGroup', tripId.toString()).catch(() => {});
      })
      .catch((err) => {
        this.isConnected = false;
        // Fallback to seamless client simulation if cloud websockets proxy is pending
      });
  }

  private registerServerEvents(): void {
    if (!this.hubConnection) return;

    this.hubConnection.on('ReceiveLocationUpdate', (data: LocationTelemetry) => {
      this.locationSubject.next(data);
    });

    this.hubConnection.on('ReceiveStopAlert', (data: StopAlert) => {
      this.stopAlertSubject.next(data);
    });
  }

  public broadcastLocation(tripId: string, lat: number, lng: number, speed: number, currentStop: string): void {
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      this.hubConnection.invoke('SendLocationUpdate', tripId, lat, lng, speed, currentStop).catch(() => {});
    }
  }

  public broadcastStopReached(tripId: string, stopName: string, etaNext: string): void {
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      this.hubConnection.invoke('SendStopReachedAlert', tripId, stopName, etaNext).catch(() => {});
    }
  }

  public stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop();
      this.isConnected = false;
    }
  }
}

