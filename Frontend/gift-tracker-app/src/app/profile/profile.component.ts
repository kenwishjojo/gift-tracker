import { Component, Input, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { Profile } from '../model/profile';
import { Occasion, OccasionInvite, canSee } from '../model/occasion';
import { Gift } from '../model/gift';
import { Connection } from '../model/connection';
import { ProfileService } from '../services/profile.service';
import { OccasionService } from '../services/occasion.service';
import { GiftService } from '../services/gift.service';
import { AuthService } from '../services/auth.service';
import { GiftGroupService } from '../services/gift-group.service';
import { InviteService } from '../services/invite.service';
import { ConnectionService } from '../services/connection.service';
import { GiftGroup, GroupMember, giftIdsSplitBy } from '../model/gift-group';
import { AvatarComponent } from '../avatar/avatar.component';
import { OccasionCardComponent } from '../occasion-card/occasion-card.component';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog.component';

// Page: one profile.
// - Someone else's profile: their details and the occasions for them that you can see.
// - Your own profile: your details, a link to the terms, and "Delete my account" (with a warning box).
//   Your own occasions are never listed – the gifts are a surprise.
@Component({
  selector: 'app-profile',
  imports: [DatePipe, RouterLink, AvatarComponent, OccasionCardComponent, ConfirmDialogComponent],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css',
})
export class ProfileComponent implements OnInit {
  // Filled from the URL, e.g. /profile/1 (withComponentInputBinding in app.config.ts)
  @Input() id!: string;

  profile?: Profile;
  occasions: Occasion[] = []; // all occasions; forThisPerson() picks the ones to show
  invites: OccasionInvite[] = []; // to decide which occasions the logged-in user can see
  connections: Connection[] = []; // to count the friendships a deleted account takes with it
  gifts: Gift[] = [];
  meId = 0; // the logged-in user, passed to each card
  groups: GiftGroup[] = []; // who splits which gift, for "Your share" on the cards
  members: GroupMember[] = [];

  showDeleteConfirm = false; // the warning box before deleting the account

  // Constructor injection (dependency injection, Lecture 6)
  constructor(
    private profileService: ProfileService,
    private occasionService: OccasionService,
    private giftService: GiftService,
    private authService: AuthService,
    private giftGroupService: GiftGroupService,
    private inviteService: InviteService,
    private connectionService: ConnectionService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    const profileId = Number(this.id);
    this.meId = this.authService.getProfile()?.id ?? 0;
    this.profileService.getProfile(profileId).subscribe((data) => (this.profile = data));
    this.occasionService
      .getOccasions()
      .subscribe((data) => (this.occasions = data.sort((a, b) => b.eventDate.localeCompare(a.eventDate)))); // newest first
    this.inviteService.getInvites().subscribe((data) => (this.invites = data));
    this.connectionService.getConnections().subscribe((data) => (this.connections = data));
    this.giftService.getGifts().subscribe((data) => (this.gifts = data));
    this.giftGroupService.getGroups().subscribe((data) => (this.groups = data));
    this.giftGroupService.getMembers().subscribe((data) => (this.members = data));
  }

  isMyProfile(): boolean {
    return !!this.profile && this.profile.id === this.meId;
  }

  // The occasions for this person that the logged-in user may see (made by them or invited to).
  // On your own profile this is always empty: canSee is false for occasions that are for you.
  forThisPerson(): Occasion[] {
    return this.occasions.filter((o) => o.recipientId === this.profile?.id && canSee(o, this.meId, this.invites));
  }

  giftsFor(occasion: Occasion): Gift[] {
    return this.gifts.filter((g) => g.occasionId === occasion.id);
  }

  // The gifts a friend splits with the logged-in user, passed to each card
  mySplitGiftIds(): number[] {
    return giftIdsSplitBy(this.meId, this.groups, this.members);
  }

  // ---- Deleting the account ----

  // What the database deletes together with the profile (on delete cascade), shown in the warning box
  // so the user knows exactly what happens. Counted from the lists loaded above.
  deleteDetails(): string[] {
    const friends = this.connections.filter((c) => c.requesterId === this.meId || c.addresseeId === this.meId).length;
    const made = this.occasions.filter((o) => o.createdById === this.meId).length;
    const forMe = this.occasions.filter((o) => o.recipientId === this.meId).length;
    const bought = this.gifts.filter((g) => g.buyerId === this.meId).length;
    return [
      `${friends} friendship(s) and friend request(s)`,
      `${made} occasion(s) you made – with every gift your friends planned on them`,
      `${forMe} occasion(s) for you – with every gift on them`,
      `${bought} gift(s) you buy, and every split you are part of`,
    ];
  }

  // Called by the warning box's (confirmed) event: delete the profile, log out, go to the login page
  deleteAccount(): void {
    this.profileService.deleteProfile(this.meId).subscribe(() => {
      this.authService.logout();
      this.router.navigate(['/login']);
    });
  }
}
