import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { CartService } from '../../services/cart';
import { AuthService } from '../../services/auth';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, MatIconModule],
  templateUrl: './navbar.html',
  styleUrls: ['./navbar.css']
})
export class NavbarComponent implements OnInit, OnDestroy {
  private cartService = inject(CartService);
  private authService = inject(AuthService);
  private router = inject(Router);

  cartItemCount = 0;
  searchQuery = '';
  user$ = this.authService.currentUser$;
  private cartSubscription!: Subscription;

  ngOnInit() {
    this.cartSubscription = this.cartService.getCart().subscribe(items => {
      this.cartItemCount = items.length;
    });
  }

  onSearch(): void {
    const query = this.searchQuery.trim();
    this.searchQuery = '';
    this.router.navigate(['/customer/products'], {
      queryParams: query ? { search: query } : {}
    });
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/customer']);
  }

  ngOnDestroy() {
    if (this.cartSubscription) {
      this.cartSubscription.unsubscribe();
    }
  }
}
