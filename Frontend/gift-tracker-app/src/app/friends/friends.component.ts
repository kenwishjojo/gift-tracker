import { Component, OnInit } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { Connection } from '../model/connection';
import { Profile } from '../model/profile';
import { ConnectionService } from '../services/connection.service';
import { ProfileService } from '../services/profile.service';
import { AuthService } from '../services/auth.service';
import { AvatarComponent } from '../avatar/avatar.component';

// Page: send friend requests, answer the ones you got, and see your friends (/friends).
// All data comes from the connection table: a row is a request while its status is "pending"
// and a friendship once it is "accepted".
@Component({
  selector: 'app-friends',
  imports: [FormsModule, RouterLink, MatIconModule, AvatarComponent],
  templateUrl: './friends.component.html',
  styleUrl: './friends.component.css',
})
export class FriendsComponent implements OnInit {
  connections: Connection[] = []; // every connection that involves the logged-in user
  profiles: Profile[] = []; // to show names, and to find a person by email
  meId = 0;

  // Two-way bound to the "Add a friend" field with [(ngModel)] (Lecture 7)
  email = '';
  errorMessage = '';
  successMessage = '';

  // Constructor injection (dependency injection, Lecture 6)
  constructor(
    private connectionService: ConnectionService,
    private profileService: ProfileService,
    private authService: AuthService,
  ) {}

  ngOnInit(): void {
    this.meId = this.authService.getProfile()?.id ?? 0;
    this.profileService.getProfiles().subscribe((data) => (this.profiles = data));
    this.loadConnections();
  }

  // Gets all connections and keeps only the ones the logged-in user is part of.
  // (No "connections for one person" endpoint yet – step 4 of the plan.)
  loadConnections(): void {
    this.connectionService.getConnections().subscribe((data) => {
      this.connections = data.filter((c) => c.requesterId === this.meId || c.addresseeId === this.meId);
    });
  }

  // Requests other people sent to me, waiting for my answer
  requestsForMe(): Connection[] {
    return this.connections.filter((c) => c.status === 'pending' && c.addresseeId === this.meId);
  }

  // Requests I sent, waiting for the other person
  requestsISent(): Connection[] {
    return this.connections.filter((c) => c.status === 'pending' && c.requesterId === this.meId);
  }

  friends(): Connection[] {
    return this.connections.filter((c) => c.status === 'accepted');
  }

  // The other person in a connection (I can be either the requester or the addressee)
  otherPerson(c: Connection): Profile | undefined {
    const otherId = c.requesterId === this.meId ? c.addresseeId : c.requesterId;
    return this.profiles.find((p) => p.id === otherId);
  }

  // Old messages from the "Add a friend" form disappear as soon as the user does something else
  clearMessages(): void {
    this.errorMessage = '';
    this.successMessage = '';
  }

  sendRequest(form: NgForm): void {
    this.clearMessages();
    // Find the person by email, ignoring capital letters (emails are unique in the database)
    const person = this.profiles.find((p) => p.email.toLowerCase() === this.email.trim().toLowerCase());
    if (!person) {
      this.errorMessage = `Nobody has the email ${this.email}. Check the spelling.`;
      return;
    }
    if (person.id === this.meId) {
      this.errorMessage = "That's your own email. Try a friend's.";
      return;
    }
    this.connectionService.sendRequest(this.meId, person.id).subscribe({
      next: () => {
        this.successMessage = `Friend request sent to ${person.fullName}.`;
        form.resetForm(); // empty the field and its touched/dirty state, so no error shows
        this.loadConnections();
      },
      error: (err: HttpErrorResponse) => this.showError(err, person),
    });
  }

  accept(c: Connection): void {
    this.clearMessages();
    this.connectionService.accept(c).subscribe(() => this.loadConnections());
  }

  // Decline a request, cancel one I sent, or remove a friend – all delete the connection
  remove(c: Connection, question: string): void {
    this.clearMessages();
    if (confirm(question)) {
      this.connectionService.deleteConnection(c.id).subscribe(() => this.loadConnections());
    }
  }

  // The database allows only one connection per pair of people, in either direction (uk_connection_pair).
  // The API turns that into a 400, which we explain in friendly words.
  showError(err: HttpErrorResponse, person: Profile): void {
    const reason = typeof err.error === 'string' ? err.error : '';
    if (reason.includes('uk_connection_pair')) {
      this.errorMessage = `You and ${person.fullName} are already friends, or there is already a request between you.`;
    } else {
      this.errorMessage = `The request couldn't be sent. ${reason || 'Please try again.'}`;
    }
  }
}
