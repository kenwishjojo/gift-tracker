import { Component } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from './services/auth.service';

@Component({
  imports: [RouterOutlet, RouterLink],
  selector: 'app-root',
  styleUrl: './app.component.css',
  templateUrl: './app.component.html',
})
export class App {
  // public, so the template can ask if someone is logged in
  constructor(
    public authService: AuthService,
    private router: Router,
  ) {}

  logout(event: Event): void {
    event.preventDefault(); // stop the link from reloading the page
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
