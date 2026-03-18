import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

@Component({
  standalone: true,
  selector: 'app-dummy-login',
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
          placeholder=""
          style="padding: 0.75rem; border: 1px solid #d1d5db; border-radius: 8px;"
        />
      </label>

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
          style="padding: 0.75rem 1.25rem; border: none; border-radius: 8px; background: #2563eb; color: white; font-weight: 600; cursor: pointer;"
        >
          Login
        </button>
      </div>
    </section>
  `
})
export class DummyLoginPage {
  email = '';
  password = '';

  constructor(private router: Router) {}

  onLogin() {
  }
}