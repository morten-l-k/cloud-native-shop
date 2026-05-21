import { Routes } from '@angular/router';
import { CustomerHomePage } from './components/customer/home/home';
import { CustomerProductsPage } from './components/customer/products/products';
import { ProductDetailsPage } from './components/customer/products/product-details';
import { CustomerCartPage } from './components/customer/cart/cart';
import { PaymentSuccessPage } from './components/customer/payment/payment';
import { CustomerLoginPage } from './components/customer/login/login';
import { CustomerOrdersDashboardPage } from './components/customer/orders/orders';
import { SellerLoginPage } from './components/seller/login/login';
import { SellerDashboardPage } from './components/seller/dashboard/dashboard';
import { AdminPage } from './components/admin/admin-home';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'customer' },

  { path: 'customer', component: CustomerHomePage },
  { path: 'customer/products', component: CustomerProductsPage },
  { path: 'customer/products/:id', component: ProductDetailsPage },
  { path: 'customer/cart', component: CustomerCartPage },
  { path: 'customer/payment-success', component: PaymentSuccessPage },
  { path: 'customer/login', component: CustomerLoginPage },
  { path: 'customer/orders', component: CustomerOrdersDashboardPage },
  { path: 'seller', component: SellerDashboardPage },
  { path: 'seller/login', component: SellerLoginPage },
  { path: 'admin', component: AdminPage },

  { path: '**', redirectTo: 'customer' }
];
