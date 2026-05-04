import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Seller, SellerOrderSummary, SellerOrderDetail } from '../models/seller';

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
}
