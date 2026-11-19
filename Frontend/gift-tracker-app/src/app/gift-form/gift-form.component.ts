import { Component, Input, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import { DecimalPipe, TitleCasePipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { Gift, GIFT_STATUSES, shareOf } from '../model/gift';
import { Occasion } from '../model/occasion';
import { Profile } from '../model/profile';
import { Connection } from '../model/connection';
import { GiftGroup, GroupMember } from '../model/gift-group';
import { GiftService } from '../services/gift.service';
import { OccasionService } from '../services/occasion.service';
import { ProfileService } from '../services/profile.service';
import { AuthService } from '../services/auth.service';
import { ConnectionService } from '../services/connection.service';
import { GiftGroupService } from '../services/gift-group.service';

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

  // Splitting with friends: who can be chosen, who is chosen, and the gift's group if it has one
  profiles: Profile[] = [];
  connections: Connection[] = []; // the friendships, to find the logged-in user's friends
  selectedFriendIds: number[] = []; // the friends ticked in the list
  existingGroup?: GiftGroup; // edit: the group the gift already has (it is replaced when saving)

  // Constructor injection (dependency injection, Lecture 6): Angular creates the services
  // and hands them to the component, so every page shares the same service objects
  constructor(
    private giftService: GiftService,
    private occasionService: OccasionService,
    private profileService: ProfileService,
    private authService: AuthService,
    private connectionService: ConnectionService,
    private giftGroupService: GiftGroupService,
    private router: Router,
  ) {}

  // ngOnInit runs once when the component starts – the place to fetch data (Lecture 6)
  ngOnInit(): void {
    this.me = this.authService.getProfile();
    this.profileService.getProfiles().subscribe((data) => (this.profiles = data));
    this.connectionService.getConnections().subscribe((data) => (this.connections = data));
    if (this.id) {
      // Edit: load the gift, then its occasion.
      // getGift returns an Observable; the code in subscribe runs when the API answers (Lecture 6)
      this.giftService.getGift(Number(this.id)).subscribe((gift) => {
        this.gift = gift;
        this.isSplit = gift.splitCount > 1; // tick the checkbox if the gift is already split
        this.loadOccasion(gift.occasionId);
        this.loadGroup(gift.id);
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

  // Edit: find the gift's group and tick the friends who are already in it.
  // (No "group for one gift" endpoint yet – step 4 – so we get all groups and members and filter.)
  loadGroup(giftId: number): void {
    this.giftGroupService.getGroups().subscribe((groups) => {
      this.existingGroup = groups.find((g) => g.giftId === giftId);
      if (this.existingGroup) {
        const groupId = this.existingGroup.id;
        this.giftGroupService.getMembers().subscribe((members: GroupMember[]) => {
          this.selectedFriendIds = members
            .filter((m) => m.groupId === groupId && m.role === 'viewer')
            .map((m) => m.profileId);
        });
      }
    });
  }

  // The people you can split with: your accepted friends, plus anyone already in the group.
  // Never the buyer (they pay already) and never the person the gift is for (it's their present).
  friendOptions(): Profile[] {
    if (!this.me || !this.gift || !this.occasion) {
      return [];
    }
    const meId = this.me.id;
    const friendIds = this.connections
      .filter((c) => c.status === 'accepted' && (c.requesterId === meId || c.addresseeId === meId))
      .map((c) => (c.requesterId === meId ? c.addresseeId : c.requesterId));
    const buyerId = this.gift.buyerId;
    const recipientId = this.occasion.recipientId;
    return this.profiles.filter(
      (p) =>
        (friendIds.includes(p.id) || this.selectedFriendIds.includes(p.id)) &&
        p.id !== buyerId &&
        p.id !== recipientId,
    );
  }

  isSelected(profileId: number): boolean {
    return this.selectedFriendIds.includes(profileId);
  }

  // A friend's checkbox was clicked: add or remove them, and update splitCount (friends + the buyer)
  toggleFriend(profileId: number): void {
    if (this.isSelected(profileId)) {
      this.selectedFriendIds = this.selectedFriendIds.filter((id) => id !== profileId);
    } else {
      this.selectedFriendIds = [...this.selectedFriendIds, profileId];
    }
    if (this.gift) {
      this.gift.splitCount = this.selectedFriendIds.length + 1;
    }
  }

  // A split needs at least one friend. Not an ngModel field, so we check it ourselves and use it
  // together with giftForm.invalid to disable the Save button.
  splitNeedsFriends(): boolean {
    return this.isSplit && this.selectedFriendIds.length === 0;
  }

  // True when the logged-in user is the occasion's recipient. Then the form is hidden:
  // you can't add gifts to your own birthday (the API checks that too: buyer can't be the recipient),
  // and opening /gift/5/edit for a gift on your own occasion mustn't reveal the surprise either.
  isForMe(): boolean {
    return !!this.me && !!this.occasion && this.occasion.recipientId === this.me.id;
  }

  // Ticking or unticking "Split the price": the number of people follows the chosen friends
  onSplitChange(): void {
    if (this.gift) {
      this.gift.splitCount = this.isSplit ? this.selectedFriendIds.length + 1 : 1;
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
      this.selectedFriendIds = [];
    }
    // First the gift, then its group (the group needs the gift's id)
    if (this.gift.id === 0) {
      this.giftService.addGift(this.gift).subscribe({
        next: (saved) => this.saveGroup(saved.id),
        error: (err) => this.showError(err),
      });
    } else {
      this.giftService.updateGift(this.gift).subscribe({
        next: () => this.saveGroup(this.gift!.id),
        error: (err) => this.showError(err),
      });
    }
  }

  // Saves who splits the gift. The simplest way to change the members is to replace the whole group:
  // delete the old one (its members go with it – on delete cascade) and create a new one with the
  // buyer as owner and the ticked friends as viewers (one transaction in the API).
  saveGroup(giftId: number): void {
    if (this.existingGroup) {
      this.giftGroupService.deleteGroup(this.existingGroup.id).subscribe({
        next: () => this.createGroup(giftId),
        error: (err) => this.showError(err),
      });
    } else {
      this.createGroup(giftId);
    }
  }

  createGroup(giftId: number): void {
    if (!this.isSplit || !this.gift) {
      this.goBack(); // not split: no group needed
      return;
    }
    this.giftGroupService.addGroup(giftId, this.gift.name, this.gift.buyerId, this.selectedFriendIds).subscribe({
      next: () => this.goBack(),
      error: (err) => this.showError(err),
    });
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
