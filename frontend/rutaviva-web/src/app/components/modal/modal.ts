import { booleanAttribute, Component, effect, HostListener, input, model } from '@angular/core';

@Component({
  selector: 'app-modal',
  templateUrl: './modal.html',
  styleUrl: './modal.css',
  host: {
    '[attr.title]': 'null'
  }
})
export class Modal {
  private static nextId = 0;

  readonly open = model(false);
  readonly side = input(false, { transform: booleanAttribute });
  readonly title = input.required<string>();
  readonly titleId = `modal-title-${Modal.nextId++}`;

  constructor() {
    effect(onCleanup => {
      if (!this.open()) return;
      const previous = document.body.style.overflow;
      document.body.style.overflow = 'hidden';
      onCleanup(() => {
        document.body.style.overflow = previous;
      });
    });
  }

  @HostListener('document:keydown.escape')
  closeOnEscape(): void {
    if (this.open()) this.open.set(false);
  }

  dismiss(): void {
    this.open.set(false);
  }

  keepOpen(event: Event): void {
    event.stopPropagation();
  }
}
