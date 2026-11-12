import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { HomeComponent } from './home/home.component';
import { OccasionListComponent } from './occasion-list/occasion-list.component';
import { OccasionDetailComponent } from './occasion-detail/occasion-detail.component';
import { GiftFormComponent } from './gift-form/gift-form.component';
import { OccasionFormComponent } from './occasion-form/occasion-form.component';
import { ProfileComponent } from './profile/profile.component';
import { FriendsComponent } from './friends/friends.component';

// Which component to show for which URL
export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' }, // the app starts on the login page
  { path: 'login', component: LoginComponent },
  { path: 'home', component: HomeComponent }, // landing page after login
  { path: 'occasions', component: OccasionListComponent },
  // 'occasion/new' must come before 'occasion/:id': Angular uses the first route that matches,
  // and 'new' would otherwise be read as an id
  { path: 'occasion/new', component: OccasionFormComponent }, // add an occasion
  { path: 'occasion/:id/edit', component: OccasionFormComponent }, // edit an occasion
  { path: 'occasion/:id', component: OccasionDetailComponent },
  { path: 'occasion/:occasionId/gift/new', component: GiftFormComponent }, // add a gift
  { path: 'gift/:id/edit', component: GiftFormComponent }, // edit a gift
  { path: 'profile/:id', component: ProfileComponent },
  { path: 'friends', component: FriendsComponent }, // friend requests and friends
];
