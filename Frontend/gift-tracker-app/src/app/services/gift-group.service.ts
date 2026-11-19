import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { GiftGroup, GroupMember } from '../model/gift-group';

// Talks to /api/GiftGroup and /api/GroupMember – who is splitting which gift.
// Same pattern as the other services (Lecture 6): one method per endpoint, each returns an Observable.
@Injectable({
  providedIn: 'root',
})
export class GiftGroupService {
  private groupUrl = 'http://localhost:5150/api/GiftGroup';
  private memberUrl = 'http://localhost:5150/api/GroupMember';

  constructor(private http: HttpClient) {}

  getGroups(): Observable<GiftGroup[]> {
    return this.http.get<GiftGroup[]>(this.groupUrl);
  }

  getMembers(): Observable<GroupMember[]> {
    return this.http.get<GroupMember[]>(this.memberUrl);
  }

  // Creates the group, its owner (the buyer) and the friends as viewers – all in one transaction in the API
  addGroup(giftId: number, name: string, ownerId: number, memberIds: number[]): Observable<GiftGroup> {
    return this.http.post<GiftGroup>(this.groupUrl, { giftId, name, ownerId, memberIds });
  }

  // Also deletes the group's members (on delete cascade in the database)
  deleteGroup(id: number): Observable<void> {
    return this.http.delete<void>(`${this.groupUrl}/${id}`);
  }
}
