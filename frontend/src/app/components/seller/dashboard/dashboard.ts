import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Chart, ChartConfiguration, registerables } from 'chart.js';
import { forkJoin, of, catchError } from 'rxjs';
import { AuthService } from '../../../services/auth';
import { SellerService } from '../../../services/seller';
import { Seller, SellerOrderSummary, SellerProduct, SellerAnalytics } from '../../../models/seller';
import { OrderDetailDialog } from '../order-detail/order-detail';
import { ProductFormDialog } from '../product-form/product-form';

Chart.register(...registerables);

@Component({
  standalone: true,
  selector: 'app-seller-dashboard',
  imports: [
    CommonModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatTabsModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatSelectModule,
    MatFormFieldModule,
    MatInputModule,
    FormsModule,
  ],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class SellerDashboardPage implements OnInit, OnDestroy {
  seller: Seller | null = null;
  orders: SellerOrderSummary[] = [];
  products: SellerProduct[] = [];
  analytics: SellerAnalytics | null = null;
  
  loading = true;
  error: string | null = null;

  ordersColumns: string[] = ['orderId', 'date', 'status', 'items', 'total', 'actions'];
  orderSortColumn: 'orderId' | 'date' | 'status' | 'items' | 'total' = 'date';
  orderSortDir: 'asc' | 'desc' = 'desc';

  get sortedOrders(): SellerOrderSummary[] {
    return [...this.orders].sort((a, b) => {
      let cmp = 0;
      switch (this.orderSortColumn) {
        case 'orderId': cmp = a.orderId.localeCompare(b.orderId); break;
        case 'date':    cmp = new Date(a.orderPurchaseTimestamp).getTime() - new Date(b.orderPurchaseTimestamp).getTime(); break;
        case 'status':  cmp = a.orderStatus.localeCompare(b.orderStatus); break;
        case 'items':   cmp = a.itemCount - b.itemCount; break;
        case 'total':   cmp = a.totalValue - b.totalValue; break;
      }
      return this.orderSortDir === 'asc' ? cmp : -cmp;
    });
  }

  sortOrdersBy(col: typeof this.orderSortColumn): void {
    if (this.orderSortColumn === col) {
      this.orderSortDir = this.orderSortDir === 'asc' ? 'desc' : 'asc';
    } else {
      this.orderSortColumn = col;
      this.orderSortDir = 'asc';
    }
    this.cdr.detectChanges();
  }
  productsColumns: string[] = ['name', 'category', 'price', 'stock', 'sold', 'revenue', 'actions'];

  productSearch = '';
  productCategory = '';
  productSortColumn: 'name' | 'category' | 'price' | 'stock' | 'sold' | 'revenue' = 'name';
  productSortDir: 'asc' | 'desc' = 'asc';

  get filteredProducts() {
    const search = this.productSearch.toLowerCase().trim();
    const filtered = this.products.filter(p => {
      const matchesSearch = !search || p.productName.toLowerCase().includes(search);
      const matchesCategory = !this.productCategory || p.productCategoryName === this.productCategory;
      return matchesSearch && matchesCategory;
    });

    return filtered.sort((a, b) => {
      let cmp = 0;
      switch (this.productSortColumn) {
        case 'name':     cmp = a.productName.localeCompare(b.productName); break;
        case 'category': cmp = (a.productCategoryName ?? '').localeCompare(b.productCategoryName ?? ''); break;
        case 'price':    cmp = a.productPrice - b.productPrice; break;
        case 'stock':    cmp = a.productStock - b.productStock; break;
        case 'sold':     cmp = a.totalSold - b.totalSold; break;
        case 'revenue':  cmp = a.totalRevenue - b.totalRevenue; break;
      }
      return this.productSortDir === 'asc' ? cmp : -cmp;
    });
  }

  trackByCategory(_i: number, cat: { value: string; label: string }): string {
    return cat.value;
  }

  sortProductsBy(col: typeof this.productSortColumn): void {
    if (this.productSortColumn === col) {
      this.productSortDir = this.productSortDir === 'asc' ? 'desc' : 'asc';
    } else {
      this.productSortColumn = col;
      this.productSortDir = 'asc';
    }
    this.cdr.detectChanges();
  }
get productCategories(): { value: string; label: string }[] {
  const seen = new Set<string>();
  const result: { value: string; label: string }[] = [];
  for (const p of this.products) {
    if (p.productCategoryName && !seen.has(p.productCategoryName)) {
      seen.add(p.productCategoryName);
      result.push({ value: p.productCategoryName, label: p.productCategoryNameEnglish || p.productCategoryName });
    }
  }
  return result.sort((a, b) => a.label.localeCompare(b.label));
}

private revenueChart: Chart | null = null;
private statusChart: Chart | null = null;

constructor(
    private authService: AuthService,
    private sellerService: SellerService,
    private router: Router,
    private dialog: MatDialog,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    if (this.authService.getRole() !== 'seller') {
      void this.router.navigate(['/seller/login']);
      return;
    }

    this.loadData();
  }

  loadData(): void {
    this.loading = true;
    this.error = null;

    forkJoin({
      seller: this.sellerService.getMe().pipe(catchError(err => { console.error('Seller fetch failed', err); return of(null); })),
      orders: this.sellerService.getOrders().pipe(catchError(err => { console.error('Orders fetch failed', err); return of([]); })),
      products: this.sellerService.getProducts().pipe(catchError(err => { console.error('Products fetch failed', err); return of([]); })),
      analytics: this.sellerService.getAnalytics().pipe(catchError(err => { console.error('Analytics fetch failed', err); return of(null); }))
    }).subscribe({
      next: (data) => {
        this.seller = data.seller;
        this.orders = data.orders;
        this.products = data.products;
        this.analytics = data.analytics;
        
        if (!this.seller && this.orders.length === 0 && this.products.length === 0) {
          this.error = 'Failed to load dashboard data. The backend might still be starting up. Please try refreshing.';
        }

        this.loading = false;
        this.cdr.detectChanges();
        
        if (this.analytics) {
          setTimeout(() => this.buildCharts(), 0);
        }
      },
      error: (err) => {
        console.error('Dashboard load critical failure', err);
        this.error = 'A critical error occurred while loading the dashboard.';
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  ngOnDestroy(): void {
    this.revenueChart?.destroy();
    this.statusChart?.destroy();
  }

  private buildCharts(): void {
    if (!this.analytics) return;
    this.buildRevenueChart();
    this.buildStatusChart();
  }

  private buildRevenueChart(): void {
    const ctx = document.getElementById('revenueChart') as HTMLCanvasElement | null;
    if (!ctx) return;

    const labels = this.analytics!.monthlyRevenue.map(m => {
      const [year, month] = m.month.split('-');
      return new Date(+year, +month - 1).toLocaleString('default', { month: 'short', year: '2-digit' });
    });
    const data = this.analytics!.monthlyRevenue.map(m => m.revenue);

    const config: ChartConfiguration = {
      type: 'bar',
      data: {
        labels,
        datasets: [{
          label: 'Revenue (USD)',
          data,
          backgroundColor: 'rgba(99, 102, 241, 0.7)',
          borderColor: 'rgba(99, 102, 241, 1)',
          borderWidth: 1,
          borderRadius: 4,
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: {
          y: {
            beginAtZero: true,
            ticks: { callback: (v: string | number) => `$${(+v).toLocaleString()}` }
          }
        }
      }
    };

    this.revenueChart?.destroy();
    this.revenueChart = new Chart(ctx, config);
  }

  private buildStatusChart(): void {
    const ctx = document.getElementById('statusChart') as HTMLCanvasElement | null;
    if (!ctx) return;

    const statusColors: Record<string, string> = {
      delivered: 'rgba(16, 185, 129, 0.8)',
      shipped: 'rgba(59, 130, 246, 0.8)',
      created: 'rgba(251, 191, 36, 0.8)',
      canceled: 'rgba(239, 68, 68, 0.8)',
      processing: 'rgba(139, 92, 246, 0.8)',
    };

    const labels = this.analytics!.statusBreakdown.map(s => s.status);
    const data = this.analytics!.statusBreakdown.map(s => s.count);
    const colors = labels.map(l => statusColors[l] ?? 'rgba(107, 114, 128, 0.8)');

    this.statusChart?.destroy();
    this.statusChart = new Chart(ctx, {
      type: 'doughnut',
      data: {
        labels,
        datasets: [{ data, backgroundColor: colors, borderWidth: 2 }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { position: 'bottom', labels: { padding: 12, font: { size: 12 } } }
        }
      }
    });
  }

  openProductForm(product?: SellerProduct): void {
    const ref = this.dialog.open(ProductFormDialog, {
      width: '500px',
      maxWidth: '95vw',
      data: product ?? null,
    });
    ref.afterClosed().subscribe((result?: { product: SellerProduct; isEdit: boolean }) => {
      if (!result) return;
      if (result.isEdit) {
        this.products = this.products.map(p =>
          p.productId === result.product.productId
            ? { ...p, ...result.product }
            : p
        );
      } else {
        this.products = [...this.products, { ...result.product, totalSold: result.product.totalSold ?? 0, totalRevenue: result.product.totalRevenue ?? 0 }];
      }
      this.cdr.detectChanges();
    });
  }

  delistProduct(product: SellerProduct): void {
    if (!confirm(`Delist "${product.productName}"? It will no longer appear in the storefront but order history is preserved.`)) return;
    this.sellerService.deleteProduct(product.productId).subscribe({
      next: () => {
        this.products = this.products.map(p =>
          p.productId === product.productId ? { ...p, isActive: false } : p
        );
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Delist failed', err);
        alert('Failed to delist product.');
      }
    });
  }

  openOrderDetail(orderId: string): void {
    this.dialog.open(OrderDetailDialog, {
      data: orderId,
      width: '700px',
      maxWidth: '95vw',
    });
  }

  logout(): void {
    this.authService.logout();
    void this.router.navigate(['/seller/login']);
  }

  getLowStockCount(): number {
    return this.products.filter(p => p.productStock <= 5).length;
  }
}
