import { Component, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Profile } from '../model/profile';
import { AuthService } from '../services/auth.service';

// The landing page after login
@Component({
  selector: 'app-home',
  imports: [RouterLink],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
})
export class HomeComponent implements OnInit {
  profile: Profile | null = null;

  constructor(
    private authService: AuthService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.profile = this.authService.getProfile();
    // Nobody logged in: send them to the login page
    if (!this.profile) {
      this.router.navigate(['/login']);
    }
  }
}
