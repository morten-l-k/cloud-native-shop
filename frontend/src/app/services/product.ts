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
}
