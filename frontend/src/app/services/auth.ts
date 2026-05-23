import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, BehaviorSubject } from 'rxjs';

export interface LoginResponse {
  token: string;
  id: string;
  role: 'customer' | 'seller';
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = '/api/auth';
  private currentUserSubject = new BehaviorSubject<LoginResponse | null>(this.getStoredUser());
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {}

  private getStoredUser(): LoginResponse | null {
    const token = localStorage.getItem('token');
    const id = localStorage.getItem('userId');
    const role = localStorage.getItem('role') as 'customer' | 'seller' | null;

    if (token && id && role) {
      return { token, id, role };
    }
    return null;
  }

  loginSeller(id: string, password: string): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${this.apiUrl}/login/seller`, { Id: id, Password: password })
      .pipe(tap(response => this.saveLogin(response)));
  }

  loginCustomer(email: string, password: string): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${this.apiUrl}/login/customer`, { Email: email, Password: password })
      .pipe(tap(response => this.saveLogin(response)));
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('userId');
    localStorage.removeItem('role');
    this.currentUserSubject.next(null);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  getUserId(): string | null {
    return localStorage.getItem('userId');
  }

  getRole(): string | null {
    return localStorage.getItem('role');
  }

  private saveLogin(response: LoginResponse): void {
    localStorage.setItem('token', response.token);
    localStorage.setItem('userId', response.id);
    localStorage.setItem('role', response.role);
    this.currentUserSubject.next(response);
  }
}
