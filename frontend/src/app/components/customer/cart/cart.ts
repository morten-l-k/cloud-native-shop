import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { Product } from '../../../models/product';
import { CartService } from '../../../services/cart';
import { AuthService } from '../../../services/auth';
import { map } from 'rxjs/operators';

@Component({
  standalone: true,
  selector: 'app-customer-cart',
  imports: [CommonModule],
  templateUrl: './cart.html'
})
export class CustomerCartPage implements OnInit {
  private cartService = inject(CartService);
  private authService = inject(AuthService);
  private router = inject(Router);

  cartItems$!: Observable<Product[]>;
  total$!: Observable<number>;
  isCheckoutModalOpen = false;

  ngOnInit() {
    this.cartItems$ = this.cartService.getCart();
    this.total$ = this.cartItems$.pipe(
      map(items => items.reduce((acc, item) => acc + item.price, 0))
    );
  }

  onRemoveFromCart(product: Product) {
    this.cartService.removeFromCart(product);
  }

  openCheckoutOptions() {
    if (this.authService.getRole() === 'customer') {
      this.processPayment();
      return;
    }
    this.isCheckoutModalOpen = true;
  }

  closeCheckoutOptions() {
    this.isCheckoutModalOpen = false;
  }

  processPayment() {
    this.closeCheckoutOptions();
    this.router.navigate(['/customer/payment-success']);
  }

  goToLogin() {
    this.closeCheckoutOptions();
    this.router.navigate(['/customer/dummy-login']);
  }
}
