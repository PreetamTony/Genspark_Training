  import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { tap, catchError } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class BusService {
  private readonly API = 'http://localhost:5047/api';



  constructor(private http: HttpClient) {}

  searchSchedules(source: string, destination: string, date: string) {
    const params = new HttpParams().set('source', source).set('destination', destination).set('date', date);
    return this.http.get<any[]>(`${this.API}/schedules/search`, { params });
  }

  getSeatAvailability(scheduleId: number) {
    return this.http.get<any>(`${this.API}/schedules/${scheduleId}/seats`);
  }

  getScheduleDetails(scheduleId: number) {
    return this.http.get<any>(`${this.API}/schedules/${scheduleId}`);
  }

  lockSeat(scheduleId: number, seatId: number) {
    return this.http.post(`${this.API}/bookings/lock-seat`, { scheduleId, seatId });
  }

  unlockSeat(scheduleId: number, seatId: number) {
    return this.http.post(`${this.API}/bookings/unlock-seat`, { scheduleId, seatId });
  }

  createBooking(scheduleId: number, seatIds: number[]) {
    return this.http.post<any>(`${this.API}/bookings`, { scheduleId, seatIds });
  }

  validateCoupon(couponCode: string, scheduleId: number) {
    return this.http.post<any>(`${this.API}/coupons/validate`, { couponCode, scheduleId });
  }

  // Coupon management for operators
  getOperatorCoupons() {
    return this.http.get<any[]>(`${this.API}/operator/coupons`);
  }

  createCoupon(coupon: any) {
    console.log('=== BUS SERVICE CREATE COUPON ===');
    console.log('API URL:', `${this.API}/operator/coupons`);
    console.log('Coupon Data:', coupon);
    console.log('Authorization Header:', 'Bearer ' + localStorage.getItem('token'));
    
    return this.http.post<any>(`${this.API}/operator/coupons`, coupon).pipe(
      tap(response => console.log('Coupon creation response:', response)),
      catchError(error => {
        console.error('Coupon creation error:', error);
        throw error;
      })
    );
  }

  updateCoupon(couponId: number, coupon: any) {
    return this.http.put<any>(`${this.API}/operator/coupons/${couponId}`, coupon);
  }

  deleteCoupon(couponId: number) {
    return this.http.delete<any>(`${this.API}/operator/coupons/${couponId}`);
  }

  createBookingWithCoupon(scheduleId: number, seatIds: number[], couponCode: string) {
    return this.http.post<any>(`${this.API}/bookings`, { scheduleId, seatIds, couponCode });
  }

  getMyBookings() {
    return this.http.get<any[]>(`${this.API}/bookings/my-bookings`);
  }

  cancelBooking(bookingId: number) {
    return this.http.post<any>(`${this.API}/bookings/${bookingId}/cancel`, {});
  }

  searchLocations(q: string) {
    return this.http.get<any[]>(`${this.API}/locations`, { params: { q } });
  }

  getLocations() {
    return this.http.get<any[]>(`${this.API}/locations`);
  }

  getPopularLocations() {
    return this.http.get<any[]>(`${this.API}/locations/popular`);
  }

  // Operator APIs
  getLayouts() { return this.http.get<any[]>(`${this.API}/layouts`); }
  createLayout(data: any) { return this.http.post<any>(`${this.API}/layouts`, data); }
  getMyBuses() { return this.http.get<any[]>(`${this.API}/buses`); }
  requestBus(data: any) { return this.http.post<any>(`${this.API}/buses/request`, data); }
  updateBusStatus(id: number, status: string) { return this.http.put(`${this.API}/buses/${id}/status`, { status }); }
  getOperatorBookings() { return this.http.get<any[]>(`${this.API}/buses/bookings`); }
  setHeadOffice(locationId: number) { return this.http.put(`${this.API}/buses/head-office`, { locationId }); }
  getMySchedules() { return this.http.get<any[]>(`${this.API}/schedules/operator/my-schedules`); }
  cancelMySchedule(id: number) { return this.http.put(`${this.API}/schedules/operator/${id}/cancel`, {}); }
  createSchedule(data: any) { return this.http.post<any>(`${this.API}/schedules`, data); }
  getRoutes() { return this.http.get<any[]>(`${this.API}/routes`); }

  // Admin APIs
  getOperators() { return this.http.get<any[]>(`${this.API}/admin/operators`); }
  approveOperator(id: number) { return this.http.put(`${this.API}/admin/operators/${id}/approve`, {}); }
  rejectOperator(id: number) { return this.http.put(`${this.API}/admin/operators/${id}/reject`, {}); }
  toggleOperator(id: number, enable: boolean) {
    return this.http.put(`${this.API}/admin/operators/${id}/${enable ? 'enable' : 'disable'}`, {});
  }
  getPendingBuses() { return this.http.get<any[]>(`${this.API}/buses/pending`); }
  approveBus(id: number) { return this.http.put(`${this.API}/buses/${id}/approve`, {}); }
  rejectBus(id: number) { return this.http.put(`${this.API}/buses/${id}/reject`, {}); }
  getRevenue() { return this.http.get<any>(`${this.API}/admin/revenue`); }
  getAdminSchedules() { return this.http.get<any[]>(`${this.API}/admin/schedules`); }
  cancelSchedule(id: number) { return this.http.put(`${this.API}/admin/schedules/${id}/cancel`, {}); }
  addLocation(data: any) { return this.http.post<any>(`${this.API}/locations`, data); }
  addRoute(data: any) { return this.http.post<any>(`${this.API}/routes`, data); }
  setConvenienceFee(fee: number) { return this.http.put(`${this.API}/admin/config/convenience-fee`, { fee }); }

  }
