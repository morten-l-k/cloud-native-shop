import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

export interface LoginResponse {
  Token: string;
  Id: string;
  Role: 'customer' | 'seller';
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = '/api/auth';

  constructor(private http: HttpClient) {}

  loginSeller(id: string, password: string): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${this.apiUrl}/login/seller`, { Id: id, Password: password })
      .pipe(tap(response => this.saveLogin(response)));
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('userId');
    localStorage.removeItem('role');
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
    localStorage.setItem('token', response.Token);
    localStorage.setItem('userId', response.Id);
    localStorage.setItem('role', response.Role);
  }
}
