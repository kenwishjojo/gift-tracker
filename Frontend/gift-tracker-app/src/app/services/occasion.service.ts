import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
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

  getOccasion(id: number): Observable<Occasion> {
    return this.http.get<Occasion>(`${this.baseUrl}/${id}`);
  }
}
