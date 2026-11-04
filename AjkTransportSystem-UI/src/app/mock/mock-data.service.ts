import { Injectable } from '@angular/core';
import { Observable, of, delay } from 'rxjs';

export interface Organization {
  id: string;
  name: string;
  area: string;
  subdomain: string;
  adminName: string;
  totalDrivers: number;
  totalVehicles: number;
  totalTrips: number;
  status: 'Active' | 'Inactive';
  createdAt: Date;
  lastActivity: Date;
}

@Injectable({
  providedIn: 'root'
})
export class MockDataService {
  private organizations: Organization[] = [
    { id: 'ORG-001', name: 'Muzaffarabad Transport', area: 'Muzaffarabad', subdomain: 'muzaffarabad', adminName: 'Ali Khan', totalDrivers: 15, totalVehicles: 12, totalTrips: 1500, status: 'Active', createdAt: new Date('2023-01-15'), lastActivity: new Date() },
    { id: 'ORG-002', name: 'Mirpur Bus Union', area: 'Mirpur', subdomain: 'mirpur', adminName: 'Zainab Ahmed', totalDrivers: 20, totalVehicles: 15, totalTrips: 2100, status: 'Active', createdAt: new Date('2023-02-10'), lastActivity: new Date() },
    { id: 'ORG-003', name: 'Kotli Mobility', area: 'Kotli', subdomain: 'kotli', adminName: 'Omar Farooq', totalDrivers: 8, totalVehicles: 6, totalTrips: 400, status: 'Active', createdAt: new Date('2023-03-05'), lastActivity: new Date() },
    { id: 'ORG-004', name: 'Rawalakot Transit', area: 'Rawalakot', subdomain: 'rawalakot', adminName: 'Unassigned', totalDrivers: 0, totalVehicles: 0, totalTrips: 0, status: 'Inactive', createdAt: new Date('2023-08-20'), lastActivity: new Date('2023-08-20') },
    { id: 'ORG-005', name: 'Bagh Commuter Co.', area: 'Bagh', subdomain: 'bagh', adminName: 'Fatima Noor', totalDrivers: 10, totalVehicles: 8, totalTrips: 850, status: 'Active', createdAt: new Date('2023-04-12'), lastActivity: new Date() },
    { id: 'ORG-006', name: 'Bhimber Shuttle', area: 'Bhimber', subdomain: 'bhimber', adminName: 'Hassan Raza', totalDrivers: 2, totalVehicles: 2, totalTrips: 45, status: 'Inactive', createdAt: new Date('2023-05-18'), lastActivity: new Date(Date.now() - 35 * 24 * 60 * 60 * 1000) },
    { id: 'ORG-007', name: 'Neelum Valley Express', area: 'Neelum', subdomain: 'neelum', adminName: 'Sara Ali', totalDrivers: 5, totalVehicles: 2, totalTrips: 300, status: 'Active', createdAt: new Date('2023-06-25'), lastActivity: new Date() },
    { id: 'ORG-008', name: 'Poonch Highway Lines', area: 'Rawalakot', subdomain: 'poonch', adminName: 'Tariq Mehmood', totalDrivers: 0, totalVehicles: 0, totalTrips: 0, status: 'Active', createdAt: new Date('2023-09-01'), lastActivity: new Date() }
  ];

  // Helper for simulated network delay (300 - 800ms)
  private get delayed(): <T>(source: Observable<T>) => Observable<T> {
    return delay(Math.floor(Math.random() * 500) + 300);
  }

  getDashboardKPIs(): Observable<any> {
    return of({
      totalOrganizations: 8,
      activeOrganizations: 5,
      totalOrgAdmins: 7,
      tripsToday: 120,
      bookingsToday: 900,
      activeDrivers: 45,
      activeVehicles: 38,
      activeTripsNow: 12,
      systemHealth: 'Healthy'
    }).pipe(this.delayed);
  }

  getOrganizations(): Observable<Organization[]> {
    return of([...this.organizations]).pipe(this.delayed);
  }
}
