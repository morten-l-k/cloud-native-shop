import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { BehaviorSubject, switchMap } from 'rxjs';
import { Product, ProductFilters, ProductPage } from '../../../models/product';
import { Category } from '../../../models/category';
import { CartService } from '../../../services/cart';
import { CategoryService } from '../../../services/category';
import { ProductService } from '../../../services/product';

@Component({
  standalone: true,
  selector: 'app-customer-products',
  imports: [CommonModule, FormsModule, RouterLink, MatCardModule],
  templateUrl: './products.html',
})
export class CustomerProductsPage implements OnInit {
  categories: Category[] = [];

  minPrice: number | null = null;
  maxPrice: number | null = null;
  selectedCategory = '';
  selectedSort: 'price_asc' | 'price_desc' | '' = '';
  searchQuery = '';

  private filters$ = new BehaviorSubject<ProductFilters>({ page: 1 });

  productPage$ = this.filters$.pipe(
    switchMap(filters => this.productService.getProducts(filters))
  );

  constructor(
    private productService: ProductService,
    private categoryService: CategoryService,
    private cartService: CartService,
    private route: ActivatedRoute,
  ) {}

  ngOnInit(): void {
    this.categoryService.getCategories().subscribe(cats => this.categories = cats);
    this.route.queryParams.subscribe(params => {
      if (params['search']) {
        this.searchQuery = params['search'];
        this.filters$.next({ page: 1, search: this.searchQuery });
      }
    });
  }

  applyFilters(): void {
    this.filters$.next({
      page: 1,
      minPrice: this.minPrice ?? undefined,
      maxPrice: this.maxPrice ?? undefined,
      category: this.selectedCategory || undefined,
      sort: this.selectedSort || undefined,
      search: this.searchQuery.trim() || undefined,
    });
  }

  clearFilters(): void {
    this.minPrice = null;
    this.maxPrice = null;
    this.selectedCategory = '';
    this.selectedSort = '';
    this.searchQuery = '';
    this.filters$.next({ page: 1 });
  }

  prevPage(data: ProductPage): void {
    if (data.page > 1) this.filters$.next({ ...this.filters$.value, page: data.page - 1 });
  }

  nextPage(data: ProductPage): void {
    if (data.page < data.totalPages) this.filters$.next({ ...this.filters$.value, page: data.page + 1 });
  }

  stockWarnings = new Set<string>();

  onAddToCart(product: Product): void {
    const added = this.cartService.addToCart(product);
    if (added) {
      this.stockWarnings.delete(product.id);
    } else {
      this.stockWarnings.add(product.id);
    }
  }
}
