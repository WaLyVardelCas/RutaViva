import { Component, computed, input, output } from '@angular/core';
import { PAGE_SIZE } from '../../shared/pagination';

@Component({
  selector: 'app-paginator',
  templateUrl: './paginator.html',
  styleUrl: './paginator.css'
})
export class Paginator {
  readonly total = input.required<number>();
  readonly page = input(1);
  readonly pageSize = input(PAGE_SIZE);
  readonly pageChange = output<number>();

  readonly pages = computed(() => Math.max(1, Math.ceil(this.total() / this.pageSize())));
  readonly rangeLabel = computed(() => {
    if (this.total() === 0) return '0 de 0';
    const start = (this.page() - 1) * this.pageSize() + 1;
    const end = Math.min(this.total(), this.page() * this.pageSize());
    return `${start}–${end} de ${this.total()}`;
  });

  go(next: number): void {
    const target = Math.min(Math.max(next, 1), this.pages());
    if (target !== this.page()) this.pageChange.emit(target);
  }
}
