import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { BusService } from '../../services/bus.service';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.css'
})
export class LandingComponent implements OnInit {
  searchQuery = {
    source: '',
    destination: '',
    date: this.getNextDay()
  };

  popularLocations: any[] = [];

  constructor(private router: Router, private busService: BusService) {}

  ngOnInit() {
    this.busService.getPopularLocations().subscribe({ next: (data) => this.popularLocations = data, error: () => {} });
  }

  getNextDay(): string {
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    return tomorrow.toISOString().split('T')[0];
  }

  onSearch() {
    // Ensure date is in YYYY-MM-DD format if it's somehow became a Date object
    const dateStr = typeof this.searchQuery.date === 'string' 
      ? this.searchQuery.date 
      : (this.searchQuery.date as any).toISOString().split('T')[0];
    
    this.router.navigate(['/search-results'], { 
      queryParams: { ...this.searchQuery, date: dateStr } 
    });
  }


  swapLocations() {
    const temp = this.searchQuery.source;
    this.searchQuery.source = this.searchQuery.destination;
    this.searchQuery.destination = temp;
  }
}
