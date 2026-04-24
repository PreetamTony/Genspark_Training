import { Component, OnInit, ChangeDetectorRef } from '@angular/core';

import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { BusService } from '../../services/bus.service';
import { RouteMapComponent } from '../route-map/route-map.component';
import { BusDetailsComponent } from '../bus-details/bus-details.component';
import { debounceTime, distinctUntilChanged, Subject, switchMap, of } from 'rxjs';

@Component({
  selector: 'app-search-results',
  standalone: true,
  imports: [CommonModule, FormsModule, RouteMapComponent, BusDetailsComponent],
  templateUrl: './search-results.component.html',
  styleUrl: './search-results.component.css'
})
export class SearchResultsComponent implements OnInit {
  source = ''; destination = ''; date = '';
  results: any[] = [];
  loading = false;
  error = '';
  sortBy = 'price';
  selectedBus: any = null;

  constructor(
    private route: ActivatedRoute, 
    private router: Router, 
    private busService: BusService,
    private cdr: ChangeDetectorRef
  ) {}


  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.source = params['source'] || '';
      this.destination = params['destination'] || '';
      this.date = params['date'] || '';
      if (this.source && this.destination && this.date) this.search();
    });
  }

  search() {
    this.loading = true; this.error = ''; this.results = [];
    console.log('Starting search for:', this.source, this.destination, this.date);

    // Failsafe: stop loading after 10s if no response
    const timeout = setTimeout(() => {
      if (this.loading) {
        this.loading = false;
        this.error = 'Search timed out. Please try again.';
      }
    }, 10000);

    this.busService.searchSchedules(this.source, this.destination, this.date).subscribe({
      next: (data) => { 
        clearTimeout(timeout);
        console.log('Search data received:', data);
        try {
          this.results = data || []; 
          console.log('Search results set:', this.results.length);
          this.sortResults(); 
        } catch (e) {
          console.error('Error processing search results:', e);
          this.error = 'An error occurred while processing results.';
        } finally {
          this.loading = false; 
          this.cdr.detectChanges(); // Force UI update
        }
      },
      error: (err) => { 
        clearTimeout(timeout);
        console.error('Search API error:', err);
        this.loading = false;
        if (err.status === 0) {
          this.error = 'Unable to connect to server. Please check your internet connection and try again.';
        } else if (err.status === 400) {
          this.error = 'Invalid search parameters. Please check your source, destination, and date.';
        } else if (err.status >= 500) {
          this.error = 'Server error. Please try again later.';
        } else {
          this.error = err.error?.message || 'Failed to fetch buses. Please try again.'; 
        }
        this.cdr.detectChanges(); // Force UI update
      }

    });
  }



  sortResults() {
    if (this.sortBy === 'price') this.results.sort((a, b) => a.basePrice - b.basePrice);
    else if (this.sortBy === 'seats') this.results.sort((a, b) => b.availableSeats - a.availableSeats);
    else if (this.sortBy === 'departure') this.results.sort((a, b) => new Date(a.departureTime).getTime() - new Date(b.departureTime).getTime());
  }

  onSortChange() { this.sortResults(); }

  showBusDetails(schedule: any) {
    this.selectedBus = schedule;
  }

  hideBusDetails() {
    this.selectedBus = null;
  }

  selectBus(schedule: any) {
    this.router.navigate(['/seat-selection', schedule.id], {
      queryParams: { basePrice: schedule.basePrice, from: this.source, to: this.destination, dep: schedule.departureTime }
    });
  }

  getDuration(dep: string, arr: string): string {
    const diff = (new Date(arr).getTime() - new Date(dep).getTime()) / 60000;
    return `${Math.floor(diff / 60)}h ${diff % 60}m`;
  }

  formatTime(dt: string): string {
    return new Date(dt).toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit' });
  }

  goBack() { this.router.navigate(['/']); }
}
