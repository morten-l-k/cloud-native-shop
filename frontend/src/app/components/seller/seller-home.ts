import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth';

@Component({
  standalone: true,
  imports: [CommonModule],
  template: `
    <section
      style="
        max-width: 720px;
        margin: 0 auto;
        display: grid;
        gap: 1rem;
        padding: 1.5rem;
        border: 1px solid #e5e7eb;
        border-radius: 12px;
        background: white;
      "
    >
      <h2 style="margin: 0;">Seller dashboard</h2>

      <p *ngIf="sellerId; else notLoggedIn" style="margin: 0;">
        Logged in as seller <strong>{{ sellerId }}</strong>.
      </p>

      <ng-template #notLoggedIn>
        <p style="margin: 0; color: #6b7280;">
          This is a dummy seller screen. Login is not protected yet.
        </p>
      </ng-template>

      <p style="margin: 0; color: #6b7280;">
        Later this page can show seller orders, products, and account details.
      </p>

      <button
        type="button"
        (click)="logout()"
        style="justify-self: start; padding: 0.75rem 1.25rem; border: 1px solid #d1d5db; border-radius: 8px; background: white; color: #111827; font-weight: 600; cursor: pointer;"
      >
        Logout
      </button>
    </section>
  `,
})
export class SellerPage {
  sellerId: string | null;

  constructor(
    private authService: AuthService,
    private router: Router,
  ) {
    this.sellerId = this.authService.getRole() === 'seller' ? this.authService.getUserId() : null;
  }

  logout(): void {
    this.authService.logout();
    void this.router.navigate(['/seller/login']);
  }
}
