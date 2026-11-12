import { Component, computed, effect, inject, input, model, signal, untracked } from '@angular/core';
import { Badge } from '../badge/badge';
import { Button } from '../button/button';
import { Modal } from '../modal/modal';
import { Paginator } from '../paginator/paginator';
import { TextField } from '../text-field/text-field';
import { statusLabel, statusTone } from '../../constants/status-labels';
import { Portal } from '../../services/portal';
import { formatInterval, formatKilometers, parseOptionalNumber } from '../../shared/format';
import { clampPage, pageOf } from '../../shared/pagination';

@Component({
  selector: 'app-routine-drawer',
  imports: [Badge, Button, Modal, Paginator, TextField],
  templateUrl: './routine-drawer.html',
  styleUrl: './routine-drawer.css'
})
export class RoutineDrawer {
  readonly portal = inject(Portal);
  readonly open = model(false);
  readonly allowCreate = input(false);
  readonly label = statusLabel;
  readonly tone = statusTone;
  readonly interval = formatInterval;
  readonly mileage = formatKilometers;

  name = '';
  kilometers = '';
  months = '';
  readonly page = signal(1);

  readonly heading = computed(() => {
    const plate = this.portal.vehicles().find(vehicle => vehicle.id === this.portal.selectedId())?.plate;
    return plate ? `Rutinas de ${plate}` : 'Rutinas';
  });

  readonly currentPage = computed(() => clampPage(this.page(), this.portal.routines().length));
  readonly visibleRoutines = computed(() => pageOf(this.portal.routines(), this.currentPage()));

  constructor() {
    effect(() => {
      this.portal.selectedId();
      untracked(() => this.page.set(1));
    });
  }

  add(event: Event): void {
    event.preventDefault();
    this.portal.addRoutine(this.name, parseOptionalNumber(this.kilometers), parseOptionalNumber(this.months)).subscribe(() => {
      this.name = '';
      this.kilometers = '';
      this.months = '';
    });
  }
}
