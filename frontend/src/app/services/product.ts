import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Product, ProductFilters, ProductPage } from '../models/product';

@Injectable({
  providedIn: 'root',
})
export class ProductService {
  private apiUrl = '/api/product';

  constructor(private http: HttpClient) { }

  getProducts(filters: ProductFilters = {}): Observable<ProductPage> {
    let params = new HttpParams().set('page', filters.page ?? 1);
    if (filters.minPrice != null) params = params.set('minPrice', filters.minPrice);
    if (filters.maxPrice != null) params = params.set('maxPrice', filters.maxPrice);
    if (filters.category)         params = params.set('category', filters.category);
    if (filters.sort)             params = params.set('sort', filters.sort);
    if (filters.search)           params = params.set('search', filters.search);
    return this.http.get<ProductPage>(this.apiUrl, { params });
  }

  getProduct(id: string | number): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/${id}`);
  }
}
