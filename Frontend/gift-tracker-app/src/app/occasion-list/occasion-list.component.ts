import { Component, OnInit } from '@angular/core';
import { Occasion } from '../model/occasion';
import { Profile } from '../model/profile';
import { Gift } from '../model/gift';
import { OccasionService } from '../services/occasion.service';
import { ProfileService } from '../services/profile.service';
import { GiftService } from '../services/gift.service';
import { AuthService } from '../services/auth.service';
import { OccasionCardComponent } from '../occasion-card/occasion-card.component';

// Page: all occasions, the next one first
@Component({
  selector: 'app-occasion-list',
  imports: [OccasionCardComponent],
  templateUrl: './occasion-list.component.html',
  styleUrl: './occasion-list.component.css',
})
export class OccasionListComponent implements OnInit {
  upcoming: Occasion[] = [];
  past: Occasion[] = [];
  profiles: Profile[] = []; // to show the recipient's name on each card
  gifts: Gift[] = []; // to show the gift status on each card
  meId = 0; // the logged-in user, passed to each card for "Your share"

  constructor(
    private occasionService: OccasionService,
    private profileService: ProfileService,
    private giftService: GiftService,
    private authService: AuthService,
  ) {}

  ngOnInit(): void {
    this.meId = this.authService.getProfile()?.id ?? 0;
    this.occasionService.getOccasions().subscribe((data) => {
      // Dates are text like "2026-12-03", so sorting the text also sorts by date
      const sorted = data.sort((a, b) => a.eventDate.localeCompare(b.eventDate));
      const today = this.todayAsText();
      this.upcoming = sorted.filter((o) => o.eventDate >= today);
      this.past = sorted.filter((o) => o.eventDate < today).reverse(); // newest first
    });
    this.profileService.getProfiles().subscribe((data) => (this.profiles = data));
    this.giftService.getGifts().subscribe((data) => (this.gifts = data));
  }

  findProfile(id: number): Profile | undefined {
    return this.profiles.find((p) => p.id === id);
  }

  giftsFor(occasionId: number): Gift[] {
    return this.gifts.filter((g) => g.occasionId === occasionId);
  }

  // Today's date as "YYYY-MM-DD" (same format as eventDate)
  private todayAsText(): string {
    const now = new Date();
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const day = String(now.getDate()).padStart(2, '0');
    return `${now.getFullYear()}-${month}-${day}`;
  }
}
