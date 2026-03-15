import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { Product } from '../../../models/product';
import { CartService } from '../../../services/cart';
import { ProductService } from '../../../services/product';

@Component({
  standalone: true,
  selector: 'app-customer-products',
  imports: [CommonModule],
  templateUrl: './products.html',
})
export class CustomerProductsPage implements OnInit {
  products$!: Observable<Product[]>;

  constructor(
    private productService: ProductService,
    private cartService: CartService,
  ) {}

  ngOnInit(): void {
    this.products$ = this.productService.getProducts();
  }

  onAddToCart(product: Product): void {
    this.cartService.addToCart(product);
  }
}