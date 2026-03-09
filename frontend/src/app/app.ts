import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatButtonModule } from '@angular/material/button';

import { NavbarComponent } from './global/navbar/navbar';
import { SidebarComponent } from './global/sidebar/sidebar';

@Component({
  selector: 'app-root',
  standalone: true,
  templateUrl: './app.html',
  styleUrl: './app.css',
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    NavbarComponent,
    SidebarComponent,
    MatIconModule,
    MatSidenavModule,
    MatButtonModule
  ]
})
export class App {
  shopNamePrefix = 'Cloud Nat';
  shopNameSuffix = 've Shop';
}