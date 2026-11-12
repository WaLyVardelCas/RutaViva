import { Injectable, signal } from '@angular/core';

export type ToastTone = 'success' | 'danger' | 'info';

export interface ToastMessage {
  id: number;
  message: string;
  tone: ToastTone;
}

const VISIBLE_FOR_MS = 5000;

@Injectable({ providedIn: 'root' })
export class ToastService {
  private nextId = 0;
  private readonly timers = new Map<number, ReturnType<typeof setTimeout>>();
  readonly messages = signal<ToastMessage[]>([]);

  show(message: string, tone: ToastTone = 'info'): void {
    const id = ++this.nextId;
    this.messages.update(current => [...current, { id, message, tone }]);
    this.timers.set(id, setTimeout(() => this.dismiss(id), VISIBLE_FOR_MS));
  }

  dismiss(id: number): void {
    const timer = this.timers.get(id);
    if (timer !== undefined) {
      clearTimeout(timer);
      this.timers.delete(id);
    }
    this.messages.update(current => current.filter(item => item.id !== id));
  }
}
