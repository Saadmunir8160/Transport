import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouteService } from '../../services/route';

@Component({
  selector: 'app-routes',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './routes.html',
  styleUrls: ['./routes.css']
})
export class RoutesComponent implements OnInit {
  routes: any[] = [];
  routeName = '';
  startPoint = '';
  endPoint = '';
  distance = 0;

  constructor(private routeService: RouteService) {}

  ngOnInit() {
    this.loadRoutes();
  }

  loadRoutes() {
    this.routeService.getAll().subscribe(data => this.routes = data);
  }

  addRoute() {
    this.routeService.create({ routeName: this.routeName, startPoint: this.startPoint, endPoint: this.endPoint, distance: this.distance })
      .subscribe(() => {
        this.loadRoutes();
        this.routeName = '';
        this.startPoint = '';
        this.endPoint = '';
        this.distance = 0;
      });
  }

  deleteRoute(id: number) {
    this.routeService.delete(id).subscribe(() => this.loadRoutes());
  }
}
