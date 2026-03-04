import { Routes } from '@angular/router';
import { CustomerHomePage } from './components/customer/home/home';
import { CustomerProductsPage } from './components/customer/products/products';
import { SellerPage } from './components/seller/seller-home';
import { AdminPage } from './components/admin/admin-home';

export const routes: Routes = [
  // Default entry: send users to customer home
  { path: '', pathMatch: 'full', redirectTo: 'customer' },

  // The 3 “areas”
  { path: 'customer', component: CustomerHomePage },
  { path: 'customer/products', component: CustomerProductsPage },
  { path: 'seller', component: SellerPage },
  { path: 'admin', component: AdminPage },

  // Nice to have: fallback
  { path: '**', redirectTo: 'customer' }
];