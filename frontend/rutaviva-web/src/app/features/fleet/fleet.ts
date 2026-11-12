import { Component, inject, signal } from '@angular/core';
import { Button } from '../../components/button/button';
import { Card } from '../../components/card/card';
import { RoutineDrawer } from '../../components/routine-drawer/routine-drawer';
import { TextField } from '../../components/text-field/text-field';
import { VehicleList } from '../../components/vehicle-list/vehicle-list';
import { Portal } from '../../services/portal';
import { parseOptionalNumber } from '../../shared/format';

@Component({
  selector: 'app-fleet',
  imports: [Button, Card, RoutineDrawer, TextField, VehicleList],
  templateUrl: './fleet.html',
  styleUrl: './fleet.css'
})
export class Fleet {
  readonly portal = inject(Portal);
  readonly drawerOpen = signal(false);

  plate = '';
  mileage = '';

  register(event: Event): void {
    event.preventDefault();
    this.portal.registerVehicle(this.plate, parseOptionalNumber(this.mileage)).subscribe(() => {
      this.plate = '';
      this.mileage = '';
    });
  }

  openRoutines(id: string): void {
    this.portal.select(id);
    this.drawerOpen.set(true);
  }
}
