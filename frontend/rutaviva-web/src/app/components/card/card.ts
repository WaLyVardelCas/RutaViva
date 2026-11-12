import { Component, input } from '@angular/core';

export type CardTone = 'default' | 'primary' | 'success' | 'warning' | 'danger' | 'navy';

@Component({
  selector: 'app-card',
  templateUrl: './card.html',
  styleUrl: './card.css',
  host: {
    '[attr.data-tone]': 'tone()',
    '[attr.title]': 'null'
  }
})
export class Card {
  readonly title = input<string | null>(null);
  readonly tone = input<CardTone>('default');
}
