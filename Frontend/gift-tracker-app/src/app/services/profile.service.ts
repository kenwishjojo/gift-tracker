import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Profile } from '../model/profile';

// Talks to /api/Profile in the backend
@Injectable({
  providedIn: 'root',
})
export class ProfileService {
  private baseUrl = 'http://localhost:5150/api/Profile';

  constructor(private http: HttpClient) {}

  getProfiles(): Observable<Profile[]> {
    return this.http.get<Profile[]>(this.baseUrl);
  }

  getProfile(id: number): Observable<Profile> {
    return this.http.get<Profile>(`${this.baseUrl}/${id}`);
  }

  // PUT sends the whole profile (id in the body). It doesn't change the password.
  updateProfile(profile: Profile): Observable<void> {
    return this.http.put<void>(this.baseUrl, profile);
  }
}
