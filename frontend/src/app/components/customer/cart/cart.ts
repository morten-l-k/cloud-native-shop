import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { Product } from '../../../models/product';
import { CartService } from '../../../services/cart';
import { map } from 'rxjs/operators';

@Component({
  standalone: true,
  selector: 'app-customer-cart',
  imports: [CommonModule, RouterLink],
  templateUrl: './cart.html'
})
export class CustomerCartPage implements OnInit {
  cartItems$!: Observable<Product[]>;
  total$!: Observable<number>;
  isCheckoutModalOpen = false;

  constructor(
    private cartService: CartService,
    private router: Router
  ) {}

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
    this.isCheckoutModalOpen = true;
  }

  closeCheckoutOptions() {
    this.isCheckoutModalOpen = false;
  }

  goToGuestCheckout() {
    this.closeCheckoutOptions();
    this.router.navigate(['/customer/payment-success']);
  }

  goToLogin() {
    this.closeCheckoutOptions();
    this.router.navigate(['/customer/dummy-login']);
  }
}