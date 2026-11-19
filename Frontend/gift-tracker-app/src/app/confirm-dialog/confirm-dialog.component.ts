import { Component, EventEmitter, Input, Output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

// The warning box ("Are you sure?") used before every delete and before saving changes to an
// existing gift or occasion. One component, so it looks and works the same everywhere.
//
// How a page uses it (the parent decides WHEN to show it with @if):
//   @if (showDeleteBox) {
//     <app-confirm-dialog title="Delete gift?" message="..." confirmLabel="Delete"
//                         (confirmed)="deleteGift()" (cancelled)="showDeleteBox = false" />
//   }
//
// Data goes IN with @Input (like StudentCard in Lecture 6). The answer goes back OUT to the parent
// with @Output: an EventEmitter "fires" an event, and the parent listens with (confirmed)="..." –
// the same (event) binding syntax as (click). This is the child -> parent half of @Input.
@Component({
  selector: 'app-confirm-dialog',
  imports: [MatIconModule],
  templateUrl: './confirm-dialog.component.html',
  styleUrl: './confirm-dialog.component.css',
})
export class ConfirmDialogComponent {
  @Input() title = 'Are you sure?';
  @Input() message = '';
  @Input() details: string[] = []; // optional bullet list, e.g. everything that will be deleted
  @Input() confirmLabel = 'Yes, continue';

  // Events the parent page listens to
  @Output() confirmed = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  confirm(): void {
    this.confirmed.emit();
  }

  cancel(): void {
    this.cancelled.emit();
  }
}
