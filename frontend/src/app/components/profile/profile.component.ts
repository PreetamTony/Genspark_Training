import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BusService } from '../../services/bus.service';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  user: any = null;
  bookings: any[] = [];
  loading = true;
  activeTab: 'upcoming' | 'past' | 'cancelled' = 'upcoming';
  cancellingId: number | null = null;
  message = '';

  constructor(private auth: AuthService, private busService: BusService, private router: Router) {}

  ngOnInit() {
    this.user = this.auth.currentUser;
    if (!this.user) { this.router.navigate(['/login']); return; }
    this.loadBookings();
  }

  loadBookings() {
    this.loading = true;
    this.busService.getMyBookings().subscribe({
      next: (data) => { this.bookings = data; this.loading = false; },
      error: (err) => {
        console.error('Load bookings error:', err);
        this.loading = false;
        if (err.status === 0) {
          this.message = 'Unable to connect to server. Please check your internet connection.';
        } else if (err.status === 401) {
          this.message = 'Session expired. Please login again.';
          this.auth.logout();
          this.router.navigate(['/login']);
        } else if (err.status >= 500) {
          this.message = 'Server error. Please try again later.';
        } else {
          this.message = 'Failed to load bookings. Please try again.';
        }
      }
    });
  }

  get upcoming() { return this.bookings.filter(b => b.status === 'Confirmed' && new Date(b.departureTime) > new Date()); }
  get past() { return this.bookings.filter(b => b.status === 'Confirmed' && new Date(b.departureTime) <= new Date()); }
  get cancelled() { return this.bookings.filter(b => b.status === 'Cancelled'); }

  cancelBooking(id: number) {
    if (!confirm('Are you sure you want to cancel this booking? Cancellation charges may apply.')) {
      return;
    }

    this.cancellingId = id;
    this.busService.cancelBooking(id).subscribe({
      next: (res) => {
        this.message = `Booking cancelled successfully! Refund: ₹${res.refundAmount}`;
        this.cancellingId = null;
        this.loadBookings();
        setTimeout(() => this.message = '', 5000);
      },
      error: (err) => {
        console.error('Cancel booking error:', err);
        this.cancellingId = null;
        if (err.status === 0) {
          this.message = 'Connection lost. Please check your internet.';
        } else if (err.status === 400) {
          this.message = err.error?.message || 'Unable to cancel booking.';
        } else if (err.status === 401) {
          this.message = 'Session expired. Please login again.';
          this.auth.logout();
          this.router.navigate(['/login']);
        } else if (err.status === 403) {
          this.message = 'You do not have permission to cancel this booking.';
        } else if (err.status >= 500) {
          this.message = 'Server error. Please try again.';
        } else {
          this.message = err.error?.message || 'Cancellation failed.';
        }
      }
    });
  }

  logout() { this.auth.logout(); this.router.navigate(['/']); }
}
