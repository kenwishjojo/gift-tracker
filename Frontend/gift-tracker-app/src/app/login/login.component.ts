import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

// The first page: log in with email and password, then go to the home page
@Component({
  selector: 'app-login',
  imports: [FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent {
  // Two-way bound to the input fields with [(ngModel)]
  email = '';
  password = '';
  errorMessage = '';

  constructor(
    private authService: AuthService,
    private router: Router,
  ) {}

  onSubmit(): void {
    this.errorMessage = '';
    this.authService.login(this.email, this.password).subscribe({
      next: (profile) => {
        this.authService.saveProfile(profile);
        this.router.navigate(['/home']);
      },
      error: () => {
        // 401 from the backend (or the backend isn't running)
        this.errorMessage = 'Wrong email or password';
      },
    });
  }
}
