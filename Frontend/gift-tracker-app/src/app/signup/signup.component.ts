import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { ProfileService } from '../services/profile.service';
import { AuthService } from '../services/auth.service';

// Page: create a new user (/signup), reached from the login page.
// Template-driven form like the login and gift forms: every field is two-way bound with [(ngModel)]
// and the simple rules (required, email, minlength) are written in the HTML (Lecture 7).
// Two rules need code, because they compare values: the passwords must match (a "cross-field"
// rule from Lecture 7) and the birthday can't be in the future.
@Component({
  selector: 'app-signup',
  imports: [FormsModule, RouterLink, MatIconModule],
  templateUrl: './signup.component.html',
  styleUrl: './signup.component.css',
})
export class SignupComponent {
  // Two-way bound to the input fields with [(ngModel)]
  fullName = '';
  email = '';
  dateOfBirth = ''; // "YYYY-MM-DD" from <input type="date">
  password = '';
  repeatPassword = '';
  acceptTerms = false;
  errorMessage = '';

  // Constructor injection (dependency injection, Lecture 6)
  constructor(
    private profileService: ProfileService,
    private authService: AuthService,
    private router: Router,
  ) {}

  // Cross-field rule: both password fields must be the same
  passwordsDiffer(): boolean {
    return this.repeatPassword !== '' && this.password !== this.repeatPassword;
  }

  // The birthday can't be after today ("YYYY-MM-DD" texts compare like dates)
  birthdayInFuture(): boolean {
    const now = new Date();
    const today = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`;
    return this.dateOfBirth > today;
  }

  // Step 1: create the profile (POST /api/Profile). Step 2: log in with the same email and password,
  // so the new user lands on the home page straight away.
  signUp(): void {
    this.errorMessage = '';
    this.profileService.addProfile(this.fullName.trim(), this.email.trim(), this.dateOfBirth, this.password).subscribe({
      next: () => {
        this.authService.login(this.email.trim(), this.password).subscribe((profile) => {
          this.authService.saveProfile(profile);
          this.router.navigate(['/home']);
        });
      },
      error: (err: HttpErrorResponse) => this.showError(err),
    });
  }

  // The API answers 400 with a text message. A used email breaks the unique index uk_profile_email.
  showError(err: HttpErrorResponse): void {
    const reason = typeof err.error === 'string' ? err.error : '';
    if (reason.includes('uk_profile_email')) {
      this.errorMessage = 'There is already an account with this email. Try logging in instead.';
    } else {
      this.errorMessage = `The account couldn't be created. ${reason || 'Please try again.'}`;
    }
  }
}
