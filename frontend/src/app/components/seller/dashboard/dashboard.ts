import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Observable } from 'rxjs';
import { AuthService } from '../../../services/auth';
import { SellerService } from '../../../services/seller';
import { Seller, SellerOrderSummary } from '../../../models/seller';

@Component({
  standalone: true,
  selector: 'app-seller-dashboard',
  imports: [
    CommonModule,
    RouterLink,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class SellerDashboardPage implements OnInit {
  seller$!: Observable<Seller>;
  orders$!: Observable<SellerOrderSummary[]>;
  displayedColumns: string[] = ['orderId', 'date', 'status', 'items', 'total', 'actions'];

  constructor(
    private authService: AuthService,
    private sellerService: SellerService,
    private router: Router
  ) {}

  ngOnInit(): void {
    if (this.authService.getRole() !== 'seller') {
      void this.router.navigate(['/seller/login']);
      return;
    }

    this.seller$ = this.sellerService.getMe();
    this.orders$ = this.sellerService.getOrders();
  }

  logout(): void {
    this.authService.logout();
    void this.router.navigate(['/seller/login']);
  }
}
