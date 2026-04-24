import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { map, shareReplay } from 'rxjs/operators';
import { Product } from '../../../models/product';
import { ProductService } from '../../../services/product';
import { MatCardModule } from '@angular/material/card';

@Component({
  standalone: true,
  selector: 'app-customer-home',
  imports: [CommonModule, RouterLink, MatCardModule],
  templateUrl: './home.html',
})
export class CustomerHomePage implements OnInit {
  featuredProducts$!: Observable<Product[]>;
  recentPurchases$!: Observable<Product[]>;

  constructor(private productService: ProductService) {}

  ngOnInit(): void {
    // We use shareReplay(1) so the HTTP request is only triggered once, 
    // even though we use the async pipe twice in the template.
    const products$ = this.productService.getProducts({ page: 1 }).pipe(map(p => p.items), shareReplay(1));

    this.featuredProducts$ = products$.pipe(map(products => products.slice(0, 4)));
    this.recentPurchases$ = products$.pipe(map(products => products.slice(4, 8)));
  }
}