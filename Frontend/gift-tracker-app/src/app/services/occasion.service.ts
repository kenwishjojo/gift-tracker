import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { Occasion } from '../model/occasion';

// Talks to /api/Occasion in the backend
@Injectable({
  providedIn: 'root',
})
export class OccasionService {
  private baseUrl = 'http://localhost:5150/api/Occasion';

  constructor(private http: HttpClient) {}

  getOccasions(): Observable<Occasion[]> {
    return this.http.get<Occasion[]>(this.baseUrl);
  }

  // The backend has no "occasions for one person" endpoint yet (step 4 of the plan),
  // so for now we get all occasions and keep only the ones for this recipient.
  getOccasionsForRecipient(profileId: number): Observable<Occasion[]> {
    return this.getOccasions().pipe(map((occasions) => occasions.filter((o) => o.recipientId === profileId)));
  }

  getOccasion(id: number): Observable<Occasion> {
    return this.http.get<Occasion>(`${this.baseUrl}/${id}`);
  }

  // POST returns the new occasion with the id the database gave it
  addOccasion(occasion: Occasion): Observable<Occasion> {
    return this.http.post<Occasion>(this.baseUrl, occasion);
  }

  // PUT sends the whole occasion (id in the body)
  updateOccasion(occasion: Occasion): Observable<void> {
    return this.http.put<void>(this.baseUrl, occasion);
  }

  // Also deletes the occasion's gifts (on delete cascade in the database)
  deleteOccasion(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
