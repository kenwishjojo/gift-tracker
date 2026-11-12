import { Component, Input, OnInit } from '@angular/core';
import { DatePipe, DecimalPipe, TitleCasePipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { Occasion } from '../model/occasion';
import { Gift, shareOf, totalPrice, totalShareFor } from '../model/gift';
import { Profile } from '../model/profile';
import { OccasionService } from '../services/occasion.service';
import { GiftService } from '../services/gift.service';
import { ProfileService } from '../services/profile.service';
import { AuthService } from '../services/auth.service';
import { AvatarComponent } from '../avatar/avatar.component';

// Page: one occasion and its gifts
@Component({
  selector: 'app-occasion-detail',
  imports: [RouterLink, DatePipe, DecimalPipe, TitleCasePipe, MatIconModule, AvatarComponent],
  templateUrl: './occasion-detail.component.html',
  styleUrl: './occasion-detail.component.css',
})
export class OccasionDetailComponent implements OnInit {
  // Filled from the URL, e.g. /occasion/3 (withComponentInputBinding in app.config.ts).
  // Route parameters are always text, so we use Number(this.id) when we need a number.
  @Input() id!: string;

  occasion?: Occasion;
  gifts: Gift[] = [];
  profiles: Profile[] = []; // to show the recipient's and buyers' names

  // Services come in through constructor injection (dependency injection, Lecture 6)
  constructor(
    private occasionService: OccasionService,
    private giftService: GiftService,
    private profileService: ProfileService,
    private authService: AuthService, // to know who "you" are in "Your share"
    private router: Router, // to go back to the list after deleting the occasion
  ) {}

  // Runs when the page opens: fetch the data from the API through the services (Lecture 6)
  ngOnInit(): void {
    const occasionId = Number(this.id);
    this.occasionService.getOccasion(occasionId).subscribe((data) => {
      this.occasion = data;
      // Only load the gifts when the occasion isn't for the logged-in user – the gifts are a surprise.
      // (This is a rule in the frontend only: the API itself doesn't know who is asking, see README.)
      if (!this.isForMe()) {
        this.giftService.getGiftsForOccasion(occasionId).subscribe((gifts) => (this.gifts = gifts));
      }
    });
    this.profileService.getProfiles().subscribe((data) => (this.profiles = data));
  }

  profileName(id: number): string {
    const profile = this.profiles.find((p) => p.id === id);
    return profile ? profile.fullName : '';
  }

  // The price calculations live in model/gift.ts, so this page and the occasion card use the same code
  share(gift: Gift): number | null {
    return shareOf(gift);
  }

  total(): number {
    return totalPrice(this.gifts);
  }

  myShare(): number {
    const me = this.authService.getProfile();
    return me ? totalShareFor(this.gifts, me.id) : 0;
  }

  // True when the logged-in user is the one getting the gifts
  isForMe(): boolean {
    const me = this.authService.getProfile();
    return !!me && !!this.occasion && this.occasion.recipientId === me.id;
  }

  isBoughtOrGiven(gift: Gift): boolean {
    return gift.status === 'bought' || gift.status === 'given';
  }

  // Deleting an occasion also deletes its gifts (on delete cascade in the database), so we ask first
  deleteOccasion(): void {
    if (this.occasion && confirm(`Delete "${this.occasion.name}" and all its gifts?`)) {
      this.occasionService.deleteOccasion(this.occasion.id).subscribe(() => this.router.navigate(['/occasions']));
    }
  }

  deleteGift(gift: Gift): void {
    if (confirm(`Delete "${gift.name}"?`)) {
      this.giftService.deleteGift(gift.id).subscribe(() => {
        // Remove it from the list on the page, so we don't have to load all gifts again
        this.gifts = this.gifts.filter((g) => g.id !== gift.id);
      });
    }
  }
}
