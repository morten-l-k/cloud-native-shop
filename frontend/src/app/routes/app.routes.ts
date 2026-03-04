import { Routes } from '@angular/router';
import { CustomerHomePage } from '../pages/customer/customer-home';
import { SellerPage } from '../pages/seller/seller-home';
import { AdminPage } from '../pages/admin/admin-home';

export const routes: Routes = [
  // Default entry: send users to customer home
  { path: '', pathMatch: 'full', redirectTo: 'customer' },

  // The 3 “areas”
  { path: 'customer', component: CustomerHomePage },
  { path: 'seller', component: SellerPage },
  { path: 'admin', component: AdminPage },

  // Nice to have: fallback
  { path: '**', redirectTo: 'customer' }
];