import { Component } from '@angular/core';
import { CloudIcon } from './shared/components/cloud-icon';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
  standalone: true,
  imports: [CloudIcon]
})
export class AppComponent {
  // 1. We split the name to insert the icon in the HTML
  shopNamePrefix = 'Cloud Nat';
  shopNameSuffix = 've Shop';

  // 2. A function we can trigger from the HTML
  testButton() {
    alert('Clicked a button');
  }
}
