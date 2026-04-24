import { Routes } from '@angular/router';
import { LandingComponent } from './components/landing/landing.component';

export const routes: Routes = [
  { path: '', component: LandingComponent },
  {
    path: 'search-results',
    loadComponent: () => import('./components/search-results/search-results.component').then(m => m.SearchResultsComponent)
  },
  {
    path: 'seat-selection/:scheduleId',
    loadComponent: () => import('./components/seat-selection/seat-selection.component').then(m => m.SeatSelectionComponent)
  },
  {
    path: 'login',
    loadComponent: () => import('./components/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./components/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'profile',
    loadComponent: () => import('./components/profile/profile.component').then(m => m.ProfileComponent)
  },
  {
    path: 'payment/:bookingId',
    loadComponent: () => import('./components/payment/payment.component').then(m => m.PaymentComponent)
  },
  {
    path: 'operator',
    loadComponent: () => import('./components/operator-dashboard/operator-dashboard.component').then(m => m.OperatorDashboardComponent)
  },
  {
    path: 'admin',
    loadComponent: () => import('./components/admin-dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent)
  },
  { path: '**', redirectTo: '' }
];
