import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CustomerOrder, CustomerOrderDetail } from '../models/customer-order';

@Injectable({
  providedIn: 'root',
})
export class CustomerOrderService {
  private orderApiUrl = '/api/order';

  constructor(private http: HttpClient) {}

  getMyOrders(): Observable<CustomerOrder[]> {
    return this.http.get<CustomerOrder[]>(`${this.orderApiUrl}/me`);
  }

  getOrderDetail(orderId: string): Observable<CustomerOrderDetail> {
    return this.http.get<CustomerOrderDetail>(`${this.orderApiUrl}/me/${orderId}`);
  }
}
