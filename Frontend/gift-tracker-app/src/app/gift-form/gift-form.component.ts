import { Component, Input, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import { DecimalPipe, TitleCasePipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { Gift, GIFT_STATUSES, shareOf } from '../model/gift';
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
  imports: [FormsModule, RouterLink, DecimalPipe, TitleCasePipe, MatIconModule],
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

  // Bound to the "Split the price" checkbox with [(ngModel)]. Not sent to the API on its own:
  // when it is off we save splitCount = 1 (see saveGift).
  isSplit = false;

  // Constructor injection (dependency injection, Lecture 6): Angular creates the services
  // and hands them to the component, so every page shares the same service objects
  constructor(
    private giftService: GiftService,
    private occasionService: OccasionService,
    private profileService: ProfileService,
    private authService: AuthService,
    private router: Router,
  ) {}

  // ngOnInit runs once when the component starts – the place to fetch data (Lecture 6)
  ngOnInit(): void {
    this.me = this.authService.getProfile();
    if (this.id) {
      // Edit: load the gift, then its occasion.
      // getGift returns an Observable; the code in subscribe runs when the API answers (Lecture 6)
      this.giftService.getGift(Number(this.id)).subscribe((gift) => {
        this.gift = gift;
        this.isSplit = gift.splitCount > 1; // tick the checkbox if the gift is already split
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
        splitCount: 1, // not split until the user ticks "Split the price"
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

  // When the checkbox is ticked we start at 2 people, the smallest real split
  onSplitChange(): void {
    if (this.gift) {
      this.gift.splitCount = this.isSplit ? Math.max(this.gift.splitCount, 2) : 1;
    }
  }

  // Each person's share, shown live under the price while the user types (null if no price yet)
  share(): number | null {
    return this.gift ? shareOf(this.gift) : null;
  }

  saveGift(): void {
    if (!this.gift) {
      return;
    }
    this.errorMessage = '';
    if (!this.isSplit) {
      this.gift.splitCount = 1; // not split: one person pays it all
    }
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
