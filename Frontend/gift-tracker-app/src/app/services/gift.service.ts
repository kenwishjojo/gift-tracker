import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { Gift } from '../model/gift';

// Talks to /api/Gift in the backend
@Injectable({
  providedIn: 'root',
})
export class GiftService {
  private baseUrl = 'http://localhost:5150/api/Gift';

  constructor(private http: HttpClient) {}

  getGifts(): Observable<Gift[]> {
    return this.http.get<Gift[]>(this.baseUrl);
  }

  // The backend has no "gifts for one occasion" endpoint yet (step 4 of the plan),
  // so for now we get all gifts and keep only the ones for this occasion.
  getGiftsForOccasion(occasionId: number): Observable<Gift[]> {
    return this.getGifts().pipe(map((gifts) => gifts.filter((g) => g.occasionId === occasionId)));
  }

  getGift(id: number): Observable<Gift> {
    return this.http.get<Gift>(`${this.baseUrl}/${id}`);
  }

  // POST returns the new gift with the id the database gave it
  addGift(gift: Gift): Observable<Gift> {
    return this.http.post<Gift>(this.baseUrl, gift);
  }

  // PUT sends the whole gift (id in the body)
  updateGift(gift: Gift): Observable<void> {
    return this.http.put<void>(this.baseUrl, gift);
  }

  deleteGift(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
