import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Product } from '../models/product';

@Injectable({
  providedIn: 'root',
})
export class ProductService {
  private apiUrl = '/api/product';

  constructor(private http: HttpClient) { }

  /**
   * Fetches a list of products from the backend API.
   * @returns An Observable that emits an array of Product objects.
   */
  getProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(this.apiUrl);
  }

  /**
   * Fetches a single product by its ID from the backend API.
   * @param id The ID of the product to fetch.
   * @returns An Observable that emits a single Product object.
   */
  getProduct(id: string | number): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/${id}`);
  }
}
