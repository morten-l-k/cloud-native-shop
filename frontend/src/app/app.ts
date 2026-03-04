import { Component } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { CloudIcon } from './components/cloud-icon';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css',
  standalone: true,
  imports: [CloudIcon, RouterOutlet, RouterLink]
})
export class App {
  shopNamePrefix = 'Cloud Nat';
  shopNameSuffix = 've Shop';
}