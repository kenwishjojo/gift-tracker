import { Component, Input, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import { TitleCasePipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { Occasion, OccasionInvite, OCCASION_TYPES, isPast } from '../model/occasion';
import { Profile } from '../model/profile';
import { Connection } from '../model/connection';
import { OccasionService } from '../services/occasion.service';
import { ProfileService } from '../services/profile.service';
import { AuthService } from '../services/auth.service';
import { ConnectionService } from '../services/connection.service';
import { InviteService } from '../services/invite.service';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog.component';

// Page: add a new occasion (/occasion/new) or edit one (/occasion/3/edit).
// Same pattern as the gift form: a template-driven form where the fields are two-way bound to
// this.occasion with [(ngModel)], and the validation rules are written in the HTML (Lecture 7).
// New in this version:
// - "Who is it for?" lists only your friends, plus "Someone without an account" (then you type a name)
// - you tick which friends to invite – only invited friends (and you) can see the occasion
// - saving changes to an existing occasion first asks in the warning box
// - only the creator can edit, and a past occasion can't be edited at all
@Component({
  selector: 'app-occasion-form',
  imports: [FormsModule, RouterLink, TitleCasePipe, MatIconModule, ConfirmDialogComponent],
  templateUrl: './occasion-form.component.html',
  styleUrl: './occasion-form.component.css',
})
export class OccasionFormComponent implements OnInit {
  // Filled from the URL when editing, e.g. /occasion/3/edit (withComponentInputBinding, Lecture 7).
  // When adding (/occasion/new) there is no id, so it stays undefined.
  @Input() id?: string;

  occasion?: Occasion;
  profiles: Profile[] = [];
  connections: Connection[] = []; // to find the logged-in user's friends
  types = OCCASION_TYPES; // the options in the type list
  meId = 0;
  errorMessage = '';

  // "Who is it for?": a friend's id, 'other' for someone without an account, or 0 = not chosen yet.
  // Two-way bound to the select with [(ngModel)]. Copied into occasion.recipientId/recipientName on save.
  recipientChoice: number | 'other' = 0;
  otherName = ''; // the typed name when recipientChoice is 'other'

  // The friends ticked in "Invite friends" (sent to the API in one call when saving)
  selectedInviteIds: number[] = [];

  showSaveConfirm = false; // the warning box before saving changes to an existing occasion

  // Constructor injection (dependency injection, Lecture 6)
  constructor(
    private occasionService: OccasionService,
    private profileService: ProfileService,
    private authService: AuthService,
    private connectionService: ConnectionService,
    private inviteService: InviteService,
    private router: Router,
  ) {}

  // Runs once when the page opens (Lecture 6)
  ngOnInit(): void {
    this.meId = this.authService.getProfile()?.id ?? 0;
    this.profileService.getProfiles().subscribe((data) => (this.profiles = data));
    this.connectionService.getConnections().subscribe((data) => (this.connections = data));

    if (this.id) {
      // Edit: load the occasion, then fill the "Who is it for?" choice and the ticked invites
      const occasionId = Number(this.id);
      this.occasionService.getOccasion(occasionId).subscribe((data) => {
        this.occasion = data;
        if (data.recipientId === null) {
          this.recipientChoice = 'other';
          this.otherName = data.recipientName ?? '';
        } else {
          this.recipientChoice = data.recipientId;
        }
      });
      this.inviteService.getInvites().subscribe((invites: OccasionInvite[]) => {
        this.selectedInviteIds = invites.filter((i) => i.occasionId === occasionId).map((i) => i.profileId);
      });
    } else {
      // Add: start with an empty occasion. id 0 means "new" – the database gives it a real id.
      // The logged-in user is the one who creates it.
      this.occasion = {
        id: 0,
        createdById: this.meId,
        recipientId: null,
        recipientName: null,
        name: '',
        eventDate: '',
        type: 'birthday',
        notes: null,
      };
    }
  }

  // ---- Who may edit ----

  isNew(): boolean {
    return !!this.occasion && this.occasion.id === 0;
  }

  // Only the creator edits an existing occasion
  isCreator(): boolean {
    return !!this.occasion && this.occasion.createdById === this.meId;
  }

  // A past occasion can't be edited (the API refuses it too)
  isLocked(): boolean {
    return !!this.occasion && !this.isNew() && isPast(this.occasion);
  }

  // ---- Friends ----

  // The ids of the logged-in user's friends: accepted connections, in either direction
  friendIds(): number[] {
    return this.connections
      .filter((c) => c.status === 'accepted' && (c.requesterId === this.meId || c.addresseeId === this.meId))
      .map((c) => (c.requesterId === this.meId ? c.addresseeId : c.requesterId));
  }

  // "Who is it for?" options: your friends (plus the current recipient when editing, so it still shows)
  recipientOptions(): Profile[] {
    const ids = this.friendIds();
    return this.profiles.filter((p) => ids.includes(p.id) || p.id === this.occasion?.recipientId);
  }

  // "Invite friends" options: your friends, but never the person the occasion is for (a surprise)
  inviteOptions(): Profile[] {
    const ids = this.friendIds();
    return this.profiles.filter(
      (p) => (ids.includes(p.id) || this.selectedInviteIds.includes(p.id)) && p.id !== this.recipientChoice,
    );
  }

  // When the recipient changes, they can't stay invited to their own occasion
  onRecipientChange(): void {
    this.selectedInviteIds = this.selectedInviteIds.filter((id) => id !== this.recipientChoice);
  }

  isInvited(profileId: number): boolean {
    return this.selectedInviteIds.includes(profileId);
  }

  // A friend's checkbox was clicked: add or remove them from the list
  toggleInvite(profileId: number): void {
    if (this.isInvited(profileId)) {
      this.selectedInviteIds = this.selectedInviteIds.filter((id) => id !== profileId);
    } else {
      this.selectedInviteIds = [...this.selectedInviteIds, profileId];
    }
  }

  // ---- Validation that needs code (the rest is in the HTML) ----

  noRecipient(): boolean {
    return this.recipientChoice === 0;
  }

  // An occasion is planned ahead: the date can't be before today
  dateInPast(): boolean {
    return !!this.occasion && this.occasion.eventDate !== '' && isPast(this.occasion);
  }

  // ---- Saving ----

  // The form's Save button. A new occasion is saved straight away; changes to an existing one are
  // saved only after the user says yes in the warning box.
  onSubmit(): void {
    if (this.isNew()) {
      this.saveOccasion();
    } else {
      this.showSaveConfirm = true;
    }
  }

  // Step 1: save the occasion (POST or PUT). Step 2: save the invite list for it (one PUT that
  // replaces the whole list). Step 3: open the occasion's page.
  saveOccasion(): void {
    if (!this.occasion) {
      return;
    }
    this.showSaveConfirm = false;
    this.errorMessage = '';
    // Copy "Who is it for?" into the two columns: exactly one of them is set
    if (this.recipientChoice === 'other') {
      this.occasion.recipientId = null;
      this.occasion.recipientName = this.otherName.trim();
    } else {
      this.occasion.recipientId = this.recipientChoice;
      this.occasion.recipientName = null;
    }

    if (this.isNew()) {
      this.occasionService.addOccasion(this.occasion).subscribe({
        next: (saved) => this.saveInvites(saved.id),
        error: (err) => this.showError(err),
      });
    } else {
      this.occasionService.updateOccasion(this.occasion).subscribe({
        next: () => this.saveInvites(this.occasion!.id),
        error: (err) => this.showError(err),
      });
    }
  }

  saveInvites(occasionId: number): void {
    this.inviteService.replaceInvites(occasionId, this.selectedInviteIds).subscribe({
      next: () => this.router.navigate(['/occasion', occasionId]),
      error: (err) => this.showError(err),
    });
  }

  // Where Cancel goes: back to the occasion when editing, back to the list when adding
  cancelLink(): (string | number)[] {
    return this.occasion && this.occasion.id !== 0 ? ['/occasion', this.occasion.id] : ['/occasions'];
  }

  // The backend answers 400 with a text message when a rule is broken.
  // The most likely one here: the same person already has an occasion with this name on this date
  // (the unique key on recipientid, name and eventdate in the database).
  showError(err: HttpErrorResponse): void {
    const reason = typeof err.error === 'string' ? err.error : 'Please try again.';
    if (reason.includes('uk_occasion')) {
      this.errorMessage = 'This person already has an occasion with that name on that date.';
    } else {
      this.errorMessage = `The occasion couldn't be saved. ${reason}`;
    }
  }
}
