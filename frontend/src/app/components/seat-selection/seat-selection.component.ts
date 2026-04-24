  import { Component, OnInit, Inject, PLATFORM_ID, ChangeDetectorRef } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { BusService } from '../../services/bus.service';
import { AuthService } from '../../services/auth.service';
import { RouteMapComponent } from '../route-map/route-map.component';
import { BusDetailsComponent } from '../bus-details/bus-details.component';
import { forkJoin, finalize, of } from 'rxjs';

@Component({
  selector: 'app-seat-selection',
  standalone: true,
  imports: [CommonModule, FormsModule, RouteMapComponent, BusDetailsComponent],
  templateUrl: './seat-selection.component.html',
  styleUrl: './seat-selection.component.css'
})
export class SeatSelectionComponent implements OnInit {
  scheduleId = 0;
  basePrice = 0;
  convenienceFee = 50;
  from = ''; to = ''; dep = '';

  seatData: any = null;
  busDetails: any = null;
  bookedSeatIds = new Set<number>();
  selectedSeats: any[] = [];
  loading = true;
  locking = false;
  error = '';

  // Coupon properties
  couponCode: string = '';
  discount: number = 0;
  couponMessage: string = '';
  validatingCoupon = false;

  // Passenger details for gender-based seat visualization
  passengerDetails: { [key: number]: { name: string; age: number; gender: string } } = {};
  showPassengerForm = false;
  currentSeatForPassenger: any = null;

  
  private get storageKey() {
    return `seat-selection-${this.scheduleId}`;
  }

  validateCoupon() {
    if (!this.couponCode.trim()) {
      this.discount = 0;
      this.couponMessage = '';
      return;
    }

    this.validatingCoupon = true;
    this.couponMessage = '';

    // Add test coupon logic for demo purposes
    if (this.couponCode.trim().toUpperCase() === 'SAVE20') {
      setTimeout(() => {
        this.discount = Math.round(this.totalAmount * 0.2); // 20% discount
        this.couponMessage = 'Coupon applied successfully! You saved 20%';
        this.validatingCoupon = false;
      }, 1000);
      return;
    }

    if (this.couponCode.trim().toUpperCase() === 'SAVE10') {
      setTimeout(() => {
        this.discount = Math.round(this.totalAmount * 0.1); // 10% discount
        this.couponMessage = 'Coupon applied successfully! You saved 10%';
        this.validatingCoupon = false;
      }, 1000);
      return;
    }

    if (this.couponCode.trim().toUpperCase() === 'SUMMER') {
      setTimeout(() => {
        this.discount = Math.round(this.totalAmount * 0.15); // 15% discount
        this.couponMessage = 'Coupon applied successfully! You saved 15%';
        this.validatingCoupon = false;
      }, 1000);
      return;
    }

    this.busService.validateCoupon(this.couponCode.trim(), this.scheduleId)
      .pipe(finalize(() => this.validatingCoupon = false))
      .subscribe({
        next: (response) => {
          if (response.valid) {
            this.discount = response.discount || 0;
            this.couponMessage = response.message || 'Coupon applied successfully!';
          } else {
            this.discount = 0;
            this.couponMessage = response.message || 'Invalid coupon code';
          }
        },
        error: (err) => {
          this.discount = 0;
          this.couponMessage = 'Failed to validate coupon. Please try again.';
          console.error('Coupon validation error:', err);
        }
      });
  }

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private busService: BusService,
    private auth: AuthService,
    private cdr: ChangeDetectorRef,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {}

  ngOnInit() {
    this.scheduleId = +this.route.snapshot.paramMap.get('scheduleId')!;
    this.route.queryParams.subscribe(p => {
      this.basePrice = +p['basePrice'] || 0;
      this.from = p['from'] || ''; 
      this.to = p['to'] || ''; 
      this.dep = p['dep'] || '';
    });

    // Only load seats in the browser (not during SSR)
    if (isPlatformBrowser(this.platformId)) {
      console.log('Component initialized, loading seats...');
      this.loadSeats();
    } else {
      this.loading = false;
    }
  }

  private restoreSelection() {
    if (!isPlatformBrowser(this.platformId)) return;
    const saved = localStorage.getItem(this.storageKey);
    if (!saved) return;

    try {
      const ids = JSON.parse(saved) as number[];
      if (!Array.isArray(ids) || ids.length === 0) return;

      this.selectedSeats = this.seatData?.seats?.filter((seat: any) => ids.includes(seat.id) && !seat.isBooked) || [];
      const unavailable = ids.filter((id) => !this.selectedSeats.some((seat: any) => seat.id === id));
      if (unavailable.length) {
        this.error = 'Some saved seats are no longer available and have been removed.';
        this.saveSelection();
      }
    } catch {
      localStorage.removeItem(this.storageKey);
    }
  }

  private saveSelection() {
    if (!isPlatformBrowser(this.platformId)) return;
    localStorage.setItem(this.storageKey, JSON.stringify(this.selectedSeats.map(s => s.id)));
  }

  private clearSavedSelection() {
    if (!isPlatformBrowser(this.platformId)) return;
    localStorage.removeItem(this.storageKey);
  }

  loadSeats() {
    this.loading = true;
    this.error = '';
    console.log('Loading seats for schedule:', this.scheduleId);

    // Load seat data only for now
    this.busService.getSeatAvailability(this.scheduleId).subscribe({
      next: (seatData) => {
        console.log('Seat data received:', seatData);
        this.loading = false;
        
        if (!seatData || !seatData.seats || !Array.isArray(seatData.seats)) {
          this.error = 'No seat information available.';
          return;
        }

        // Set seat data
        this.seatData = seatData;
        this.bookedSeatIds = new Set(seatData.seats.filter((s: any) => s.isBooked).map((s: any) => s.id));
        
        // Set basic bus details (we'll enhance this later)
        this.busDetails = {
          features: {
            hasWaterBottle: true,
            hasBlankets: true,
            hasChargingPoint: true,
            hasCCTV: true,
            hasToilet: false,
            hasWiFi: true,
            hasReadingLight: true,
            hasEmergencyExit: true,
            hasGPS: true
          },
          rating: 4.9,
          totalRatings: 1116,
          onTimeTrips: 940,
          totalTrips: 950,
          policies: {
            cancellationPolicy: "Before 25th Apr 11:10 AM - 85%; From 25th Apr 11:10 AM Until 25th Apr 03:10 PM - 70%; From 25th Apr 03:10 PM Until 25th Apr 07:10 PM - 40%; From 25th Apr 07:10 PM Until 25th Apr 11:10 PM - 5%",
            reschedulePolicy: "Before 25th Apr 04:10 PM - FREE",
            childPolicy: "Children above the age of 3 will need a ticket",
            luggagePolicy: "1 pieces of luggage will be accepted free of charge per passenger. Excess items will be chargeable",
            petPolicy: "Pets are not allowed",
            toiletPolicy: "In Bus Toilet with Only Urinal Facility",
            liquorPolicy: "Carrying or consuming liquor inside the bus is prohibited. Bus operator reserves the right to deboard drunk passengers."
          }
        };
        
        console.log('Bus details set:', this.busDetails);
        console.log('Booked seats:', this.bookedSeatIds);

        // Create basic configuration if missing
        if (!seatData.seatConfiguration || !Array.isArray(seatData.seatConfiguration)) {
          console.warn('No seat configuration found, creating basic layout');
          seatData.seatConfiguration = this.createBasicSeatConfiguration(seatData.seats);
        }

        this.seatData = seatData;
        
        console.log('Seats loaded successfully:', {
          totalSeats: seatData.seats.length,
          bookedSeats: this.bookedSeatIds.size,
          rows: this.rows.length
        });
        
        // Force change detection to ensure UI updates
        this.cdr.detectChanges();
        
        this.restoreSelection();
      },
      error: (err) => {
        console.error('Failed to load seats:', err);
        this.loading = false;
        this.error = 'Failed to load seats. Please try again.';
      }
    });
  }

  private createBasicSeatConfiguration(seats: any[]): any[] {
    const config: any[] = [];
    const rows = new Map<number, any[]>();
    
    // Group seats by row number
    seats.forEach(seat => {
      const row = Math.ceil(seat.seatNumber.replace(/[^0-9]/g, '') / 4); // Assuming 4 seats per row
      if (!rows.has(row)) rows.set(row, []);
      rows.get(row)!.push(seat);
    });

    // Create configuration
    let colIndex = 0;
    rows.forEach((seatsInRow, rowNum) => {
      seatsInRow.forEach((seat, index) => {
        config.push({
          row: rowNum,
          col: String.fromCharCode(65 + index), // A, B, C, D...
          label: seat.seatNumber
        });
      });
    });

    return config;
  }

  // Enhanced toggleSeat method with passenger details
  toggleSeat(seat: any) {
    if (!seat || this.bookedSeatIds.has(seat.id) || this.locking) return;

    if (this.selectedSeats.some(s => s.id === seat.id)) {
      // Remove seat and passenger details
      this.selectedSeats = this.selectedSeats.filter(s => s.id !== seat.id);
      this.removePassengerDetails(seat.id);
    } else {
      // Add seat and show passenger form
      this.selectedSeats.push(seat);
      this.showPassengerDetailsForm(seat);
    }
    this.saveSelection();
  }

  
  private getErrorMessage(err: any, defaultMessage: string): string {
    if (err.status === 0) {
      return 'Connection lost. Please check your internet.';
    } else if (err.status === 400) {
      return err.error?.message || defaultMessage;
    } else if (err.status === 401) {
      this.auth.logout();
      this.router.navigate(['/login'], { state: { redirectTo: this.router.url } });
      return 'Session expired. Please login again.';
    } else if (err.status >= 500) {
      return 'Server error. Please try again.';
    } else {
      return err.error?.message || defaultMessage;
    }
  }

  get totalAmount(): number {
    return this.selectedSeats.length * this.basePrice + this.selectedSeats.length * this.convenienceFee;
  }

  
  private lockSelectedSeats() {
    const lockRequests = this.selectedSeats.map(seat => this.busService.lockSeat(this.scheduleId, seat.id));
    return lockRequests.length ? forkJoin(lockRequests) : of([]);
  }

  proceedToCheckout() {
    console.log('=== PROCEED TO CHECKOUT CALLED ===');
    console.log('Selected seats:', this.selectedSeats);
    console.log('Selected seats count:', this.selectedSeats.length);
    console.log('Is logged in:', this.auth.isLoggedIn);
    console.log('Schedule ID:', this.scheduleId);
    console.log('Coupon code:', this.couponCode);

    if (this.selectedSeats.length === 0) {
      this.error = 'Please select at least one seat to proceed.';
      console.log('No seats selected, returning');
      return;
    }

    if (!this.auth.isLoggedIn) {
      console.log('User not logged in, redirecting to login');
      this.saveSelection();
      this.router.navigate(['/login'], { state: { redirectTo: `/seat-selection/${this.scheduleId}?basePrice=${this.basePrice}&from=${this.from}&to=${this.to}&dep=${this.dep}` } });
      return;
    }

    this.locking = true;
    this.error = '';
    
    // First, lock all selected seats
    console.log('Locking selected seats before booking...');
    this.lockSelectedSeats()
      .pipe(finalize(() => this.locking = false))
      .subscribe({
        next: () => {
          console.log('All seats locked successfully, creating booking...');
          const seatIds = this.selectedSeats.map(s => s.id);
          
          // Create booking after all seats are locked
          this.busService.createBookingWithCoupon(this.scheduleId, seatIds, this.couponCode).subscribe({
            next: (res: any) => {
              console.log('Booking created successfully:', res);
              this.clearSavedSelection();
              this.discount = res.discount || 0;
              this.couponMessage = res.couponMessage || '';
              this.locking = false;
              
              const bookingId = res.bookingId || res.id;
              console.log('Navigating to payment with booking ID:', bookingId);
              
              // Reload seat data to update availability after booking
              console.log('Reloading seat data to update availability...');
              this.loadSeats();
              
              this.router.navigate(['/payment', bookingId], { 
                state: { 
                  booking: res, 
                  from: this.from, 
                  to: this.to, 
                  dep: this.dep,
                  passengerDetails: this.passengerDetails,
                  selectedSeats: this.selectedSeats
                } 
              });
            },
            error: (err: any) => {
              console.error('Create booking error:', err);
              console.error('Error status:', err.status);
              console.error('Error message:', err.error?.message);
              this.locking = false;
              if (err.status === 0) {
                this.error = 'Connection lost. Please check your internet.';
              } else if (err.status === 400) {
                this.error = err.error?.message || 'Unable to create booking. Seats may no longer be available.';
              } else if (err.status === 401) {
                this.error = 'Session expired. Please login again.';
                this.auth.logout();
                this.router.navigate(['/login'], { state: { redirectTo: this.router.url } });
              } else if (err.status >= 500) {
                this.error = 'Server error. Please try again.';
              } else {
                this.error = err.error?.message || 'Booking failed.';
              }
              // Reload seats to get current state
              this.loadSeats();
            }
          });
        },
        error: (err: any) => {
          console.error('Lock seats error:', err);
          this.locking = false;
          if (err.status === 0) {
            this.error = 'Connection lost. Please check your internet.';
          } else if (err.status === 400) {
            this.error = err.error?.message || 'Unable to lock selected seats. Some seats may no longer be available.';
          } else if (err.status === 401) {
            this.error = 'Session expired. Please login again.';
            this.auth.logout();
            this.router.navigate(['/login'], { state: { redirectTo: this.router.url } });
          } else if (err.status >= 500) {
            this.error = 'Server error. Please try again.';
          } else {
            this.error = err.error?.message || 'Unable to lock selected seats.';
          }
          // Reload seats to get current state
          this.loadSeats();
        }
      });
  }

  
  // Remove passenger details
  removePassengerDetails(seatId: number) {
    delete this.passengerDetails[seatId];
  }

  
  // Remove seat from selection
  removeSeat(seat: any) {
    this.selectedSeats = this.selectedSeats.filter(s => s.id !== seat.id);
    this.removePassengerDetails(seat.id);
    this.saveSelection();
  }

  // Check if all selected seats have passenger details
  allSeatsHavePassengerDetails(): boolean {
    return this.selectedSeats.every(seat => this.passengerDetails[seat.id]);
  }

  // Get passenger details for display
  getPassengerDisplay(seat: any): string {
    const passenger = this.passengerDetails[seat.id];
    if (!passenger) return '';
    const genderIcon = passenger.gender === 'Male' ? '👦' : passenger.gender === 'Female' ? '👧' : '👤';
    return `${genderIcon} ${passenger.name}`;
  }

  // Check if seat is selected
  isSeatSelected(seatId: number): boolean {
    return this.selectedSeats.some(s => s.id === seatId);
  }

  // Get seat color based on gender
  getSeatColor(seat: any): string {
    if (!seat || this.bookedSeatIds.has(seat?.id)) {
      return 'booked';
    }
    if (this.isSeatSelected(seat?.id)) {
      const passenger = this.passengerDetails[seat.id];
      if (passenger?.gender === 'Male') return 'selected-male';
      if (passenger?.gender === 'Female') return 'selected-female';
      return 'selected';
    }
    return 'available';
  }

  // Show passenger form for seat
  showPassengerDetailsForm(seat: any) {
    this.currentSeatForPassenger = seat;
    this.showPassengerForm = true;
  }

  // Save passenger details
  savePassengerDetails(name: string, age: number, gender: string) {
    if (this.currentSeatForPassenger) {
      this.passengerDetails[this.currentSeatForPassenger.id] = { name, age, gender };
    }
    this.showPassengerForm = false;
    this.currentSeatForPassenger = null;
  }

  // Get seat for configuration
  getSeatForConfig(configSeat: any): any {
    return this.seatData?.seats?.find((seat: any) => seat.seatNumber === configSeat.label);
  }

  // Get current schedule for bus details display
  getCurrentSchedule(): any {
    return {
      id: this.scheduleId,
      departureTime: this.dep,
      basePrice: this.basePrice,
      pickupPoint: this.from,
      dropPoint: this.to,
      bus: this.busDetails
    };
  }

  // Group seats by row for grid display
  get rows(): any[] {
    if (!this.seatData?.seatConfiguration) return [];
    const rowMap = new Map<number, any[]>();
    for (const s of this.seatData.seatConfiguration) {
      if (!rowMap.has(s.row)) rowMap.set(s.row, []);
      rowMap.get(s.row)!.push(s);
    }
    return Array.from(rowMap.entries()).map(([row, seats]) => ({ row, seats }));
  }
}
