import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { BusService } from '../../services/bus.service';
import { RouteMapComponent } from '../route-map/route-map.component';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouteMapComponent],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard implements OnInit {
  searchForm = {
    source: '',
    destination: '',
    date: ''
  };
  searchResults: any[] = [];
  loading = false;
  error = '';
  popularLocations: any[] = [];
  recentSearches: any[] = [];
  minDate = '';

  constructor(
    private router: Router,
    private busService: BusService
  ) {}

  ngOnInit() {
    this.setDefaultDate();
    this.minDate = new Date().toISOString().split('T')[0];
    this.loadPopularLocations();
    this.loadRecentSearches();
  }

  private setDefaultDate() {
    const today = new Date();
    const tomorrow = new Date(today);
    tomorrow.setDate(today.getDate() + 1);
    this.searchForm.date = tomorrow.toISOString().split('T')[0];
  }

  private loadPopularLocations() {
    this.busService.getPopularLocations()
      .subscribe({
        next: (data) => {
          this.popularLocations = data.slice(0, 8);
        },
        error: (err) => {
          console.error('Failed to load popular locations:', err);
        }
      });
  }

  private loadRecentSearches() {
    if (typeof localStorage !== 'undefined') {
      const saved = localStorage.getItem('recentSearches');
      if (saved) {
        try {
          this.recentSearches = JSON.parse(saved).slice(0, 5);
        } catch {
          localStorage.removeItem('recentSearches');
        }
      }
    }
  }

  private saveSearch(source: string, destination: string) {
    if (typeof localStorage === 'undefined') return;
    
    const search = { source, destination, date: new Date().toISOString() };
    const existing = this.recentSearches.filter(s => 
      !(s.source === source && s.destination === destination)
    );
    this.recentSearches = [search, ...existing].slice(0, 5);
    localStorage.setItem('recentSearches', JSON.stringify(this.recentSearches));
  }

  validateSearchForm(): boolean {
    this.error = '';
    
    if (!this.searchForm.source.trim()) {
      this.error = 'Please enter a source location';
      return false;
    }
    
    if (!this.searchForm.destination.trim()) {
      this.error = 'Please enter a destination location';
      return false;
    }
    
    if (this.searchForm.source.toLowerCase() === this.searchForm.destination.toLowerCase()) {
      this.error = 'Source and destination cannot be the same';
      return false;
    }
    
    if (!this.searchForm.date) {
      this.error = 'Please select a travel date';
      return false;
    }
    
    const selectedDate = new Date(this.searchForm.date);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    
    if (selectedDate < today) {
      this.error = 'Travel date cannot be in the past';
      return false;
    }
    
    return true;
  }

  searchBuses() {
    if (!this.validateSearchForm()) return;

    this.loading = true;
    this.error = '';
    this.searchResults = [];

    this.busService.searchSchedules(
      this.searchForm.source,
      this.searchForm.destination,
      this.searchForm.date
    ).pipe(finalize(() => this.loading = false))
      .subscribe({
        next: (data) => {
          this.searchResults = data;
          this.saveSearch(this.searchForm.source, this.searchForm.destination);
          
          if (data.length === 0) {
            this.error = `No buses found from ${this.searchForm.source} to ${this.searchForm.destination} on ${new Date(this.searchForm.date).toLocaleDateString()}`;
          }
        },
        error: (err) => {
          console.error('Search error:', err);
          if (err.status === 0) {
            this.error = 'Unable to connect to server. Please check your internet connection.';
          } else if (err.status === 400) {
            this.error = err.error?.message || 'Invalid search parameters. Please try again.';
          } else if (err.status >= 500) {
            this.error = 'Server error. Please try again later.';
          } else {
            this.error = 'Failed to search buses. Please try again.';
          }
        }
      });
  }

  selectBus(schedule: any) {
    this.router.navigate(['/seat-selection', schedule.id], {
      queryParams: {
        basePrice: schedule.basePrice,
        from: schedule.route.source,
        to: schedule.route.destination,
        dep: schedule.departureTime
      }
    });
  }

  usePopularLocation(location: any, isSource: boolean) {
    if (isSource) {
      this.searchForm.source = location.name;
    } else {
      this.searchForm.destination = location.name;
    }
  }

  useRecentSearch(search: any) {
    this.searchForm.source = search.source;
    this.searchForm.destination = search.destination;
  }

  swapLocations() {
    const temp = this.searchForm.source;
    this.searchForm.source = this.searchForm.destination;
    this.searchForm.destination = temp;
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('en-US', {
      weekday: 'short',
      month: 'short',
      day: 'numeric'
    });
  }

  formatTime(dateString: string): string {
    return new Date(dateString).toLocaleTimeString('en-US', {
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  formatPrice(price: number): string {
    return `₹${price.toLocaleString('en-IN')}`;
  }

  clearRecentSearches() {
    this.recentSearches = [];
    if (typeof localStorage !== 'undefined') {
      localStorage.removeItem('recentSearches');
    }
  }
}
