import { Component, Inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CustomerOrderService } from '../../../services/customer-order';
import { CustomerOrderDetail } from '../../../models/customer-order';

@Component({
  standalone: true,
  selector: 'app-customer-order-detail-dialog',
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './order-detail.html',
})
export class CustomerOrderDetailDialog implements OnInit {
  order: CustomerOrderDetail | null = null;
  loading = true;
  error = false;
  displayedColumns = ['product', 'category', 'qty', 'unitPrice', 'lineTotal'];

  constructor(
    @Inject(MAT_DIALOG_DATA) public orderId: string,
    private dialogRef: MatDialogRef<CustomerOrderDetailDialog>,
    private customerOrderService: CustomerOrderService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.customerOrderService.getOrderDetail(this.orderId).subscribe({
      next: (detail) => {
        this.order = detail;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.error = true;
        this.loading = false;
        this.cdr.detectChanges();
      },
    });
  }

  close(): void {
    this.dialogRef.close();
  }

  getTotal(): number {
    if (!this.order) return 0;
    return this.order.orderItems.reduce(
      (sum, item) => sum + (item.price ?? 0) * (item.orderItemQuantity ?? 0), 0
    );
  }
}
