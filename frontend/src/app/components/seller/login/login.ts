import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../../services/auth';

@Component({
  standalone: true,
  selector: 'app-seller-login',
  imports: [CommonModule, FormsModule],
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
      <h2 style="margin: 0;">Seller login</h2>
      <p style="margin: 0; color: #6b7280;">
        For now, use a seller ID and the password <strong>password</strong>.
      </p>

      <label style="display: grid; gap: 0.4rem;">
        <span>Seller ID</span>
        <input
          [(ngModel)]="sellerId"
          type="text"
          placeholder="seller_abc123def456"
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

      <button
        type="button"
        (click)="onLogin()"
        [disabled]="isLoading"
        style="padding: 0.75rem 1.25rem; border: none; border-radius: 8px; background: #2563eb; color: white; font-weight: 600; cursor: pointer;"
      >
        {{ isLoading ? 'Logging in…' : 'Login as seller' }}
      </button>
    </section>
  `,
})
export class SellerLoginPage {
  sellerId = '';
  password = 'password';
  errorMessage = '';
  isLoading = false;

  constructor(
    private authService: AuthService,
    private router: Router,
  ) {}

  onLogin(): void {
    this.errorMessage = '';

    if (!this.sellerId.trim() || !this.password.trim()) {
      this.errorMessage = 'Please enter both seller ID and password.';
      return;
    }

    this.isLoading = true;
    this.authService
      .loginSeller(this.sellerId.trim(), this.password)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: () => void this.router.navigate(['/seller']),
        error: () => {
          this.errorMessage = 'Login failed. Check the seller ID and password.';
        },
      });
  }
}
