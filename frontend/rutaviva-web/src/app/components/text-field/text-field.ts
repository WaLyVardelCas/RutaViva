import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-text-field',
  templateUrl: './text-field.html',
  styleUrl: './text-field.css',
  host: {
    '[class.invalid]': '!!error()'
  }
})
export class TextField {
  private static nextId = 0;

  readonly label = input.required<string>();
  readonly value = input('');
  readonly type = input<'text' | 'search' | 'number' | 'date'>('text');
  readonly autocomplete = input('on');
  readonly disabled = input(false);
  readonly error = input<string | null>(null);
  readonly valueChange = output<string>();

  readonly inputId = `field-${TextField.nextId++}`;
  readonly errorId = `${this.inputId}-error`;

  onInput(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.valueChange.emit(target.value);
  }
}
