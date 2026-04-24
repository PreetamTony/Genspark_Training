import { Component, OnInit, Inject, PLATFORM_ID } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { AuthService, AuthUser } from './services/auth.service';
import { Router } from '@angular/router';
import { ChatbotComponent } from './components/chatbot/chatbot.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule, ChatbotComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  user: AuthUser | null = null;
  theme: 'light' | 'dark' = 'dark';

  constructor(
    private auth: AuthService, 
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {}

  ngOnInit() {
    this.auth.user$.subscribe(u => this.user = u);
    this.initTheme();
  }

  initTheme() {
    if (isPlatformBrowser(this.platformId)) {
      const savedTheme = localStorage.getItem('theme') as 'light' | 'dark';
      this.theme = savedTheme || 'dark';
      this.applyTheme();
    }
  }

  toggleTheme() {
    this.theme = this.theme === 'light' ? 'dark' : 'light';
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem('theme', this.theme);
    }
    this.applyTheme();
  }

  private applyTheme() {
    if (isPlatformBrowser(this.platformId)) {
      document.documentElement.setAttribute('data-theme', this.theme);
    }
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/']);
  }

  get dashboardRoute(): string {
    if (!this.user) return '/login';
    if (this.user.role === 'Admin') return '/admin';
    if (this.user.role === 'Operator') return '/operator';
    return '/profile';
  }
}
