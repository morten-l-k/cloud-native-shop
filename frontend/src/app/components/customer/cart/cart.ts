import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, firstValueFrom } from 'rxjs';
import { map } from 'rxjs/operators';
import { CartItem } from '../../../models/product';
import { CartService } from '../../../services/cart';
import { AuthService } from '../../../services/auth';
import { OrderService } from '../../../services/order';

@Component({
  standalone: true,
  selector: 'app-customer-cart',
  imports: [CommonModule, FormsModule],
  templateUrl: './cart.html'
})
export class CustomerCartPage implements OnInit {
  private cartService = inject(CartService);
  private authService = inject(AuthService);
  private orderService = inject(OrderService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  cartItems$!: Observable<CartItem[]>;
  total$!: Observable<number>;
  isLoading = false;
  errorMessage: string | null = null;

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
    } else {
      void this.router.navigate(['/customer/dummy-login']);
    }
  }

  async processPayment() {
    this.isLoading = true;
    this.errorMessage = null;

    try {
      const items = await firstValueFrom(this.cartService.getCart());
      const orderItems = items.map(i => ({
        ProductId: i.product.id,
        Quantity: i.quantity,
        Price: i.product.price,
      }));
      const order = await firstValueFrom(this.orderService.placeOrder(orderItems));
      await firstValueFrom(this.orderService.payOrder(order.orderId));

      this.cartService.clearCart();
      void this.router.navigateByUrl('/customer/payment-success');
    } catch (err: any) {
      this.errorMessage = typeof err?.error === 'string' ? err.error : 'Checkout failed. Please try again.';
    } finally {
      this.isLoading = false;
      this.cdr.detectChanges();
    }
  }
}
