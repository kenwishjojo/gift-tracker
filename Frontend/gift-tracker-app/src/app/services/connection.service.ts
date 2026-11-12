import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Connection } from '../model/connection';

// Talks to /api/Connection in the backend (friend requests and friendships).
// Same pattern as StudentService in Lecture 6: one method per endpoint, each returns an Observable.
@Injectable({
  providedIn: 'root',
})
export class ConnectionService {
  private baseUrl = 'http://localhost:5150/api/Connection';

  constructor(private http: HttpClient) {}

  getConnections(): Observable<Connection[]> {
    return this.http.get<Connection[]>(this.baseUrl);
  }

  // Send a friend request. It always starts as "pending" (the API refuses anything else).
  sendRequest(requesterId: number, addresseeId: number): Observable<Connection> {
    return this.http.post<Connection>(this.baseUrl, { requesterId, addresseeId, status: 'pending' });
  }

  // Accept a request: PUT the same connection with status "accepted"
  accept(connection: Connection): Observable<void> {
    return this.http.put<void>(this.baseUrl, { ...connection, status: 'accepted' });
  }

  // Decline or cancel a request, or remove a friend – all three delete the connection
  deleteConnection(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
