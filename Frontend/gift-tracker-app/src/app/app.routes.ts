import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { HomeComponent } from './home/home.component';
import { OccasionListComponent } from './occasion-list/occasion-list.component';
import { OccasionDetailComponent } from './occasion-detail/occasion-detail.component';
import { GiftFormComponent } from './gift-form/gift-form.component';
import { ProfileComponent } from './profile/profile.component';

// Which component to show for which URL
export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' }, // the app starts on the login page
  { path: 'login', component: LoginComponent },
  { path: 'home', component: HomeComponent }, // landing page after login
  { path: 'occasions', component: OccasionListComponent },
  { path: 'occasion/:id', component: OccasionDetailComponent },
  { path: 'occasion/:occasionId/gift/new', component: GiftFormComponent }, // add a gift
  { path: 'gift/:id/edit', component: GiftFormComponent }, // edit a gift
  { path: 'profile/:id', component: ProfileComponent },
];
