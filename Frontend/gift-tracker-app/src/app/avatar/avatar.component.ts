import { Component, Input } from '@angular/core';

// A round light-blue circle with a person's initials, e.g. "Anna Jensen" -> "AJ"
@Component({
  selector: 'app-avatar',
  imports: [],
  templateUrl: './avatar.component.html',
  styleUrl: './avatar.component.css',
})
export class AvatarComponent {
  @Input() name = '';

  initials(): string {
    const words = this.name.split(' ');
    return words
      .slice(0, 2)
      .map((word) => word.charAt(0))
      .join('')
      .toUpperCase();
  }
}
