import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Profile } from '../model/profile';
import { ProfileService } from '../services/profile.service';

// Page 4: one profile
@Component({
  selector: 'app-profile',
  imports: [],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css',
})
export class ProfileComponent implements OnInit {
  profile?: Profile;

  constructor(
    private route: ActivatedRoute,
    private profileService: ProfileService,
  ) {}

  ngOnInit(): void {
    // The id comes from the URL, e.g. /profile/1
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.profileService.getProfile(id).subscribe((data) => (this.profile = data));
  }
}
