import { Component, input } from '@angular/core';

@Component({
  selector: 'button[appButton]',
  template: '<ng-content />',
  styleUrl: './button.css',
  host: {
    '[class.primary]': 'variant() === "primary"',
    '[class.secondary]': 'variant() === "secondary"',
    '[class.danger]': 'variant() === "danger"'
  }
})
export class Button {
  readonly variant = input<'primary' | 'secondary' | 'danger'>('primary');
}
