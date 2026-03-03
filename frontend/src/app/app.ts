import { Component } from '@angular/core';
import { CloudIcon } from './components/cloud-icon';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css',
  standalone: true,
  imports: [CloudIcon]
})
export class App {
  // 1. We split the name to insert the icon in the HTML
  shopNamePrefix = 'Cloud Nat';
  shopNameSuffix = 've Shop';

  // 2. A function we can trigger from the HTML
  testButton() {
    alert('Clicked a button');
  }
}
