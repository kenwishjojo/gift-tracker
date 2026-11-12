import { Component, Input } from '@angular/core';
import { DatePipe, DecimalPipe, TitleCasePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { Occasion } from '../model/occasion';
import { Profile } from '../model/profile';
import { Gift, totalPrice, totalShareFor } from '../model/gift';
import { AvatarComponent } from '../avatar/avatar.component';

// One occasion as a card (.os-occ from the brand book): who, what, when, and gift status.
// The parent page gives it the data through @Input.
@Component({
  selector: 'app-occasion-card',
  imports: [RouterLink, DatePipe, DecimalPipe, TitleCasePipe, MatIconModule, AvatarComponent],
  templateUrl: './occasion-card.component.html',
  styleUrl: './occasion-card.component.css',
})
export class OccasionCardComponent {
  // @Input: the parent page passes the data in, like [student] into StudentCard in Lecture 6.
  // The ! tells TypeScript the value will be set by the parent before it is used.
  @Input() occasion!: Occasion;
  @Input() recipient?: Profile; // the person who gets the gifts
  @Input() gifts: Gift[] = []; // the gifts for this occasion
  @Input() meId = 0; // the logged-in user's id, for "Your share"

  // Same calculations as on the occasion page (shared functions in model/gift.ts)
  total(): number {
    return totalPrice(this.gifts);
  }

  myShare(): number {
    return totalShareFor(this.gifts, this.meId);
  }

  // Number of days from today until the occasion (negative if it is over)
  daysLeft(): number {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const eventDay = new Date(this.occasion.eventDate + 'T00:00:00');
    const oneDay = 24 * 60 * 60 * 1000; // milliseconds
    return Math.round((eventDay.getTime() - today.getTime()) / oneDay);
  }

  // The text after the date, e.g. "in 21 days" (written so it follows the date without a separator)
  timeText(): string {
    const days = this.daysLeft();
    if (days === 0) return 'is today';
    if (days === 1) return 'is tomorrow';
    if (days > 1) return `in ${days} days`;
    return 'has passed';
  }

  // A gift counts as done when it is bought or already given
  hasBoughtGift(): boolean {
    return this.gifts.some((g) => g.status === 'bought' || g.status === 'given');
  }

  // Brand book: blue border and blue badge only when the occasion is close (30 days) and has no gift
  isSoonWithoutGift(): boolean {
    const days = this.daysLeft();
    return days >= 0 && days <= 30 && this.gifts.length === 0;
  }
}
