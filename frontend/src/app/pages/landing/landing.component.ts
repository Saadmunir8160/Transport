import { Component } from '@angular/core';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.css'
})
export class LandingComponent {
  login() {
    console.log('Navigate to login');
  }

  searchTrips() {
    console.log('Navigate to trip search');
  }
}
