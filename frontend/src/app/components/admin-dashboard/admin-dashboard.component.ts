import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BusService } from '../../services/bus.service';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.css'
})
export class AdminDashboardComponent implements OnInit {
  activeTab: 'operators' | 'buses' | 'revenue' | 'config' = 'revenue';
  operators: any[] = [];
  pendingBuses: any[] = [];
  revenue: any = null;
  convenienceFee = 50;
  newLocName = ''; newLocState = '';
  newRouteSrc = 0; newRouteDst = 0;
  locations: any[] = [];
  message = '';
  loading = false;

  constructor(private busService: BusService, private auth: AuthService, private router: Router) {}

  ngOnInit() {
    if (this.auth.currentUser?.role !== 'Admin') { this.router.navigate(['/']); return; }
    this.loadAll();
  }

  loadAll() {
    this.busService.getOperators().subscribe({ next: d => this.operators = d, error: () => {} });
    this.busService.getPendingBuses().subscribe({ next: d => this.pendingBuses = d, error: () => {} });
    this.busService.getRevenue().subscribe({ next: d => this.revenue = d, error: () => {} });
    this.busService['http'].get<any[]>('http://localhost:5047/api/locations').subscribe({ next: d => this.locations = d, error: () => {} });
  }

  approveOp(id: number) { this.busService.approveOperator(id).subscribe({ next: () => this.loadAll() }); }
  rejectOp(id: number) { this.busService.rejectOperator(id).subscribe({ next: () => this.loadAll() }); }
  toggleOp(id: number, enable: boolean) { this.busService.toggleOperator(id, enable).subscribe({ next: () => this.loadAll() }); }
  approveBus(id: number) { this.busService.approveBus(id).subscribe({ next: () => this.loadAll() }); }
  rejectBus(id: number) { this.busService.rejectBus(id).subscribe({ next: () => this.loadAll() }); }

  setFee() {
    this.busService.setConvenienceFee(this.convenienceFee).subscribe({
      next: (res: any) => this.message = res.message, error: () => {}
    });
  }

  addLocation() {
    this.busService['http'].post('http://localhost:5047/api/locations', { name: this.newLocName, state: this.newLocState }).subscribe({
      next: () => { this.message = 'Location added!'; this.newLocName = ''; this.newLocState = ''; this.loadAll(); }
    });
  }

  addRoute() {
    this.busService['http'].post('http://localhost:5047/api/routes', { sourceId: this.newRouteSrc, destinationId: this.newRouteDst }).subscribe({
      next: () => { this.message = 'Route created!'; this.newRouteSrc = 0; this.newRouteDst = 0; }
    });
  }
}
