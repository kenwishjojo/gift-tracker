import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { OccasionInvite } from '../model/occasion';

// Talks to /api/OccasionInvite – who is invited to which occasion.
// Same pattern as the other services (Lecture 6): one method per endpoint, each returns an Observable.
@Injectable({
  providedIn: 'root',
})
export class InviteService {
  private baseUrl = 'http://localhost:5150/api/OccasionInvite';

  constructor(private http: HttpClient) {}

  getInvites(): Observable<OccasionInvite[]> {
    return this.http.get<OccasionInvite[]>(this.baseUrl);
  }

  // Replaces the whole invite list of one occasion with these profiles (one transaction in the API).
  // The API refuses anyone who isn't a friend of the creator, the creator, or the person it is for.
  replaceInvites(occasionId: number, profileIds: number[]): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/occasion/${occasionId}`, profileIds);
  }
}
