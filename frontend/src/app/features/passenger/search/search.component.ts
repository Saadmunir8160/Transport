import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { PassengerService } from '../../../core/services/passenger.service';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-search',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="search-container">
      <h2>Search for a Trip</h2>
      <div class="form-group">
        <label>Origin Stop:</label>
        <select [(ngModel)]="originId" class="form-control">
          <option *ngFor="let stop of stops" [value]="stop.id">{{ stop.name }}</option>
        </select>
      </div>
      <div class="form-group">
        <label>Destination Stop:</label>
        <select [(ngModel)]="destinationId" class="form-control">
          <option *ngFor="let stop of stops" [value]="stop.id">{{ stop.name }}</option>
        </select>
      </div>
      <div class="form-group">
        <label>Travel Date:</label>
        <input type="date" [(ngModel)]="date" class="form-control" />
      </div>
      <button class="btn btn-primary" (click)="search()">Search</button>
    </div>
  `,
  styles: [`
    .search-container { max-width: 500px; margin: 2rem auto; padding: 2rem; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); background: white; }
    .form-group { margin-bottom: 1rem; }
    .form-control { width: 100%; padding: 0.5rem; border: 1px solid #ccc; border-radius: 4px; }
    .btn { padding: 0.5rem 1rem; background: #007bff; color: white; border: none; border-radius: 4px; cursor: pointer; }
    .btn:hover { background: #0056b3; }
  `]
})
export class SearchComponent implements OnInit {
  stops: any[] = [];
  originId: string = '';
  destinationId: string = '';
  date: string = new Date().toISOString().split('T')[0];

  constructor(private passengerService: PassengerService, private http: HttpClient, private router: Router) {}

  ngOnInit() {
    this.http.get<any[]>('https://localhost:7194/api/stops').subscribe((res: any[]) => {
      this.stops = res;
    });
  }

  search() {
    if (this.originId && this.destinationId && this.date) {
      this.router.navigate(['/trip-list'], {
        queryParams: { originId: this.originId, destinationId: this.destinationId, date: this.date }
      });
    }
  }
}
