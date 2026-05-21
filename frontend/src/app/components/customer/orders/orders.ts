import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { Subscription } from 'rxjs';
import { AuthService } from '../../../services/auth';
import { CustomerOrderService } from '../../../services/customer-order';
import { CustomerOrder } from '../../../models/customer-order';
import { CustomerOrderDetailDialog } from '../order-detail/order-detail';

@Component({
  standalone: true,
  selector: 'app-customer-orders',
  imports: [CommonModule, MatCardModule, MatTableModule, MatButtonModule, MatIconModule, MatDialogModule],
  templateUrl: './orders.html',
  styleUrls: ['./orders.css'],
})
export class CustomerOrdersDashboardPage implements OnInit, OnDestroy {
  orders: CustomerOrder[] = [];
  displayedColumns: string[] = ['orderId', 'date', 'status', 'items', 'total', 'actions'];

  sortColumn: 'orderId' | 'date' | 'status' | 'items' | 'total' = 'date';
  sortDir: 'asc' | 'desc' = 'desc';

  get sortedOrders(): CustomerOrder[] {
    return [...this.orders].sort((a, b) => {
      let cmp = 0;
      switch (this.sortColumn) {
        case 'orderId': cmp = a.orderId.localeCompare(b.orderId); break;
        case 'date':    cmp = new Date(a.orderPurchaseTimestamp).getTime() - new Date(b.orderPurchaseTimestamp).getTime(); break;
        case 'status':  cmp = (a.orderStatus ?? '').localeCompare(b.orderStatus ?? ''); break;
        case 'items':   cmp = a.orderItems.length - b.orderItems.length; break;
        case 'total':   cmp = this.orderTotal(a) - this.orderTotal(b); break;
      }
      return this.sortDir === 'asc' ? cmp : -cmp;
    });
  }

  sortBy(col: typeof this.sortColumn): void {
    if (this.sortColumn === col) {
      this.sortDir = this.sortDir === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = col;
      this.sortDir = 'asc';
    }
    this.cdr.detectChanges();
  }

  private sub!: Subscription;

  constructor(
    private authService: AuthService,
    private customerOrderService: CustomerOrderService,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private dialog: MatDialog,
  ) {}

  ngOnInit(): void {
    if (this.authService.getRole() !== 'customer') {
      void this.router.navigate(['/customer/login']);
      return;
    }

    this.sub = this.customerOrderService.getMyOrders().subscribe(orders => {
      this.orders = orders;
      this.cdr.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  openDetail(orderId: string): void {
    this.dialog.open(CustomerOrderDetailDialog, { data: orderId });
  }

  orderTotal(order: CustomerOrder): number {
    return order.orderItems.reduce((sum, i) => sum + (i.price ?? 0) * (i.orderItemQuantity ?? 0), 0);
  }
}
