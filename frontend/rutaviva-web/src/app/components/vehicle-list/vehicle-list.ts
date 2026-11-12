import { Component, computed, input, output, signal } from '@angular/core';
import { Paginator } from '../paginator/paginator';
import { TextField } from '../text-field/text-field';
import { Button } from '../button/button';
import { formatKilometers } from '../../shared/format';
import { Vehicle } from '../../shared/models/vehicle';
import { clampPage, pageOf } from '../../shared/pagination';

@Component({
  selector: 'app-vehicle-list',
  imports: [Paginator, TextField, Button],
  templateUrl: './vehicle-list.html',
  styleUrl: './vehicle-list.css'
})
export class VehicleList {
  readonly vehicles = input.required<Vehicle[]>();
  readonly loading = input(false);
  readonly selectedId = input<string | null>(null);
  readonly selected = output<string>();
  readonly kilometers = formatKilometers;
  readonly query = signal('');
  readonly page = signal(1);

  readonly filtered = computed(() => {
    const term = this.query().trim().toLocaleLowerCase('es');
    const vehicles = this.vehicles();
    if (!term) return vehicles;
    return vehicles.filter(vehicle => vehicle.plate.toLocaleLowerCase('es').includes(term));
  });

  readonly currentPage = computed(() => clampPage(this.page(), this.filtered().length));
  readonly visible = computed(() => pageOf(this.filtered(), this.currentPage()));

  filter(value: string): void {
    this.query.set(value);
    this.page.set(1);
  }
}
