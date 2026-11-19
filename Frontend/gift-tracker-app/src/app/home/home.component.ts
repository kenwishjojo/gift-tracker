import { Component, OnInit } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { Profile } from '../model/profile';
import { Gift, shareOf } from '../model/gift';
import { Occasion } from '../model/occasion';
import { GiftGroup, GroupMember, giftIdsSplitBy, joinNames } from '../model/gift-group';
import { AuthService } from '../services/auth.service';
import { GiftService } from '../services/gift.service';
import { OccasionService } from '../services/occasion.service';
import { ProfileService } from '../services/profile.service';
import { GiftGroupService } from '../services/gift-group.service';

// The landing page after login, with a feed of the gifts you are splitting with friends.
// When someone ticks you as a friend to split a gift with, that gift and its occasion show up here.
@Component({
  selector: 'app-home',
  imports: [RouterLink, DatePipe, DecimalPipe],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
})
export class HomeComponent implements OnInit {
  profile: Profile | null = null;

  // Everything the feed needs. Each list is loaded with its own call and the feed is put together
  // in splitGifts() – so it updates by itself as each answer arrives.
  gifts: Gift[] = [];
  occasions: Occasion[] = [];
  profiles: Profile[] = [];
  groups: GiftGroup[] = [];
  members: GroupMember[] = [];

  // Constructor injection (dependency injection, Lecture 6)
  constructor(
    private authService: AuthService,
    private giftService: GiftService,
    private occasionService: OccasionService,
    private profileService: ProfileService,
    private giftGroupService: GiftGroupService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.profile = this.authService.getProfile();
    // Nobody logged in: send them to the login page
    if (!this.profile) {
      this.router.navigate(['/login']);
      return;
    }
    this.giftService.getGifts().subscribe((data) => (this.gifts = data));
    this.occasionService.getOccasions().subscribe((data) => (this.occasions = data));
    this.profileService.getProfiles().subscribe((data) => (this.profiles = data));
    this.giftGroupService.getGroups().subscribe((data) => (this.groups = data));
    this.giftGroupService.getMembers().subscribe((data) => (this.members = data));
  }

  // The gifts I am splitting (as buyer/owner or as a friend) for upcoming occasions, soonest first.
  // Never gifts for my own occasions – those are a surprise.
  splitGifts(): Gift[] {
    if (!this.profile) {
      return [];
    }
    const ids = giftIdsSplitBy(this.profile.id, this.groups, this.members);
    const meId = this.profile.id;
    // Today as "YYYY-MM-DD" (the same format as eventDate), in local time
    const now = new Date();
    const today = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`;
    return this.gifts
      .filter((g) => ids.includes(g.id) && this.occasionOf(g)?.recipientId !== meId)
      .filter((g) => (this.occasionOf(g)?.eventDate ?? '') >= today)
      .sort((a, b) => (this.occasionOf(a)?.eventDate ?? '').localeCompare(this.occasionOf(b)?.eventDate ?? ''));
  }

  occasionOf(gift: Gift): Occasion | undefined {
    return this.occasions.find((o) => o.id === gift.occasionId);
  }

  nameOf(profileId: number | undefined): string {
    return this.profiles.find((p) => p.id === profileId)?.fullName ?? '';
  }

  // The other people in the gift's group, e.g. "Lars Jensen and Mette Jensen"
  splitWith(gift: Gift): string {
    const group = this.groups.find((g) => g.giftId === gift.id);
    if (!group || !this.profile) {
      return '';
    }
    const names = this.members
      .filter((m) => m.groupId === group.id && m.profileId !== this.profile!.id)
      .map((m) => this.nameOf(m.profileId));
    return joinNames(names);
  }

  share(gift: Gift): number | null {
    return shareOf(gift);
  }
}
