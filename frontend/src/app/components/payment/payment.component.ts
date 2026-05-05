import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { BusService } from '../../services/bus.service';
import { EmailService } from '../../services/email.service';
import { AuthService } from '../../services/auth.service';
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
  
  // Coupon discount information
  couponCode: string = '';
  discount: number = 0;
  couponMessage: string = '';

  // Email confirmation fallback data
  emailConfirmationData: any = null;

  constructor(
    private router: Router,
    private busService: BusService,
    private emailService: EmailService,
    private authService: AuthService
  ) {
    const nav = this.router.getCurrentNavigation();
    const state = nav?.extras?.state as any;
    
    if (state) {
      this.booking = state.booking;
      this.from = state.from;
      this.to = state.to;
      this.dep = state.dep;
      this.bookingId = state.booking?.bookingId || state.booking?.id;
      this.passengerDetails = state.passengerDetails || {};
      this.selectedSeats = state.selectedSeats || [];
      this.couponCode = state.couponCode || '';
      this.discount = state.discount || 0;
      this.couponMessage = state.couponMessage || '';
    }
  }

  ngOnInit() {
    if (!this.booking) {
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
        
        // Send confirmation email
        this.sendConfirmationEmail();
        
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

  private sendConfirmationEmail() {
    // Prepare booking details for email
    const bookingDetails = {
      bookingId: this.bookingId,
      bookingDate: this.booking?.bookingDate || new Date(),
      passengerName: this.getPrimaryPassengerName(),
      email: this.getPassengerEmail(),
      seatNumbers: this.selectedSeats.map(seat => seat.seatNumber),
      baseFare: this.ticketFare,
      convenienceFee: this.booking?.convenienceFee || 0,
      discountAmount: this.discountAmount,
      totalAmount: this.finalAmount,
      couponCode: this.couponCode
    };

    // Prepare trip details for email
    const tripDetails = {
      route: `${this.from} → ${this.to}`,
      travelDate: this.dep,
      departureTime: this.booking?.schedule?.departureTime || 'Not specified',
      arrivalTime: this.booking?.schedule?.arrivalTime || 'Not specified',
      busOperator: this.booking?.schedule?.bus?.operatorProfile?.name || 'NexBus',
      busType: this.booking?.schedule?.bus?.busType || 'Not specified',
      boardingPoint: this.booking?.boardingPoint || 'Not specified',
      droppingPoint: this.booking?.droppingPoint || 'Not specified'
    };

    // Use the new backend email service
    const userEmail = this.getPassengerEmail();
    const userName = this.getPrimaryPassengerName();
    
    this.emailService.sendConfirmationEmail(bookingDetails, tripDetails).subscribe({
      next: (response) => {
        // Backend email sent successfully
      },
      error: (error) => {
        // Backend email failed, using simulated email service as fallback
        this.useSimulatedEmailFallback(bookingDetails, tripDetails);
      }
    });
  }

  private getPrimaryPassengerName(): string {
    // Try to get the logged-in user's name first
    const currentUser = this.authService.currentUser;
    if (currentUser && currentUser.name) {
      return currentUser.name;
    }
    
    // Fallback to first passenger's name
    const firstSeatId = this.selectedSeats[0]?.id;
    if (firstSeatId && this.passengerDetails[firstSeatId]) {
      return this.passengerDetails[firstSeatId].name;
    }
    
    return 'NexBus Customer';
  }

  private getPassengerEmail(): string {
    // Get the logged-in user's email from AuthService
    const currentUser = this.authService.currentUser;
    if (currentUser && currentUser.email) {
      return currentUser.email;
    }
    
    // Fallback to demo email if no user is logged in (shouldn't happen in normal flow)
    console.warn('No logged-in user found, using fallback email');
    return 'test@go-mail.us.to';
  }

  private showEmailConfirmationFallback(bookingDetails: any, tripDetails: any): void {
    // Store booking details for UI display
    this.emailConfirmationData = {
      bookingDetails,
      tripDetails,
      showFallback: true
    };
  }

  private useSimulatedEmailFallback(bookingDetails: any, tripDetails: any): void {
    this.emailService.sendSimulatedConfirmationEmail(bookingDetails, tripDetails).subscribe({
      next: (response) => {
        // Store the simulated response for UI display
        this.emailConfirmationData = {
          bookingDetails,
          tripDetails,
          simulatedResponse: response,
          showFallback: true
        };
      },
      error: (error) => {
        // Still show fallback UI even if simulation fails
        this.showEmailConfirmationFallback(bookingDetails, tripDetails);
      }
    });
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
    return '•••• •••• •••• ' + cleaned.slice(-4);
  }

  // Calculate final amount with coupon discount
  get finalAmount(): number {
    const baseAmount = this.booking?.totalPrice || 0;
    return Math.max(0, baseAmount - this.discount);
  }

  // Calculate ticket fare without convenience fee
  get ticketFare(): number {
    const baseFare = this.booking?.totalPrice || 0;
    const convenienceFee = this.booking?.convenienceFee || 0;
    return Math.max(0, baseFare - convenienceFee);
  }

  // Get discount amount for display
  get discountAmount(): number {
    return this.discount || 0;
  }

  // Check if coupon was applied
  get hasCouponApplied(): boolean {
    return !!(this.couponCode && this.discount > 0);
  }
}
