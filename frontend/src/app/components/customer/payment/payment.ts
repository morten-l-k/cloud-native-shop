import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  standalone: true,
  selector: 'app-payment-success',
  imports: [CommonModule],
  template: `
    <section style="display: grid; gap: 1rem; text-align: center; padding: 2rem 1rem;">
      <h2>Payment Successful :)</h2>
    </section>
  `
})
export class PaymentSuccessPage {}