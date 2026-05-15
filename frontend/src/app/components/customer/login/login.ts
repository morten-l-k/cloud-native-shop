import { ChangeDetectorRef, Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../services/auth';
import { finalize } from 'rxjs';

@Component({
  standalone: true,
  selector: 'app-login',
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <section
      style="
        max-width: 420px;
        margin: 0 auto;
        display: grid;
        gap: 1rem;
        padding: 1.5rem;
        border: 1px solid #e5e7eb;
        border-radius: 12px;
        background: white;
      "
    >
      <h2 style="margin: 0;">Customer login</h2>
      <p style="margin: 0; color: #6b7280;">
        Use your email and the password <strong>password</strong>.
      </p>

      <label style="display: grid; gap: 0.4rem;">
        <span>Email</span>
        <input
          [(ngModel)]="email"
          type="email"
          placeholder="customer@example.com"
          style="padding: 0.75rem; border: 1px solid #d1d5db; border-radius: 8px;"
        />
      </label>

      <label style="display: grid; gap: 0.4rem;">
        <span>Password</span>
        <input
          [(ngModel)]="password"
          type="password"
          placeholder="password"
          style="padding: 0.75rem; border: 1px solid #d1d5db; border-radius: 8px;"
          (keyup.enter)="onLogin()"
        />
      </label>

      <p *ngIf="errorMessage" style="margin: 0; color: #b91c1c;">
        {{ errorMessage }}
      </p>

      <div style="display: flex; gap: 0.75rem; justify-content: flex-end; flex-wrap: wrap;">
        <a
          routerLink="/customer/cart"
          style="padding: 0.75rem 1.25rem; border-radius: 8px; border: 1px solid #d1d5db; color: #111827; text-decoration: none;"
        >
          Back to cart
        </a>
        <button
          type="button"
          (click)="onLogin()"
          [disabled]="isLoading"
          style="padding: 0.75rem 1.25rem; border: none; border-radius: 8px; background: #2563eb; color: white; font-weight: 600; cursor: pointer;"
        >
          {{ isLoading ? 'Logging in…' : 'Login' }}
        </button>
      </div>
    </section>
  `
})
export class CustomerLoginPage {
  email = '';
  password = 'password';
  errorMessage = '';
  isLoading = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  onLogin() {
    this.errorMessage = '';

    if (!this.email.trim() || !this.password.trim()) {
      this.errorMessage = 'Please enter both email and password.';
      return;
    }

    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.email.trim())) {
      this.errorMessage = 'Please enter a valid email address.';
      return;
    }

    this.isLoading = true;
    this.authService
      .loginCustomer(this.email.trim(), this.password)
      .pipe(finalize(() => { this.isLoading = false; this.cdr.markForCheck(); }))
      .subscribe({
        next: () => void this.router.navigate(['/customer/cart']),
        error: () => {
          this.errorMessage = 'Login failed. Check the email and password.';
          this.cdr.markForCheck();
        },
      });
  }
}
