import { Component, Input, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import { TitleCasePipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { Occasion, OCCASION_TYPES } from '../model/occasion';
import { Profile } from '../model/profile';
import { OccasionService } from '../services/occasion.service';
import { ProfileService } from '../services/profile.service';
import { AuthService } from '../services/auth.service';

// Page: add a new occasion (/occasion/new) or edit one (/occasion/3/edit).
// Same pattern as the gift form: a template-driven form where the fields are two-way bound to
// this.occasion with [(ngModel)], and the validation rules are written in the HTML (Lecture 7).
@Component({
  selector: 'app-occasion-form',
  imports: [FormsModule, RouterLink, TitleCasePipe, MatIconModule],
  templateUrl: './occasion-form.component.html',
  styleUrl: './occasion-form.component.css',
})
export class OccasionFormComponent implements OnInit {
  // Filled from the URL when editing, e.g. /occasion/3/edit (withComponentInputBinding, Lecture 7).
  // When adding (/occasion/new) there is no id, so it stays undefined.
  @Input() id?: string;

  occasion?: Occasion;
  profiles: Profile[] = []; // the options in the "Who is it for?" list
  types = OCCASION_TYPES; // the options in the type list
  errorMessage = '';

  // Constructor injection (dependency injection, Lecture 6)
  constructor(
    private occasionService: OccasionService,
    private profileService: ProfileService,
    private authService: AuthService,
    private router: Router,
  ) {}

  // Runs once when the page opens (Lecture 6)
  ngOnInit(): void {
    this.profileService.getProfiles().subscribe((data) => (this.profiles = data));

    if (this.id) {
      // Edit: load the occasion from the API
      this.occasionService.getOccasion(Number(this.id)).subscribe((data) => (this.occasion = data));
    } else {
      // Add: start with an empty occasion. id 0 means "new" – the database gives it a real id.
      // The logged-in user is the one who creates it.
      const me = this.authService.getProfile();
      this.occasion = {
        id: 0,
        createdById: me ? me.id : 0,
        recipientId: 0, // 0 = nobody chosen yet, so the "required" check below can catch it
        name: '',
        eventDate: '',
        type: 'birthday',
        notes: null,
      };
    }
  }

  saveOccasion(): void {
    if (!this.occasion) {
      return;
    }
    this.errorMessage = '';
    if (this.occasion.id === 0) {
      // POST answers with the new occasion, so we can open its page straight away
      this.occasionService.addOccasion(this.occasion).subscribe({
        next: (saved) => this.router.navigate(['/occasion', saved.id]),
        error: (err) => this.showError(err),
      });
    } else {
      this.occasionService.updateOccasion(this.occasion).subscribe({
        next: () => this.router.navigate(['/occasion', this.occasion!.id]),
        error: (err) => this.showError(err),
      });
    }
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
