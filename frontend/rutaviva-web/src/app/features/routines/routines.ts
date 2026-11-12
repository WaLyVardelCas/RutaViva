import { Component, computed, inject, signal } from '@angular/core';
import { Card } from '../../components/card/card';
import { RoutineDrawer } from '../../components/routine-drawer/routine-drawer';
import { VehicleList } from '../../components/vehicle-list/vehicle-list';
import { Role } from '../../constants/role';
import { Portal } from '../../services/portal';

@Component({
  selector: 'app-routines',
  imports: [Card, RoutineDrawer, VehicleList],
  templateUrl: './routines.html',
  styleUrl: './routines.css'
})
export class Routines {
  readonly portal = inject(Portal);
  readonly allowCreate = computed(() => this.portal.role() === Role.Workshop);
  readonly drawerOpen = signal(false);

  openRoutines(id: string): void {
    this.portal.select(id);
    this.drawerOpen.set(true);
  }
}
