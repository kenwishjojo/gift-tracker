import { Component, Input, OnInit } from '@angular/core';
import { DatePipe, DecimalPipe, TitleCasePipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { Occasion, OccasionInvite, canSee, isPast, recipientNameOf } from '../model/occasion';
import { Gift, shareOf, totalShareFor } from '../model/gift';
import { Profile } from '../model/profile';
import { GiftGroup, GroupMember, giftIdsSplitBy, joinNames } from '../model/gift-group';
import { OccasionService } from '../services/occasion.service';
import { GiftService } from '../services/gift.service';
import { ProfileService } from '../services/profile.service';
import { AuthService } from '../services/auth.service';
import { GiftGroupService } from '../services/gift-group.service';
import { InviteService } from '../services/invite.service';
import { AvatarComponent } from '../avatar/avatar.component';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog.component';

// Page: one occasion and its gifts.
// - Only shown to people who may see it (the creator and invited friends – never the person it is for).
// - Past occasions are locked: no editing and no gift changes, but the creator can still delete it.
// - Every delete first opens the warning box (ConfirmDialogComponent).
@Component({
  selector: 'app-occasion-detail',
  imports: [RouterLink, DatePipe, DecimalPipe, TitleCasePipe, MatIconModule, AvatarComponent, ConfirmDialogComponent],
  templateUrl: './occasion-detail.component.html',
  styleUrl: './occasion-detail.component.css',
})
export class OccasionDetailComponent implements OnInit {
  // Filled from the URL, e.g. /occasion/3 (withComponentInputBinding in app.config.ts).
  // Route parameters are always text, so we use Number(this.id) when we need a number.
  @Input() id!: string;

  occasion?: Occasion;
  gifts: Gift[] = [];
  groups: GiftGroup[] = []; // the split groups, to see who splits which gift
  members: GroupMember[] = [];
  profiles: Profile[] = []; // to show the recipient's and buyers' names
  invites: OccasionInvite[] = []; // to check that the user may see this occasion, and to list who is invited
  meId = 0;

  // Which warning box is open (the template shows it with @if)
  showDeleteOccasion = false;
  giftToDelete?: Gift;

  // Services come in through constructor injection (dependency injection, Lecture 6)
  constructor(
    private occasionService: OccasionService,
    private giftService: GiftService,
    private profileService: ProfileService,
    private authService: AuthService, // to know who "you" are
    private giftGroupService: GiftGroupService, // who splits which gift
    private inviteService: InviteService, // who is invited
    private router: Router, // to go back to the list after deleting the occasion
  ) {}

  // Runs when the page opens: fetch the data from the API through the services (Lecture 6)
  ngOnInit(): void {
    this.meId = this.authService.getProfile()?.id ?? 0;
    const occasionId = Number(this.id);
    this.occasionService.getOccasion(occasionId).subscribe((data) => (this.occasion = data));
    this.inviteService.getInvites().subscribe((data) => (this.invites = data));
    this.giftService.getGiftsForOccasion(occasionId).subscribe((gifts) => (this.gifts = gifts));
    this.giftGroupService.getGroups().subscribe((data) => (this.groups = data));
    this.giftGroupService.getMembers().subscribe((data) => (this.members = data));
    this.profileService.getProfiles().subscribe((data) => (this.profiles = data));
  }

  // ---- Who may do what ----

  // The creator and invited friends see the page; the person it is for never does (frontend rule)
  mayView(): boolean {
    return !!this.occasion && canSee(this.occasion, this.meId, this.invites);
  }

  // Only the creator edits or deletes the occasion itself
  isCreator(): boolean {
    return !!this.occasion && this.occasion.createdById === this.meId;
  }

  // Past occasions are locked (the API refuses changes too)
  isLocked(): boolean {
    return !!this.occasion && isPast(this.occasion);
  }

  // ---- Names ----

  profileName(id: number): string {
    const profile = this.profiles.find((p) => p.id === id);
    return profile ? profile.fullName : '';
  }

  recipientName(): string {
    return this.occasion ? recipientNameOf(this.occasion, this.profiles) : '';
  }

  // The invited friends, e.g. "Sofie Larsen and Lars Jensen"
  invitedNames(): string {
    const names = this.invites
      .filter((i) => i.occasionId === this.occasion?.id)
      .map((i) => this.profileName(i.profileId));
    return joinNames(names);
  }

  // ---- Prices (the calculations live in model/gift.ts, shared with the occasion card) ----

  share(gift: Gift): number | null {
    return shareOf(gift);
  }

  // The logged-in user's share of all gifts on this occasion (the total was taken away on purpose)
  myShare(): number {
    return totalShareFor(this.gifts, this.meId, this.mySplitGiftIds());
  }

  // The gifts the logged-in user is splitting with friends
  mySplitGiftIds(): number[] {
    return giftIdsSplitBy(this.meId, this.groups, this.members);
  }

  // True when the logged-in user pays for this gift: as the buyer or as a friend splitting it
  iPayFor(gift: Gift): boolean {
    return gift.buyerId === this.meId || this.mySplitGiftIds().includes(gift.id);
  }

  // Everyone in the gift's group except the buyer, e.g. "Lars Jensen and Mette Jensen"
  splitWith(gift: Gift): string {
    const group = this.groups.find((g) => g.giftId === gift.id);
    if (!group) {
      return '';
    }
    const names = this.members
      .filter((m) => m.groupId === group.id && m.profileId !== gift.buyerId)
      .map((m) => this.profileName(m.profileId));
    return joinNames(names);
  }

  isBoughtOrGiven(gift: Gift): boolean {
    return gift.status === 'bought' || gift.status === 'given';
  }

  // ---- Deleting (always through the warning box) ----

  // Called by the warning box's (confirmed) event. Also deletes the gifts and invites (cascade).
  deleteOccasion(): void {
    if (this.occasion) {
      this.occasionService.deleteOccasion(this.occasion.id).subscribe(() => this.router.navigate(['/occasions']));
    }
  }

  // Called by the warning box's (confirmed) event
  deleteGift(): void {
    const gift = this.giftToDelete;
    if (!gift) {
      return;
    }
    this.giftService.deleteGift(gift.id).subscribe(() => {
      // Remove it from the list on the page, so we don't have to load all gifts again
      this.gifts = this.gifts.filter((g) => g.id !== gift.id);
      this.giftToDelete = undefined;
    });
  }
}
