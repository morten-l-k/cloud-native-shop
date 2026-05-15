import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { CartItem } from '../../../models/product';
import { CartService } from '../../../services/cart';
import { AuthService } from '../../../services/auth';

@Component({
  standalone: true,
  selector: 'app-customer-cart',
  imports: [CommonModule, FormsModule],
  templateUrl: './cart.html'
})
export class CustomerCartPage implements OnInit {
  private cartService = inject(CartService);
  private authService = inject(AuthService);
  private router = inject(Router);

  cartItems$!: Observable<CartItem[]>;
  total$!: Observable<number>;
  isCheckoutModalOpen = false;

  ngOnInit() {
    this.cartItems$ = this.cartService.getCart();
    this.total$ = this.cartItems$.pipe(
      map(items => items.reduce((acc, i) => acc + i.product.price * i.quantity, 0))
    );
  }

  onRemove(productId: string) {
    this.cartService.removeFromCart(productId);
  }

  onDecrement(item: CartItem) {
    this.cartService.updateQuantity(item.product.id, item.quantity - 1);
  }

  onIncrement(item: CartItem) {
    this.cartService.updateQuantity(item.product.id, item.quantity + 1);
  }

  onQuantityInput(item: CartItem, value: string) {
    const qty = parseInt(value, 10);
    if (!isNaN(qty)) this.cartService.updateQuantity(item.product.id, qty);
  }

  hasStockIssue(items: CartItem[]): boolean {
    return items.some(i => i.quantity > i.product.stock);
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
    this.router.navigate(['/customer/login']);
  }
}
