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
}
