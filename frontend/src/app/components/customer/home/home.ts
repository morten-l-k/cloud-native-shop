import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  standalone: true,
  selector: 'app-customer-home',
  imports: [CommonModule, RouterLink],
  templateUrl: './home.html'
})
export class CustomerHomePage {
}