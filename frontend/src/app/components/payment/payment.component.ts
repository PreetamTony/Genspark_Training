import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { BusService } from '../../services/bus.service';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './payment.component.html',
  styleUrl: './payment.component.css'
})
export class PaymentComponent implements OnInit {
  booking: any = null;
  from = ''; to = ''; dep = '';
  step: 'summary' | 'processing' | 'confirmed' | 'failed' = 'summary';
  bookingId = 0;
  error = '';
  loading = false;
  paymentDetails = {
    cardNumber: '',
    cardName: '',
    expiryMonth: '',
    expiryYear: '',
    cvv: ''
  };
  formErrors: { [key: string]: string } = {};
  
  // Passenger details with gender information
  passengerDetails: { [key: number]: { name: string; age: number; gender: string } } = {};
  selectedSeats: any[] = [];

  constructor(
    private router: Router,
    private busService: BusService
  ) {
    console.log('=== PAYMENT COMPONENT CONSTRUCTOR ===');
    const nav = this.router.getCurrentNavigation();
    console.log('Navigation object:', nav);
    const state = nav?.extras?.state as any;
    console.log('State data:', state);
    
    if (state) {
      this.booking = state.booking;
      this.from = state.from;
      this.to = state.to;
      this.dep = state.dep;
      this.bookingId = state.booking?.bookingId || state.booking?.id;
      this.passengerDetails = state.passengerDetails || {};
      this.selectedSeats = state.selectedSeats || [];
      console.log('Payment data initialized:', {
        booking: this.booking,
        bookingId: this.bookingId,
        from: this.from,
        to: this.to,
        dep: this.dep,
        passengerDetails: this.passengerDetails,
        selectedSeats: this.selectedSeats
      });
    } else {
      console.log('No state data found in navigation');
    }
  }

  ngOnInit() {
    console.log('=== PAYMENT COMPONENT INIT ===');
    console.log('Booking data:', this.booking);
    console.log('Booking ID:', this.bookingId);
    
    if (!this.booking) {
      console.log('No booking data, redirecting to home');
      this.router.navigate(['/']);
      return;
    }
    
    // Verify booking exists
    this.verifyBooking();
  }

  private verifyBooking() {
    this.loading = true;
    this.busService.getMyBookings()
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: (bookings) => {
          const currentBooking = bookings.find((b: any) => b.id === this.bookingId);
          if (!currentBooking) {
            this.error = 'Booking not found. Please make a new booking.';
            setTimeout(() => this.router.navigate(['/']), 3000);
          }
        },
        error: (err) => {
          console.error('Failed to verify booking:', err);
          this.error = 'Unable to verify booking. Please try again.';
        }
      });
  }

  // Get passenger display with gender information
  getPassengerDisplay(seat: any): string {
    const passenger = this.passengerDetails[seat.id];
    if (!passenger) return '';
    const genderIcon = passenger.gender === 'Male' ? '👦' : passenger.gender === 'Female' ? '👧' : '👤';
    return `${genderIcon} ${passenger.name} (${passenger.age}y)`;
  }

  // Get seat color based on gender for display
  getSeatGenderClass(seat: any): string {
    const passenger = this.passengerDetails[seat.id];
    if (passenger?.gender === 'Male') return 'male-passenger';
    if (passenger?.gender === 'Female') return 'female-passenger';
    return 'default-passenger';
  }

  validatePaymentForm(): boolean {
    this.formErrors = {};
    let isValid = true;

    if (!this.paymentDetails.cardNumber.trim()) {
      this.formErrors['cardNumber'] = 'Card number is required';
      isValid = false;
    } else if (!/^\d{16}$/.test(this.paymentDetails.cardNumber.replace(/\s/g, ''))) {
      this.formErrors['cardNumber'] = 'Please enter a valid 16-digit card number';
      isValid = false;
    }

    if (!this.paymentDetails.cardName.trim()) {
      this.formErrors['cardName'] = 'Cardholder name is required';
      isValid = false;
    } else if (this.paymentDetails.cardName.trim().length < 3) {
      this.formErrors['cardName'] = 'Please enter a valid cardholder name';
      isValid = false;
    }

    if (!this.paymentDetails.expiryMonth || !this.paymentDetails.expiryYear) {
      this.formErrors['expiry'] = 'Expiry date is required';
      isValid = false;
    } else {
      const currentYear = new Date().getFullYear();
      const currentMonth = new Date().getMonth() + 1;
      const expYear = parseInt(this.paymentDetails.expiryYear);
      const expMonth = parseInt(this.paymentDetails.expiryMonth);
      
      if (expYear < currentYear || (expYear === currentYear && expMonth < currentMonth)) {
        this.formErrors['expiry'] = 'Card has expired';
        isValid = false;
      }
    }

    if (!this.paymentDetails.cvv.trim()) {
      this.formErrors['cvv'] = 'CVV is required';
      isValid = false;
    } else if (!/^\d{3,4}$/.test(this.paymentDetails.cvv)) {
      this.formErrors['cvv'] = 'Please enter a valid CVV';
      isValid = false;
    }

    return isValid;
  }

  processPayment() {
    if (!this.validatePaymentForm()) {
      return;
    }

    this.step = 'processing';
    this.error = '';
    this.loading = true;

    // Simulate payment processing with realistic timing
    setTimeout(() => {
      // 85% success rate for demo - more realistic than 90%
      const success = Math.random() > 0.15;
      
      if (success) {
        this.step = 'confirmed';
        // Clear sensitive payment data
        this.paymentDetails = {
          cardNumber: '',
          cardName: '',
          expiryMonth: '',
          expiryYear: '',
          cvv: ''
        };
      } else {
        this.step = 'failed';
        this.error = 'Payment was declined. Please check your card details and try again.';
      }
      
      this.loading = false;
    }, 2500);
  }

  formatCardNumber() {
    // Format card number with spaces every 4 digits
    let value = this.paymentDetails.cardNumber.replace(/\s/g, '');
    let formattedValue = value.match(/.{1,4}/g)?.join(' ') || value;
    if (formattedValue !== this.paymentDetails.cardNumber) {
      this.paymentDetails.cardNumber = formattedValue;
    }
  }

  getExpiryYears(): number[] {
    const currentYear = new Date().getFullYear();
    const years = [];
    for (let i = 0; i < 15; i++) {
      years.push(currentYear + i);
    }
    return years;
  }

  getExpiryMonths(): number[] {
    return Array.from({ length: 12 }, (_, i) => i + 1);
  }

  goHome() { 
    this.router.navigate(['/']); 
  }

  viewTickets() { 
    this.router.navigate(['/profile']); 
  }

  retry() { 
    this.step = 'summary';
    this.error = '';
    this.formErrors = {};
  }

  get maskedCardNumber(): string {
    if (!this.paymentDetails.cardNumber) return '';
    const cleaned = this.paymentDetails.cardNumber.replace(/\s/g, '');
    if (cleaned.length < 4) return '•••• •••• •••• ••••';
    return `•••• •••• •••• ${cleaned.slice(-4)}`;
  }
}
