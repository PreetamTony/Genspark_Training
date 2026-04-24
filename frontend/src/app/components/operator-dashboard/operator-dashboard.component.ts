import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BusService } from '../../services/bus.service';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-operator-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './operator-dashboard.component.html',
  styleUrl: './operator-dashboard.component.css'
})
export class OperatorDashboardComponent implements OnInit {
  user: any = null;
  activeTab: 'buses' | 'schedules' | 'add-bus' | 'add-schedule' | 'coupons' | 'add-coupon' = 'buses';

  buses: any[] = []; layouts: any[] = []; schedules: any[] = []; routes: any[] = []; coupons: any[] = [];
  loading = false; message = '';

  busForm = { registrationNumber: '', layoutId: 0 };
  scheduleForm = { busId: 0, routeId: 0, departureTime: '', arrivalTime: '', basePrice: 0, pickupPoint: '', dropPoint: '' };
  couponForm = { code: '', discountType: 'amount', discountAmount: 0, discountPercent: 0, validFrom: '', validTo: '', isActive: true };
  editingCoupon: any = null;

  constructor(private busService: BusService, private auth: AuthService, private router: Router) {}

  ngOnInit() {
    this.user = this.auth.currentUser;
    if (!this.user || this.user.role !== 'Operator') { this.router.navigate(['/']); return; }
    this.loadAll();
  }

  loadAll() {
    this.busService.getMyBuses().subscribe({ next: d => this.buses = d, error: () => {} });
    this.busService.getLayouts().subscribe({ next: d => this.layouts = d, error: () => {} });
    this.busService.getMySchedules().subscribe({ next: d => this.schedules = d, error: () => {} });
    this.busService.getRoutes().subscribe({ next: d => this.routes = d, error: () => {} });
    this.loadCoupons();
  }

  loadCoupons() {
    this.busService.getOperatorCoupons().subscribe({
      next: d => this.coupons = d,
      error: (err) => {
        console.error('Failed to load coupons:', err);
        this.coupons = [];
      }
    });
  }

  submitCoupon() {
    console.log('=== SUBMITTING COUPON ===');
    console.log('User:', this.user);
    console.log('Auth Token:', this.auth.token);
    console.log('User Role:', this.user?.role);
    console.log('Operator Profile ID:', this.user?.operatorProfileId);
    console.log('Coupon Form Data:', this.couponForm);
    console.log('Editing Coupon:', this.editingCoupon);
    
    if (!this.auth.token) {
      console.error('No authentication token found');
      this.message = 'Authentication error. Please login again.';
      return;
    }
    
    if (this.user?.role !== 'Operator') {
      console.error('User is not an operator:', this.user?.role);
      this.message = 'Access denied. Operator account required.';
      return;
    }
    
    this.loading = true;
    const couponData: any = { ...this.couponForm };
    
    // Handle discount type
    if (couponData.discountType === 'amount') {
      couponData.discountPercent = null; // Clear percent for amount type
    } else {
      couponData.discountAmount = 0; // Clear amount for percent type
    }
    
    // Remove discountType as it's not needed in backend
    delete couponData.discountType;
    
    // Format dates properly - send null for empty dates
    if (couponData.validFrom && couponData.validFrom.trim() !== '') {
      couponData.validFrom = new Date(couponData.validFrom).toISOString();
    } else {
      couponData.validFrom = null;
    }
    if (couponData.validTo && couponData.validTo.trim() !== '') {
      couponData.validTo = new Date(couponData.validTo).toISOString();
    } else {
      couponData.validTo = null;
    }
    
    console.log('Formatted Coupon Data:', couponData);
    
    // Wrap in couponDto object as expected by backend
    const requestBody = { couponDto: couponData };
    console.log('Request Body:', requestBody);
    
    if (this.editingCoupon) {
      // Update existing coupon
      console.log('Updating existing coupon ID:', this.editingCoupon.id);
      this.busService.updateCoupon(this.editingCoupon.id, couponData).subscribe({
        next: (res) => {
          console.log('Coupon updated successfully:', res);
          this.message = res.message;
          this.loadAll();
          this.activeTab = 'coupons';
          this.resetCouponForm();
          this.loading = false;
        },
        error: (err) => {
          console.error('Update coupon error:', err);
          console.error('Error status:', err.status);
          console.error('Error message:', err.error?.message);
          this.message = err.error?.message || 'Failed to update coupon.';
          this.loading = false;
        }
      });
    } else {
      // Create new coupon
      console.log('Creating new coupon...');
      this.busService.createCoupon(requestBody).subscribe({
        next: (res: any) => {
          console.log('=== COUPON CREATION SUCCESS ===');
          console.log('Response:', res);
          console.log('Response message:', res.message);
          console.log('Setting message to:', res.message);
          
          this.message = res.message;
          console.log('Message set, calling loadAll...');
          
          this.loadAll();
          console.log('loadAll called, switching to coupons tab...');
          
          this.activeTab = 'coupons';
          console.log('Tab switched, resetting form...');
          
          this.resetCouponForm();
          console.log('Form reset, setting loading to false...');
          
          this.loading = false;
          console.log('=== COUPON CREATION COMPLETE ===');
        },
        error: (err) => {
          console.error('=== CREATE COUPON ERROR DEBUG ===');
          console.error('Full error object:', err);
          console.error('Error status:', err.status);
          console.error('Error statusText:', err.statusText);
          console.error('Error error:', err.error);
          console.error('Error message:', err.message);
          console.error('Error error?.message:', err.error?.message);
          console.error('Error error?.errors:', err.error?.errors);
          
          // Try different ways to extract the error message
          let errorMessage = 'Failed to create coupon.';
          if (err.error?.message) {
            errorMessage = err.error.message;
          } else if (err.error?.errors) {
            errorMessage = Object.values(err.error.errors).join(', ');
          } else if (err.message) {
            errorMessage = err.message;
          } else if (err.statusText) {
            errorMessage = err.statusText;
          }
          
          console.error('Final error message:', errorMessage);
          this.message = errorMessage;
          this.loading = false;
        }
      });
    }
  }

  editCoupon(coupon: any) {
    this.editingCoupon = coupon;
    this.couponForm = {
      code: coupon.code,
      discountType: coupon.discountPercent > 0 ? 'percent' : 'amount',
      discountAmount: coupon.discountAmount,
      discountPercent: coupon.discountPercent || 0,
      validFrom: coupon.validFrom ? new Date(coupon.validFrom).toISOString().slice(0, 16) : '',
      validTo: coupon.validTo ? new Date(coupon.validTo).toISOString().slice(0, 16) : '',
      isActive: coupon.isActive
    };
    this.activeTab = 'add-coupon';
  }

  deleteCoupon(coupon: any) {
    if (!confirm(`Are you sure you want to delete coupon "${coupon.code}"?`)) return;
    
    this.loading = true;
    this.busService.deleteCoupon(coupon.id).subscribe({
      next: (res) => {
        this.message = res.message;
        this.loadCoupons();
        this.loading = false;
      },
      error: (err) => {
        console.error('Delete coupon error:', err);
        this.message = err.error?.message || 'Failed to delete coupon.';
        this.loading = false;
      }
    });
  }

  resetCouponForm() {
    this.couponForm = { code: '', discountType: 'amount', discountAmount: 0, discountPercent: 0, validFrom: '', validTo: '', isActive: true };
    this.editingCoupon = null;
  }

  submitBusRequest() {
    this.loading = true;
    this.busService.requestBus(this.busForm).subscribe({
      next: (res) => { this.message = res.message; this.loadAll(); this.activeTab = 'buses'; this.loading = false; },
      error: (err) => { this.message = err.error?.message || 'Failed.'; this.loading = false; }
    });
  }

  submitSchedule() {
    this.loading = true;
    this.busService.createSchedule(this.scheduleForm).subscribe({
      next: (res) => { this.message = res.message; this.loadAll(); this.activeTab = 'schedules'; this.loading = false; },
      error: (err) => { this.message = err.error?.message || 'Failed.'; this.loading = false; }
    });
  }

  updateBusStatus(id: number, status: string) {
    this.busService['http'].put(`http://localhost:5047/api/buses/${id}/status`, { status }).subscribe({
      next: () => this.loadAll(), error: () => {}
    });
  }
}
