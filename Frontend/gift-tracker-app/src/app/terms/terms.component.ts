import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

// Page: terms and conditions (/terms). Only text – no data from the API.
// Linked from the profile page and from the sign-up form. The text is a short placeholder written
// for the student project, not real legal terms.
@Component({
  selector: 'app-terms',
  imports: [RouterLink],
  templateUrl: './terms.component.html',
  styleUrl: './terms.component.css',
})
export class TermsComponent {}
