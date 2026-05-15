import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { Observable } from 'rxjs';
import { switchMap, tap } from 'rxjs/operators';
import { Product } from '../../../models/product';
import { CartService } from '../../../services/cart';
import { ProductService } from '../../../services/product';

@Component({
  standalone: true,
  selector: 'app-product-details',
  imports: [CommonModule, FormsModule, RouterLink, MatCardModule],
  templateUrl: './product-details.html',
})
export class ProductDetailsPage implements OnInit {
  product$!: Observable<Product | undefined>;
  quantity = 1;
  stockWarning = false;

  constructor(
    private route: ActivatedRoute,
    private productService: ProductService,
    private cartService: CartService,
  ) {}

  ngOnInit(): void {
    this.product$ = this.route.paramMap.pipe(
      switchMap(params => this.productService.getProduct(params.get('id')!)),
      tap(() => { this.quantity = 1; this.stockWarning = false; })
    );
  }

  decrement() {
    if (this.quantity > 1) this.quantity--;
    this.stockWarning = false;
  }

  increment(stock: number) {
    if (this.quantity < stock) this.quantity++;
  }

  onAddToCart(product: Product): void {
    const added = this.cartService.addToCart(product, this.quantity);
    this.stockWarning = !added;
  }
}
