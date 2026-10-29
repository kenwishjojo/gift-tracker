import { Routes } from '@angular/router';
import { OccasionListComponent } from './occasion-list/occasion-list.component';
import { OccasionDetailComponent } from './occasion-detail/occasion-detail.component';
import { GiftFormComponent } from './gift-form/gift-form.component';
import { ProfileComponent } from './profile/profile.component';

// Which component to show for which URL
export const routes: Routes = [
  { path: '', redirectTo: 'occasions', pathMatch: 'full' },
  { path: 'occasions', component: OccasionListComponent },
  { path: 'occasion/:id', component: OccasionDetailComponent },
  { path: 'occasion/:occasionId/gift/new', component: GiftFormComponent }, // add a gift
  { path: 'gift/:id/edit', component: GiftFormComponent }, // edit a gift
  { path: 'profile/:id', component: ProfileComponent },
];
