import { Component, Inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { SellerService } from '../../../services/seller';
import { SellerOrderDetail } from '../../../models/seller';

@Component({
  standalone: true,
  selector: 'app-order-detail-dialog',
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatChipsModule,
  ],
  templateUrl: './order-detail.html',
})
export class OrderDetailDialog implements OnInit {
  order: SellerOrderDetail | null = null;
  loading = true;
  error = false;
  displayedColumns = ['product', 'category', 'qty', 'unitPrice', 'lineTotal'];

  constructor(
    @Inject(MAT_DIALOG_DATA) public orderId: string,
    private dialogRef: MatDialogRef<OrderDetailDialog>,
    private sellerService: SellerService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.sellerService.getOrderDetail(this.orderId).subscribe({
      next: (detail) => {
        this.order = detail;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.error = true;
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  close(): void {
    this.dialogRef.close();
  }

  getTotal(): number {
    if (!this.order) return 0;
    return this.order.orderItems.reduce(
      (sum, item) => sum + item.price * item.orderItemQuantity, 0
    );
  }
}
