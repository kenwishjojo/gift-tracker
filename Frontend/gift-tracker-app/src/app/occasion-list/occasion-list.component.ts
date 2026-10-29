import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Occasion } from '../model/occasion';
import { OccasionService } from '../services/occasion.service';

// Page 1: all occasions
@Component({
  selector: 'app-occasion-list',
  imports: [RouterLink],
  templateUrl: './occasion-list.component.html',
  styleUrl: './occasion-list.component.css',
})
export class OccasionListComponent implements OnInit {
  occasions: Occasion[] = [];

  constructor(private occasionService: OccasionService) {}

  ngOnInit(): void {
    this.occasionService.getOccasions().subscribe((data) => (this.occasions = data));
  }
}
