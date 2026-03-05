import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from './global/navbar';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css',
  standalone: true,
  imports: [RouterOutlet, NavbarComponent, MatIconModule]
})
export class App {
  shopNamePrefix = 'Cloud Nat';
  shopNameSuffix = 've Shop';
}