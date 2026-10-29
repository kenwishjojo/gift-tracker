import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Occasion } from '../model/occasion';
import { Gift } from '../model/gift';
import { OccasionService } from '../services/occasion.service';
import { GiftService } from '../services/gift.service';

// Page 2: one occasion and its gifts
@Component({
  selector: 'app-occasion-detail',
  imports: [RouterLink],
  templateUrl: './occasion-detail.component.html',
  styleUrl: './occasion-detail.component.css',
})
export class OccasionDetailComponent implements OnInit {
  occasion?: Occasion;
  gifts: Gift[] = [];

  constructor(
    private route: ActivatedRoute,
    private occasionService: OccasionService,
    private giftService: GiftService,
  ) {}

  ngOnInit(): void {
    // The id comes from the URL, e.g. /occasion/3
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.occasionService.getOccasion(id).subscribe((data) => (this.occasion = data));
    this.giftService.getGiftsForOccasion(id).subscribe((data) => (this.gifts = data));
  }
}
