import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css',
  standalone: true
})
export class App {
  // 1. A variable we can show in the HTML
  shopName = 'Cloud Native Shop';

  // 2. A function we can trigger from the HTML
  testButton() {
    alert('Clicked a button');
  }
}
