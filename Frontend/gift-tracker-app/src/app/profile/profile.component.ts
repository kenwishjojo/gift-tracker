import { Component, Input, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Profile } from '../model/profile';
import { Occasion } from '../model/occasion';
import { Gift } from '../model/gift';
import { ProfileService } from '../services/profile.service';
import { OccasionService } from '../services/occasion.service';
import { GiftService } from '../services/gift.service';
import { AuthService } from '../services/auth.service';
import { GiftGroupService } from '../services/gift-group.service';
import { GiftGroup, GroupMember, giftIdsSplitBy } from '../model/gift-group';
import { AvatarComponent } from '../avatar/avatar.component';
import { OccasionCardComponent } from '../occasion-card/occasion-card.component';

// Page: one profile and the occasions where this person gets gifts
@Component({
  selector: 'app-profile',
  imports: [DatePipe, AvatarComponent, OccasionCardComponent],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css',
})
export class ProfileComponent implements OnInit {
  // Filled from the URL, e.g. /profile/1 (withComponentInputBinding in app.config.ts)
  @Input() id!: string;

  profile?: Profile;
  occasions: Occasion[] = [];
  gifts: Gift[] = [];
  meId = 0; // the logged-in user, passed to each card
  groups: GiftGroup[] = []; // who splits which gift, for "Your share" on the cards
  members: GroupMember[] = [];

  constructor(
    private profileService: ProfileService,
    private occasionService: OccasionService,
    private giftService: GiftService,
    private authService: AuthService,
    private giftGroupService: GiftGroupService,
  ) {}

  ngOnInit(): void {
    const profileId = Number(this.id);
    this.meId = this.authService.getProfile()?.id ?? 0;
    this.profileService.getProfile(profileId).subscribe((data) => (this.profile = data));
    this.occasionService
      .getOccasionsForRecipient(profileId)
      .subscribe((data) => (this.occasions = data.sort((a, b) => b.eventDate.localeCompare(a.eventDate)))); // newest first
    this.giftService.getGifts().subscribe((data) => (this.gifts = data));
    this.giftGroupService.getGroups().subscribe((data) => (this.groups = data));
    this.giftGroupService.getMembers().subscribe((data) => (this.members = data));
  }

  // On your own profile every occasion is yours, so no gifts are passed to the cards (they are a surprise)
  giftsFor(occasion: Occasion): Gift[] {
    if (occasion.recipientId === this.meId) {
      return [];
    }
    return this.gifts.filter((g) => g.occasionId === occasion.id);
  }

  // The gifts a friend splits with the logged-in user, passed to each card
  mySplitGiftIds(): number[] {
    return giftIdsSplitBy(this.meId, this.groups, this.members);
  }
}
