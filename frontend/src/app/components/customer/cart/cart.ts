import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  standalone: true,
  selector: 'app-customer-cart',
  imports: [CommonModule, RouterLink],
  template: `
    <section style="display: grid; gap: 1rem;">
      <h2>Your Cart</h2>

      <div
        style="
          display: grid;
          gap: 1rem;
          grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
        "
      >
        <div
          *ngFor="let slot of cartSlots"
          style="
            min-height: 140px;
            border: 1px solid #ddd;
            border-radius: 10px;
            padding: 1rem;
            background: #fafafa;
            display: flex;
            align-items: center;
            justify-content: center;
            color: #777;
          "
        >
          Cart item slot
        </div>
      </div>

      <!-- Total row -->
      <div
        style="
          display: flex;
          justify-content: flex-end;
          align-items: center;
          font-size: 1.2rem;
          font-weight: 600;
          margin-top: 0.5rem;
        "
      >
        Total: $0.00
      </div>

      <div style="display: flex; justify-content: flex-end;">
        <a
          routerLink="/customer/payment-success"
          style="
            display: inline-flex;
            align-items: center;
            justify-content: center;
            padding: 0.75rem 1.25rem;
            border-radius: 8px;
            background: #16a34a;
            color: white;
            text-decoration: none;
            font-weight: 600;
          "
        >
          Pay
        </a>
      </div>
    </section>
  `
})
export class CustomerCartPage {
  //Liste med element for hver item
  cartSlots = [1, 2, 3, 4, 5, 6, 7];
}