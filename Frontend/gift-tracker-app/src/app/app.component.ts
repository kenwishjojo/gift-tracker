import { Component } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { AuthService } from './services/auth.service';
import { ConfirmDialogComponent } from './confirm-dialog/confirm-dialog.component';

@Component({
  imports: [RouterOutlet, RouterLink, MatToolbarModule, ConfirmDialogComponent],
  selector: 'app-root',
  styleUrl: './app.component.css',
  templateUrl: './app.component.html',
})
export class App {
  // True while the "Log out?" warning box is open (the template shows it with @if)
  showLogoutConfirm = false;

  // public, so the template can ask if someone is logged in
  constructor(
    public authService: AuthService,
    private router: Router,
  ) {}

  // Called by the warning box's (confirmed) event: forget who is logged in and go to the login page
  logout(): void {
    this.showLogoutConfirm = false;
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
