import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { Product } from '../models/product';

@Injectable({
  providedIn: 'root',
})
export class CartService {
  private items: Product[] = [];
  private cart = new BehaviorSubject<Product[]>([]);

  constructor() {}

  getCart(): Observable<Product[]> {
    return this.cart.asObservable();
  }

  addToCart(product: Product) {
    this.items.push(product);
    this.cart.next(this.items);
  }

  removeFromCart(product: Product) {
    const index = this.items.findIndex(item => item.id === product.id);
    if (index > -1) {
      this.items.splice(index, 1);
      this.cart.next([...this.items]);
    }
  }
}
