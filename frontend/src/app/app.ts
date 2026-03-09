import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { NavbarComponent } from './global/navbar';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    NavbarComponent,
    MatIconModule,
    MatSidenavModule,
    MatButtonModule
  ]
})
export class App {
  shopNamePrefix = 'Cloud Nat';
  shopNameSuffix = 've Shop';

  categories = ['Category1', 'Category2', 'Category3', 'Category4'];
}