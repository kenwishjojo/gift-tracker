import { Component, Input, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import { TitleCasePipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { Gift, GIFT_STATUSES } from '../model/gift';
import { Occasion } from '../model/occasion';
import { Profile } from '../model/profile';
import { GiftService } from '../services/gift.service';
import { OccasionService } from '../services/occasion.service';
import { ProfileService } from '../services/profile.service';
import { AuthService } from '../services/auth.service';

// Page: add a new gift (/occasion/3/gift/new) or edit one (/gift/5/edit).
// Template-driven form: the fields are two-way bound to this.gift with [(ngModel)],
// and the validation rules (required, min) are written in the HTML (Lecture 7).
@Component({
  selector: 'app-gift-form',
  imports: [FormsModule, RouterLink, TitleCasePipe, MatIconModule],
  templateUrl: './gift-form.component.html',
  styleUrl: './gift-form.component.css',
})
export class GiftFormComponent implements OnInit {
  // Only one of them is set, depending on the URL (withComponentInputBinding in app.config.ts)
  @Input() id?: string; // edit: the gift's id
  @Input() occasionId?: string; // add: the occasion the new gift belongs to

  gift?: Gift;
  occasion?: Occasion;
  recipient?: Profile;
  statuses = GIFT_STATUSES; // the options in the status list
  me: Profile | null = null; // the logged-in user
  errorMessage = '';

  constructor(
    private giftService: GiftService,
    private occasionService: OccasionService,
    private profileService: ProfileService,
    private authService: AuthService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.me = this.authService.getProfile();
    if (this.id) {
      // Edit: load the gift, then its occasion
      this.giftService.getGift(Number(this.id)).subscribe((gift) => {
        this.gift = gift;
        this.loadOccasion(gift.occasionId);
      });
    } else {
      // Add: start with an empty gift. id 0 means "new" – the database gives it a real id.
      // The logged-in user is the buyer.
      this.gift = {
        id: 0,
        occasionId: Number(this.occasionId),
        buyerId: this.me ? this.me.id : 0,
        name: '',
        description: null,
        price: null,
        link: null,
        status: 'idea',
      };
      this.loadOccasion(this.gift.occasionId);
    }
  }

  // The occasion and its recipient are shown at the top of the form
  loadOccasion(occasionId: number): void {
    this.occasionService.getOccasion(occasionId).subscribe((occasion) => {
      this.occasion = occasion;
      this.profileService.getProfile(occasion.recipientId).subscribe((p) => (this.recipient = p));
    });
  }

  // Rule from the API: the buyer can't be the occasion's recipient
  // (you can't add gifts to your own birthday)
  buyerIsRecipient(): boolean {
    return !!this.gift && !!this.occasion && this.gift.buyerId === this.occasion.recipientId;
  }

  saveGift(): void {
    if (!this.gift) {
      return;
    }
    this.errorMessage = '';
    if (this.gift.id === 0) {
      this.giftService.addGift(this.gift).subscribe({
        next: () => this.goBack(),
        error: (err) => this.showError(err),
      });
    } else {
      this.giftService.updateGift(this.gift).subscribe({
        next: () => this.goBack(),
        error: (err) => this.showError(err),
      });
    }
  }

  // Back to the occasion the gift belongs to
  goBack(): void {
    this.router.navigate(['/occasion', this.gift!.occasionId]);
  }

  // The backend answers 400 with a text message when a rule is broken
  showError(err: HttpErrorResponse): void {
    const reason = typeof err.error === 'string' ? err.error : 'Please try again.';
    this.errorMessage = `The gift couldn't be saved. ${reason}`;
  }
}
