import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface PlaceOrderItem {
  ProductId: string;
  Quantity: number;
  Price: number;
}

export interface PlaceOrderResponse {
  orderId: string;
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  constructor(private http: HttpClient) {}

  placeOrder(items: PlaceOrderItem[]): Observable<PlaceOrderResponse> {
    return this.http.post<PlaceOrderResponse>('/api/order', { Items: items });
  }

  payOrder(orderId: string): Observable<string> {
    return this.http.post('/api/payment', { OrderId: orderId }, { responseType: 'text' });
  }

  shipOrder(orderId: string): Observable<string> {
    return this.http.post('/api/shipment', { OrderId: orderId }, { responseType: 'text' });
  }
}
