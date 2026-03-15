import { Routes } from '@angular/router';
import { CustomerHomePage } from './components/customer/home/home';
import { CustomerProductsPage } from './components/customer/products/products';
import { CustomerCartPage } from './components/customer/cart/cart';
import { PaymentSuccessPage } from './components/customer/payment/payment';
import { SellerPage } from './components/seller/seller-home';
import { AdminPage } from './components/admin/admin-home';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'customer' },

  { path: 'customer', component: CustomerHomePage },
  { path: 'customer/products', component: CustomerProductsPage },
  { path: 'customer/cart', component: CustomerCartPage },
  { path: 'customer/payment-success', component: PaymentSuccessPage },
  { path: 'seller', component: SellerPage },
  { path: 'admin', component: AdminPage },

  { path: '**', redirectTo: 'customer' }
];