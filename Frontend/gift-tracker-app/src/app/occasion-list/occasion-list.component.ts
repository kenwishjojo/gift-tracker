import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Occasion, OccasionInvite, canSee, recipientNameOf } from '../model/occasion';
import { Profile } from '../model/profile';
import { Gift } from '../model/gift';
import { OccasionService } from '../services/occasion.service';
import { ProfileService } from '../services/profile.service';
import { GiftService } from '../services/gift.service';
import { AuthService } from '../services/auth.service';
import { GiftGroupService } from '../services/gift-group.service';
import { InviteService } from '../services/invite.service';
import { GiftGroup, GroupMember, giftIdsSplitBy } from '../model/gift-group';
import { OccasionCardComponent } from '../occasion-card/occasion-card.component';

// Page: the occasions the logged-in user can see, the next one first.
// You see an occasion when you made it or are invited to it – never when it is for you (canSee in model/occasion.ts).
@Component({
  selector: 'app-occasion-list',
  imports: [RouterLink, OccasionCardComponent],
  templateUrl: './occasion-list.component.html',
  styleUrl: './occasion-list.component.css',
})
export class OccasionListComponent implements OnInit {
  occasions: Occasion[] = []; // all occasions from the API – upcoming() and past() pick the visible ones
  invites: OccasionInvite[] = []; // who is invited where, to decide what the user can see
  profiles: Profile[] = []; // to show the recipient's name on each card
  gifts: Gift[] = []; // to show the gift status on each card
  meId = 0; // the logged-in user, passed to each card for "Your share"
  groups: GiftGroup[] = []; // who splits which gift, for "Your share" on the cards
  members: GroupMember[] = [];

  constructor(
    private occasionService: OccasionService,
    private profileService: ProfileService,
    private giftService: GiftService,
    private authService: AuthService,
    private giftGroupService: GiftGroupService,
    private inviteService: InviteService,
  ) {}

  ngOnInit(): void {
    this.meId = this.authService.getProfile()?.id ?? 0;
    // Dates are text like "2026-12-03", so sorting the text also sorts by date
    this.occasionService
      .getOccasions()
      .subscribe((data) => (this.occasions = data.sort((a, b) => a.eventDate.localeCompare(b.eventDate))));
    this.inviteService.getInvites().subscribe((data) => (this.invites = data));
    this.profileService.getProfiles().subscribe((data) => (this.profiles = data));
    this.giftService.getGifts().subscribe((data) => (this.gifts = data));
    this.giftGroupService.getGroups().subscribe((data) => (this.groups = data));
    this.giftGroupService.getMembers().subscribe((data) => (this.members = data));
  }

  // Only the occasions this user may see (made by them or invited to, never for them)
  visible(): Occasion[] {
    return this.occasions.filter((o) => canSee(o, this.meId, this.invites));
  }

  upcoming(): Occasion[] {
    const today = this.todayAsText();
    return this.visible().filter((o) => o.eventDate >= today);
  }

  past(): Occasion[] {
    const today = this.todayAsText();
    return this.visible().filter((o) => o.eventDate < today).reverse(); // newest first
  }

  recipientName(occasion: Occasion): string {
    return recipientNameOf(occasion, this.profiles);
  }

  // The gifts for one occasion (shown as status and "Your share" on its card)
  giftsFor(occasion: Occasion): Gift[] {
    return this.gifts.filter((g) => g.occasionId === occasion.id);
  }

  // Today's date as "YYYY-MM-DD" (same format as eventDate)
  private todayAsText(): string {
    const now = new Date();
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const day = String(now.getDate()).padStart(2, '0');
    return `${now.getFullYear()}-${month}-${day}`;
  }

  // The gifts a friend splits with the logged-in user, passed to each card
  mySplitGiftIds(): number[] {
    return giftIdsSplitBy(this.meId, this.groups, this.members);
  }
}
