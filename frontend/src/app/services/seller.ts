import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Seller, SellerOrderSummary, SellerOrderDetail, SellerProduct, SellerAnalytics } from '../models/seller';

@Injectable({
  providedIn: 'root',
})
export class SellerService {
  private apiUrl = '/api/seller';
  private orderApiUrl = '/api/order';

  constructor(private http: HttpClient) {}

  getMe(): Observable<Seller> {
    return this.http.get<Seller>(`${this.apiUrl}/me`);
  }

  getOrders(): Observable<SellerOrderSummary[]> {
    return this.http.get<SellerOrderSummary[]>(`${this.orderApiUrl}/seller`);
  }

  getOrderDetail(orderId: string): Observable<SellerOrderDetail> {
    return this.http.get<SellerOrderDetail>(`${this.orderApiUrl}/seller/${orderId}`);
  }

  getProducts(): Observable<SellerProduct[]> {
    return this.http.get<SellerProduct[]>('/api/product/seller');
  }

  createProduct(data: { name: string; category: string; description: string; price: number; stock: number }): Observable<SellerProduct> {
    return this.http.post<SellerProduct>('/api/product', data);
  }

  updateProduct(id: string, data: { name: string; category: string; description: string; price: number; stock: number }): Observable<SellerProduct> {
    return this.http.put<SellerProduct>(`/api/product/${id}`, data);
  }

  getCategories(): Observable<{ productCategoryName: string; productCategoryNameEnglish: string | null }[]> {
    return this.http.get<{ productCategoryName: string; productCategoryNameEnglish: string | null }[]>('/api/category');
  }

  deleteProduct(id: string): Observable<void> {
    return this.http.delete<void>(`/api/product/${id}`);
  }

  relistProduct(id: string): Observable<void> {
    return this.http.post<void>(`/api/product/${id}/relist`, {});
  }

  getAnalytics(): Observable<SellerAnalytics> {
    return this.http.get<SellerAnalytics>(`${this.orderApiUrl}/seller/analytics`);
  }
}
