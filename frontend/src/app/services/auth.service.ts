import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, tap } from 'rxjs';

export interface AuthUser {
  id: number;
  name: string;
  email: string;
  role: string;
  operatorProfileId?: number;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly API = 'http://localhost:5047/api/auth';


  private _user = new BehaviorSubject<AuthUser | null>(this.loadUser());

  user$ = this._user.asObservable();

  constructor(private http: HttpClient) {}

  private loadUser(): AuthUser | null {
    const s = localStorage.getItem('user');
    return s ? JSON.parse(s) : null;
  }

  get currentUser(): AuthUser | null { return this._user.value; }
  get token(): string | null { return localStorage.getItem('token'); }
  get isLoggedIn(): boolean { return !!this.token; }

  register(data: any) {
    return this.http.post(`${this.API}/register`, data);
  }

  login(email: string, password: string) {
    return this.http.post<any>(`${this.API}/login`, { email, password }).pipe(
      tap(res => {
        localStorage.setItem('token', res.token);
        localStorage.setItem('user', JSON.stringify(res.user));
        this._user.next(res.user);
      })
    );
  }

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this._user.next(null);
  }
}
