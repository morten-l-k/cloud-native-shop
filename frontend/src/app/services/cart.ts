import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { CartItem, Product } from '../models/product';

@Injectable({
  providedIn: 'root',
})
export class CartService {
  private items: CartItem[] = [];
  private cart = new BehaviorSubject<CartItem[]>([]);

  getCart(): Observable<CartItem[]> {
    return this.cart.asObservable();
  }

  addToCart(product: Product, quantity: number = 1): boolean {
    const existing = this.items.find(i => i.product.id === product.id);
    const currentQty = existing?.quantity ?? 0;
    if (currentQty + quantity > product.stock) return false;

    if (existing) {
      existing.quantity += quantity;
    } else {
      this.items.push({ product, quantity });
    }
    this.cart.next([...this.items]);
    return true;
  }

  updateQuantity(productId: string, quantity: number): void {
    const item = this.items.find(i => i.product.id === productId);
    if (!item) return;
    if (quantity <= 0) {
      this.items = this.items.filter(i => i.product.id !== productId);
    } else {
      item.quantity = quantity;
    }
    this.cart.next([...this.items]);
  }

  removeFromCart(productId: string): void {
    this.items = this.items.filter(i => i.product.id !== productId);
    this.cart.next([...this.items]);
  }
}
