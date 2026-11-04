import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { VehicleService } from '../../services/vehicle';

@Component({
  selector: 'app-vehicles',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './vehicles.html',
  styleUrls: ['./vehicles.css']
})
export class VehiclesComponent implements OnInit {
  vehicles: any[] = [];
  vehicleNumber = '';
  vehicleType = '';
  capacity = 0;
  status = 'Active';

  constructor(private vehicleService: VehicleService) {}

  ngOnInit() {
    this.loadVehicles();
  }

  loadVehicles() {
    this.vehicleService.getAll().subscribe(data => this.vehicles = data);
  }

  addVehicle() {
    this.vehicleService.create({ vehicleNumber: this.vehicleNumber, vehicleType: this.vehicleType, capacity: this.capacity, status: this.status })
      .subscribe(() => {
        this.loadVehicles();
        this.vehicleNumber = '';
        this.vehicleType = '';
        this.capacity = 0;
      });
  }

  deleteVehicle(id: number) {
    this.vehicleService.delete(id).subscribe(() => this.loadVehicles());
  }
}
