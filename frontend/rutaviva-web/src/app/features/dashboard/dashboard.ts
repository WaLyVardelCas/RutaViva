import { Component, computed, inject, signal } from '@angular/core';
import { Badge } from '../../components/badge/badge';
import { Card, CardTone } from '../../components/card/card';
import { DataTable } from '../../components/data-table/data-table';
import { Paginator } from '../../components/paginator/paginator';
import { TextField } from '../../components/text-field/text-field';
import { OrderStatus } from '../../constants/order-status';
import { Role } from '../../constants/role';
import { RoutineStatus } from '../../constants/routine-status';
import { statusLabel, statusTone } from '../../constants/status-labels';
import { Portal } from '../../services/portal';
import { Order } from '../../shared/models/order';
import { clampPage, pageOf } from '../../shared/pagination';

@Component({
  selector: 'app-dashboard',
  imports: [Card, Badge, DataTable, TextField, Paginator],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard {
  private readonly portal = inject(Portal);
  readonly query = signal('');
  readonly page = signal(1);
  readonly label = statusLabel;
  readonly tone = statusTone;

  readonly workshop = computed(() => this.portal.role() === Role.Workshop);

  readonly metrics = computed(() => {
    const orders = this.portal.orders();
    const routines = this.portal.allRoutines();
    if (this.workshop()) {
      return [
        { label: 'Órdenes abiertas', tone: 'navy' as CardTone, value: this.count(orders, order => order.status !== OrderStatus.Completed && order.status !== OrderStatus.Canceled) },
        { label: 'Cotizaciones pendientes', tone: 'warning' as CardTone, value: this.count(orders, order => order.status === OrderStatus.Quoted) },
        { label: 'Servicios en ejecución', tone: 'primary' as CardTone, value: this.count(orders, order => order.status === OrderStatus.InProgress) },
        { label: 'Órdenes completadas', tone: 'success' as CardTone, value: this.count(orders, order => order.status === OrderStatus.Completed) }
      ];
    }
    return [
      { label: 'Vehículos en la flota', tone: 'primary' as CardTone, value: String(this.portal.vehicles().length) },
      { label: 'Rutinas al día', tone: 'success' as CardTone, value: this.count(routines, routine => routine.status === RoutineStatus.UpToDate) },
      { label: 'Rutinas por vencer', tone: 'warning' as CardTone, value: this.count(routines, routine => routine.status === RoutineStatus.DueSoon) },
      { label: 'Rutinas vencidas', tone: 'danger' as CardTone, value: this.count(routines, routine => routine.status === RoutineStatus.Overdue) }
    ];
  });

  plate(order: Order): string {
    return this.portal.vehicleLabel(order.vehicleId);
  }

  routine(order: Order): string {
    return this.portal.routineNameOf(order);
  }

  readonly rows = computed(() => {
    const term = this.query().trim().toLocaleLowerCase('es');
    return this.portal.orders().filter(order => {
      if (!term) return true;
      const plate = this.portal.vehicleLabel(order.vehicleId);
      const routine = this.portal.routineNameOf(order);
      return [plate, routine, statusLabel(order.status)].some(value => value.toLocaleLowerCase('es').includes(term));
    });
  });

  readonly currentPage = computed(() => clampPage(this.page(), this.rows().length));
  readonly pageRows = computed(() => pageOf(this.rows(), this.currentPage()));

  filter(value: string): void {
    this.query.set(value);
    this.page.set(1);
  }

  private count<T>(items: T[], match: (item: T) => boolean): string {
    return String(items.filter(match).length);
  }
}
