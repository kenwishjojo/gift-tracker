import { Component, Input, OnInit } from '@angular/core';
import { DatePipe, DecimalPipe, TitleCasePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { Occasion } from '../model/occasion';
import { Gift } from '../model/gift';
import { Profile } from '../model/profile';
import { OccasionService } from '../services/occasion.service';
import { GiftService } from '../services/gift.service';
import { ProfileService } from '../services/profile.service';
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

  constructor(
    private occasionService: OccasionService,
    private giftService: GiftService,
    private profileService: ProfileService,
  ) {}

  ngOnInit(): void {
    const occasionId = Number(this.id);
    this.occasionService.getOccasion(occasionId).subscribe((data) => (this.occasion = data));
    this.giftService.getGiftsForOccasion(occasionId).subscribe((data) => (this.gifts = data));
    this.profileService.getProfiles().subscribe((data) => (this.profiles = data));
  }

  profileName(id: number): string {
    const profile = this.profiles.find((p) => p.id === id);
    return profile ? profile.fullName : '';
  }

  isBoughtOrGiven(gift: Gift): boolean {
    return gift.status === 'bought' || gift.status === 'given';
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
