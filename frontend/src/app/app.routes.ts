import { Routes } from '@angular/router';
import { LandingComponent } from './pages/landing/landing.component';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { AdminLayoutComponent } from './shared/components/admin-layout/admin-layout.component';
import { DashboardComponent } from './features/superadmin/dashboard/dashboard.component';
import { OrganizationListComponent } from './features/superadmin/organizations/organization-list/organization-list.component';
import { OrganizationFormComponent } from './features/superadmin/organizations/organization-form/organization-form.component';
import { roleGuard } from './core/guards/role.guard';
import { SearchComponent } from './features/passenger/search/search.component';
import { TripListComponent } from './features/passenger/trip-list/trip-list.component';
import { SeatMapComponent } from './features/passenger/seat-map/seat-map.component';
import { CheckoutComponent } from './features/passenger/checkout/checkout.component';
import { BookingHistoryComponent } from './features/passenger/booking-history/booking-history.component';

export const routes: Routes = [
  { path: '', component: LandingComponent },
  { path: 'auth/login', component: LoginComponent },
  { path: 'auth/register', component: RegisterComponent },
  { path: 'search', component: SearchComponent },
  { path: 'trip-list', component: TripListComponent },
  { path: 'seat-map/:id', component: SeatMapComponent },
  { path: 'checkout', component: CheckoutComponent, canActivate: [roleGuard], data: { roles: ['Passenger'] } },
  { path: 'my-bookings', component: BookingHistoryComponent, canActivate: [roleGuard], data: { roles: ['Passenger'] } },
  { 
    path: 'driver', 
    loadComponent: () => import('./pages/driver-dashboard/driver-dashboard.component').then(m => m.DriverDashboardComponent),
    canActivate: [roleGuard], 
    data: { roles: ['Driver'] } 
  },
  {
    path: 'superadmin',
    component: AdminLayoutComponent,
    canActivate: [roleGuard],
    data: { roles: ['SuperAdmin'] },
    children: [
      { path: 'dashboard', component: DashboardComponent },
      { path: 'organizations', component: OrganizationListComponent },
      { path: 'organizations/new', component: OrganizationFormComponent },
      { path: 'organizations/:id', component: OrganizationFormComponent },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  },
  {
    path: 'transportadmin',
    component: AdminLayoutComponent,
    canActivate: [roleGuard],
    data: { roles: ['TransportAdmin'] },
    children: [
      { path: 'dashboard', loadComponent: () => import('./features/transportadmin/dashboard/dashboard.component').then(m => m.DashboardComponent) },
      { path: 'drivers', loadComponent: () => import('./features/transportadmin/drivers/drivers.component').then(m => m.DriversComponent) },
      { path: 'vehicles', loadComponent: () => import('./features/transportadmin/vehicles/vehicles.component').then(m => m.VehiclesComponent) },
      { path: 'routes', loadComponent: () => import('./features/transportadmin/routes/routes.component').then(m => m.RoutesComponent) },
      { path: 'trips', loadComponent: () => import('./features/transportadmin/trips/trips.component').then(m => m.TripsComponent) },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  },
  { path: '**', redirectTo: '' }
];
