import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { Observable } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { Product } from '../../../models/product';
import { CartService } from '../../../services/cart';
import { ProductService } from '../../../services/product';

@Component({
  standalone: true,
  selector: 'app-product-details',
  imports: [CommonModule, RouterLink, MatCardModule],
  templateUrl: './product-details.html',
})
export class ProductDetailsPage implements OnInit {
  product$!: Observable<Product | undefined>;

  constructor(
    private route: ActivatedRoute,
    private productService: ProductService,
    private cartService: CartService,
  ) {}

  ngOnInit(): void {
    // We read the 'id' parameter from the URL, then fetch the corresponding product
    this.product$ = this.route.paramMap.pipe(
      switchMap(params => {
        const id = params.get('id')!;
        return this.productService.getProduct(id);
      })
    );
  }

  onAddToCart(product: Product): void {
    this.cartService.addToCart(product);
  }
}