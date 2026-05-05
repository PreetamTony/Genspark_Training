import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { timeout, catchError } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class EmailService {
  private readonly backendApiUrl = 'http://localhost:5047/api/email';

  constructor(private http: HttpClient) {}

  sendConfirmationEmail(bookingDetails: any, tripDetails: any): Observable<any> {
    // Use backend email service
    const emailData = {
      bookingId: bookingDetails.bookingId,
      userEmail: bookingDetails.email,
      userName: bookingDetails.passengerName
    };

    // Sending booking confirmation via backend

    const headers = new HttpHeaders({
      'Content-Type': 'application/json'
    });

    return this.http.post(`${this.backendApiUrl}/send-booking-confirmation`, emailData, { headers }).pipe(
      timeout(15000), // 15 second timeout
      catchError((error: any) => {
        console.error('Backend email request failed:', error);
        return throwError(() => error);
      })
    );
  }

  // Test method using backend API
  sendTestEmail(userEmail: string, userName: string): Observable<any> {
    const testData = {
      toEmail: userEmail,
      toName: userName
    };

    // Sending test email via backend

    const headers = new HttpHeaders({
      'Content-Type': 'application/json'
    });

    return this.http.post(`${this.backendApiUrl}/test-email`, testData, { headers }).pipe(
      timeout(10000), // 10 second timeout
      catchError((error: any) => {
        console.error('Backend test email request failed:', error);
        return throwError(() => error);
      })
    );
  }

  // Alternative method: Simulate email sending for demo purposes
  sendSimulatedConfirmationEmail(bookingDetails: any, tripDetails: any): Observable<any> {
    // Simulated email sending for demo purposes
    
    // Simulate API delay
    return new Observable(observer => {
      setTimeout(() => {
        const simulatedResponse = {
          success: true,
          message: 'Email confirmation simulated successfully',
          emailSent: bookingDetails.email,
          bookingId: bookingDetails.bookingId,
          timestamp: new Date().toISOString()
        };
        
        observer.next(simulatedResponse);
        observer.complete();
      }, 1000);
    });
  }

  private generateEmailBody(bookingDetails: any, tripDetails: any): string {
    const bookingDate = new Date(bookingDetails.bookingDate).toLocaleDateString();
    const travelDate = new Date(tripDetails.travelDate).toLocaleDateString();
    const departureTime = tripDetails.departureTime || 'Not specified';
    const arrivalTime = tripDetails.arrivalTime || 'Not specified';

    return `
      <!DOCTYPE html>
      <html>
      <head>
        <meta charset="utf-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>NexBus Booking Confirmation</title>
        <style>
          body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }
          .header { background: linear-gradient(135deg, #6366f1, #8b5cf6); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }
          .content { background: #f9fafb; padding: 30px; border-radius: 0 0 10px 10px; }
          .booking-info { background: white; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #6366f1; }
          .trip-details { background: white; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #10b981; }
          .seat-info { background: white; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #f59e0b; }
          .price-info { background: white; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #ef4444; }
          .footer { text-align: center; margin-top: 30px; color: #6b7280; font-size: 14px; }
          h1 { margin: 0; font-size: 28px; }
          h2 { color: #1f2937; margin-top: 0; }
          .label { font-weight: bold; color: #374151; }
          .value { color: #6b7280; }
          .highlight { background: #fef3c7; padding: 2px 6px; border-radius: 4px; }
        </style>
      </head>
      <body>
        <div class="header">
          <h1>🚌 NexBus Booking Confirmed!</h1>
          <p>Your journey details are ready</p>
        </div>
        
        <div class="content">
          <div class="booking-info">
            <h2>📋 Booking Information</h2>
            <p><span class="label">Booking ID:</span> <span class="value highlight">#${bookingDetails.bookingId}</span></p>
            <p><span class="label">Booking Date:</span> <span class="value">${bookingDate}</span></p>
            <p><span class="label">Passenger Name:</span> <span class="value">${bookingDetails.passengerName || 'Not specified'}</span></p>
            <p><span class="label">Email:</span> <span class="value">${bookingDetails.email || 'Not specified'}</span></p>
          </div>

          <div class="trip-details">
            <h2>🛣️ Trip Details</h2>
            <p><span class="label">Route:</span> <span class="value">${tripDetails.route || 'Not specified'}</span></p>
            <p><span class="label">Travel Date:</span> <span class="value">${travelDate}</span></p>
            <p><span class="label">Departure Time:</span> <span class="value">${departureTime}</span></p>
            <p><span class="label">Arrival Time:</span> <span class="value">${arrivalTime}</span></p>
            <p><span class="label">Bus Operator:</span> <span class="value">${tripDetails.busOperator || 'Not specified'}</span></p>
            <p><span class="label">Bus Type:</span> <span class="value">${tripDetails.busType || 'Not specified'}</span></p>
          </div>

          <div class="seat-info">
            <h2>🪑 Seat Information</h2>
            <p><span class="label">Seat Numbers:</span> <span class="value highlight">${bookingDetails.seatNumbers?.join(', ') || 'Not specified'}</span></p>
            <p><span class="label">Total Seats:</span> <span class="value">${bookingDetails.seatNumbers?.length || 0}</span></p>
            <p><span class="label">Boarding Point:</span> <span class="value">${tripDetails.boardingPoint || 'Not specified'}</span></p>
            <p><span class="label">Dropping Point:</span> <span class="value">${tripDetails.droppingPoint || 'Not specified'}</span></p>
          </div>

          <div class="price-info">
            <h2>💰 Payment Details</h2>
            <p><span class="label">Base Fare:</span> <span class="value">₹${bookingDetails.baseFare || 0}</span></p>
            <p><span class="label">Convenience Fee:</span> <span class="value">₹${bookingDetails.convenienceFee || 0}</span></p>
            ${bookingDetails.discountAmount > 0 ? `<p><span class="label">Discount:</span> <span class="value" style="color: #10b981;">-₹${bookingDetails.discountAmount}</span></p>` : ''}
            <p><span class="label">Total Amount Paid:</span> <span class="value" style="font-size: 18px; font-weight: bold; color: #059669;">₹${bookingDetails.totalAmount || 0}</span></p>
            ${bookingDetails.couponCode ? `<p><span class="label">Coupon Applied:</span> <span class="value">${bookingDetails.couponCode}</span></p>` : ''}
          </div>

          <div style="background: #fef3c7; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #f59e0b;">
            <h3 style="margin-top: 0; color: #92400e;">📝 Important Information</h3>
            <ul style="margin: 10px 0; padding-left: 20px;">
              <li>Please arrive at the boarding point at least 15 minutes before departure</li>
              <li>Carry a valid ID proof for verification</li>
              <li>Show this booking confirmation at the boarding point</li>
              <li>For any queries, contact our customer support</li>
            </ul>
          </div>
        </div>

        <div class="footer">
          <p><strong>Thank you for choosing NexBus! 🚌</strong></p>
          <p>Have a safe and comfortable journey</p>
          <p style="font-size: 12px; margin-top: 20px;">
            This is an automated confirmation email. Please do not reply to this message.
          </p>
        </div>
      </body>
      </html>
    `;
  }
}
