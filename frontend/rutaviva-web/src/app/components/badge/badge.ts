import { Component, input } from '@angular/core';
import { BadgeTone } from '../../shared/badge-tone';

@Component({
  selector: 'app-badge',
  template: '<ng-content />',
  styleUrl: './badge.css',
  host: {
    '[class]': 'tone()'
  }
})
export class Badge {
  readonly tone = input<BadgeTone>('neutral');
}
