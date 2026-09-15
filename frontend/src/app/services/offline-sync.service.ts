import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, fromEvent, Observable, of } from 'rxjs';
import { environment } from '../../environments/environment';
import { catchError } from 'rxjs/operators';

export interface TripDto {
  id: string;
  routeId: string;
  driverId: string;
  vehicleId: string;
  departureTime: string;
  estimatedArrivalTime: string;
  status: string;
}

export interface SyncOperation {
  id?: number;
  url: string;
  method: string;
  body: any;
  timestamp: number;
}

@Injectable({
  providedIn: 'root'
})
export class OfflineSyncService {
  private dbName = 'TransportAppDB';
  private tripStoreName = 'trips';
  private syncStoreName = 'sync-queue';
  private db: IDBDatabase | null = null;
  private isOnline = new BehaviorSubject<boolean>(true);

  public isOnline$ = this.isOnline.asObservable();

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    if (isPlatformBrowser(this.platformId)) {
      this.isOnline.next(navigator.onLine);
      window.addEventListener('online', () => this.updateOnlineStatus());
      window.addEventListener('offline', () => this.updateOnlineStatus());
      this.initDB();
    }
  }

  private updateOnlineStatus() {
    this.isOnline.next(navigator.onLine);
    if (navigator.onLine) {
      this.syncQueuedOperations();
    }
  }

  private initDB(): Promise<void> {
    return new Promise((resolve, reject) => {
      const request = indexedDB.open(this.dbName, 1);

      request.onupgradeneeded = (event: any) => {
        const db = event.target.result;
        if (!db.objectStoreNames.contains(this.tripStoreName)) {
          db.createObjectStore(this.tripStoreName, { keyPath: 'id' });
        }
        if (!db.objectStoreNames.contains(this.syncStoreName)) {
          db.createObjectStore(this.syncStoreName, { keyPath: 'id', autoIncrement: true });
        }
      };

      request.onsuccess = (event: any) => {
        this.db = event.target.result;
        resolve();
      };

      request.onerror = (event: any) => {
        console.error('IndexedDB error', event);
        reject();
      };
    });
  }

  public async cacheTrips(trips: TripDto[]): Promise<void> {
    if (!this.db) await this.initDB();
    return new Promise((resolve, reject) => {
      const tx = this.db!.transaction(this.tripStoreName, 'readwrite');
      const store = tx.objectStore(this.tripStoreName);
      store.clear();
      trips.forEach(trip => store.add(trip));
      tx.oncomplete = () => resolve();
      tx.onerror = () => reject();
    });
  }

  public async getCachedTrips(): Promise<TripDto[]> {
    if (!this.db) await this.initDB();
    return new Promise((resolve, reject) => {
      const tx = this.db!.transaction(this.tripStoreName, 'readonly');
      const store = tx.objectStore(this.tripStoreName);
      const request = store.getAll();
      request.onsuccess = () => resolve(request.result);
      request.onerror = () => reject();
    });
  }

  public async updateTripLocally(id: string, status: string): Promise<void> {
    if (!this.db) await this.initDB();
    return new Promise((resolve, reject) => {
      const tx = this.db!.transaction(this.tripStoreName, 'readwrite');
      const store = tx.objectStore(this.tripStoreName);
      const request = store.get(id);
      request.onsuccess = () => {
        const trip = request.result;
        if (trip) {
          trip.status = status;
          store.put(trip);
        }
        resolve();
      };
      request.onerror = () => reject();
    });
  }

  public async queueSyncOperation(url: string, method: string, body: any): Promise<void> {
    if (!this.db) await this.initDB();
    return new Promise((resolve, reject) => {
      const tx = this.db!.transaction(this.syncStoreName, 'readwrite');
      const store = tx.objectStore(this.syncStoreName);
      const op: SyncOperation = { url, method, body, timestamp: Date.now() };
      store.add(op);
      tx.oncomplete = () => resolve();
      tx.onerror = () => reject();
    });
  }

  private async getQueuedOperations(): Promise<SyncOperation[]> {
    if (!this.db) await this.initDB();
    return new Promise((resolve, reject) => {
      const tx = this.db!.transaction(this.syncStoreName, 'readonly');
      const store = tx.objectStore(this.syncStoreName);
      const request = store.getAll();
      request.onsuccess = () => resolve(request.result);
      request.onerror = () => reject();
    });
  }

  private async clearQueuedOperation(id: number): Promise<void> {
    if (!this.db) return;
    return new Promise((resolve) => {
      const tx = this.db!.transaction(this.syncStoreName, 'readwrite');
      tx.objectStore(this.syncStoreName).delete(id);
      tx.oncomplete = () => resolve();
    });
  }

  public async syncQueuedOperations() {
    const ops = await this.getQueuedOperations();
    for (const op of ops) {
      if (op.method === 'PUT') {
        this.http.put(op.url, op.body).pipe(
          catchError((err) => {
             console.error('Failed to sync operation', err);
             return of(null);
          })
        ).subscribe(res => {
          if (res !== null || (res === null && !this.isOnline.value)) {
             // either successful or handled error. Wait, if online, clear it.
             this.clearQueuedOperation(op.id!);
          }
        });
      }
    }
  }

  public updateTripStatus(tripId: string, status: string): Observable<any> {
    const url = `${environment.apiUrl}/api/driverapp/trips/${tripId}/status`;
    const body = { status };

    if (this.isOnline.value) {
      // Online: send directly, also update local cache
      this.updateTripLocally(tripId, status);
      return this.http.put(url, body);
    } else {
      // Offline: queue it and update local cache
      this.updateTripLocally(tripId, status);
      this.queueSyncOperation(url, 'PUT', body);
      return of({ success: true, offline: true });
    }
  }

  public fetchTrips(): Observable<TripDto[]> {
    if (this.isOnline.value) {
      return new Observable<TripDto[]>(observer => {
        this.http.get<TripDto[]>(`${environment.apiUrl}/api/driverapp/trips`).subscribe({
          next: (trips) => {
            this.cacheTrips(trips);
            observer.next(trips);
            observer.complete();
          },
          error: (err) => {
            this.getCachedTrips().then(trips => {
              observer.next(trips);
              observer.complete();
            });
          }
        });
      });
    } else {
      return new Observable<TripDto[]>(observer => {
        this.getCachedTrips().then(trips => {
          observer.next(trips);
          observer.complete();
        });
      });
    }
  }
}
