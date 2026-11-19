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

  // Sign up: POST a new profile with a password. The API hashes the password with BCrypt and
  // answers with the new profile (without the password). 400 if the email is already used.
  addProfile(fullName: string, email: string, dateOfBirth: string, password: string): Observable<Profile> {
    return this.http.post<Profile>(this.baseUrl, { fullName, email, dateOfBirth, password });
  }

  // Deleting a profile also deletes (on delete cascade in the database) their friendships, invites,
  // the occasions they made or receive – with all gifts on them – and the gifts they buy.
  deleteProfile(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
