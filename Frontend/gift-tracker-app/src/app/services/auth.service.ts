import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Profile } from '../model/profile';

// Logs in through /api/Profile/login and remembers who is logged in.
// The profile is kept in localStorage, so refreshing the page doesn't log you out.
@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private loginUrl = 'http://localhost:5150/api/Profile/login';
  private storageKey = 'currentProfile';

  constructor(private http: HttpClient) {}

  // The backend answers 200 with the profile, or 401 if email or password is wrong
  login(email: string, password: string): Observable<Profile> {
    return this.http.post<Profile>(this.loginUrl, { email, password });
  }

  // Called by the login page after a successful login
  saveProfile(profile: Profile): void {
    localStorage.setItem(this.storageKey, JSON.stringify(profile));
  }

  // The logged-in profile, or null if nobody is logged in
  getProfile(): Profile | null {
    const saved = localStorage.getItem(this.storageKey);
    return saved ? JSON.parse(saved) : null;
  }

  isLoggedIn(): boolean {
    return this.getProfile() !== null;
  }

  logout(): void {
    localStorage.removeItem(this.storageKey);
  }
}
