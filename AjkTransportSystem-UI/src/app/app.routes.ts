import { Routes } from '@angular/router';
import { HomeComponent } from './components/home/home';
import { LoginComponent } from './components/login/login';
import { AdminLoginComponent } from './components/admin-login/admin-login';
import { SuperAdminLoginComponent } from './components/superadmin-login/superadmin-login';
import { AdminDashboardComponent } from './components/admin-dashboard/admin-dashboard';
import { SuperAdminDashboardComponent } from './components/superadmin-dashboard/superadmin-dashboard';
import { PassengerDashboardComponent } from './components/passenger-dashboard/passenger-dashboard';
import { BookingFormComponent } from './components/booking-form/booking-form';
import { MyBookingsComponent } from './components/my-bookings/my-bookings';
import { AuthGuardService } from './services/auth-guard';
import { DriverDashboardComponent } from './components/driver-dashboard/driver-dashboard';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'login', component: LoginComponent },
  { path: 'admin-login', component: AdminLoginComponent },
  { path: 'superadmin-login', component: SuperAdminLoginComponent },
  { path: 'register', component: LoginComponent },
  { 
    path: 'superadmin', 
    component: SuperAdminDashboardComponent,
    canActivate: [AuthGuardService],
    data: { roles: ['SuperAdmin'] }
  },
  { 
    path: 'superadmin/:section', 
    component: SuperAdminDashboardComponent,
    canActivate: [AuthGuardService],
    data: { roles: ['SuperAdmin'] }
  },
  { 
    path: 'superadmin/:section/:subsection', 
    component: SuperAdminDashboardComponent,
    canActivate: [AuthGuardService],
    data: { roles: ['SuperAdmin'] }
  },
  { 
    path: 'admin', 
    component: AdminDashboardComponent,
    canActivate: [AuthGuardService],
    data: { roles: ['Admin', 'TransportAdmin'] }
  },
  {
    path: 'driver',
    component: DriverDashboardComponent,
    canActivate: [AuthGuardService],
    data: { roles: ['Driver'] }
  },
  { 
    path: 'passenger', 
    component: PassengerDashboardComponent,
    canActivate: [AuthGuardService],
    data: { roles: ['Passenger'] }
  },
  { 
    path: 'book/:id', 
    component: BookingFormComponent,
    canActivate: [AuthGuardService],
    data: { roles: ['Passenger'] }
  },
  { 
    path: 'my-bookings', 
    component: MyBookingsComponent,
    canActivate: [AuthGuardService],
    data: { roles: ['Passenger'] }
  },
  { path: '**', redirectTo: '' }
];
