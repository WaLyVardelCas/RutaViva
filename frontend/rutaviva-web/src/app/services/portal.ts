import { DestroyRef, Injectable, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  EMPTY,
  Observable,
  Subject,
  catchError,
  finalize,
  forkJoin,
  map,
  of,
  switchMap,
  tap,
  timeout
} from 'rxjs';
import { statusLabel } from '../constants/status-labels';
import { Order } from '../shared/models/order';
import { Routine } from '../shared/models/routine';
import { Vehicle } from '../shared/models/vehicle';
import { problemDetail } from './http-problem';
import { OrderService } from './order.service';
import { ToastService } from './toast.service';
import { RoleContext } from './role-context';
import { RoutineService } from './routine.service';
import { VehicleService } from './vehicle.service';

const REQUEST_TIMEOUT_MS = 8000;

interface Snapshot {
  vehicles: Vehicle[];
  orders: Order[];
  routinesByVehicle: Record<string, Routine[]>;
}

@Injectable({ providedIn: 'root' })
export class Portal {
  private readonly vehiclesApi = inject(VehicleService);
  private readonly routinesApi = inject(RoutineService);
  private readonly ordersApi = inject(OrderService);
  private readonly toasts = inject(ToastService);
  private readonly roles = inject(RoleContext);
  private readonly destroyRef = inject(DestroyRef);
  private readonly refreshRequests = new Subject<number>();
  private generation = 0;

  readonly refreshing = signal(false);
  readonly role = this.roles.role;
  readonly vehicles = signal<Vehicle[]>([]);
  readonly selectedId = signal<string | null>(null);
  readonly routines = signal<Routine[]>([]);
  private readonly routinesByVehicle = signal<Record<string, Routine[]>>({});
  readonly orders = signal<Order[]>([]);

  constructor() {
    this.refreshRequests
      .pipe(
        switchMap(generation => {
          this.refreshing.set(true);
          return this.fetchSnapshot().pipe(
            timeout(REQUEST_TIMEOUT_MS),
            catchError(error => {
              this.showFailure(error);
              return EMPTY;
            }),
            finalize(() => {
              if (this.generation === generation) this.refreshing.set(false);
            })
          );
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(snapshot => this.applySnapshot(snapshot));
  }

  load(): void {
    this.refreshRequests.next(++this.generation);
  }

  select(id: string): void {
    this.selectedId.set(id);
    this.routines.set(this.routinesByVehicle()[id] ?? []);
  }

  registerVehicle(plate: string, mileage: number | null): Observable<void> {
    return this.vehiclesApi.register(plate, mileage).pipe(
      tap(vehicle => {
        this.showSuccess(`Vehículo ${vehicle.plate} registrado.`);
        this.select(vehicle.id);
        this.load();
      }),
      map(() => undefined),
      catchError(error => {
        this.showFailure(error);
        this.load();
        return EMPTY;
      })
    );
  }

  addRoutine(name: string, kilometers: number | null, months: number | null): Observable<void> {
    const vehicleId = this.selectedId();
    if (!vehicleId) return EMPTY;

    return this.routinesApi.add(vehicleId, name, kilometers, months).pipe(
      tap(() => {
        this.showSuccess('Rutina configurada.');
        this.load();
      }),
      map(() => undefined),
      catchError(error => {
        this.showFailure(error);
        this.load();
        return EMPTY;
      })
    );
  }

  quote(order: Order, amount: number | null): void {
    this.act(this.ordersApi.quote(order.id, amount));
  }

  approve(order: Order): void {
    this.act(this.ordersApi.approve(order.id));
  }

  reject(order: Order): void {
    this.act(this.ordersApi.reject(order.id));
  }

  cancel(order: Order): void {
    this.act(this.ordersApi.cancel(order.id));
  }

  startService(order: Order): void {
    this.act(this.ordersApi.start(order.id));
  }

  complete(order: Order, mileage: number | null, date: string): void {
    this.act(this.ordersApi.complete(order.id, mileage, date));
  }

  vehicleLabel(id: string): string {
    return this.vehicles().find(vehicle => vehicle.id === id)?.plate ?? id.slice(0, 8);
  }

  routineNameOf(order: Order): string {
    const routine = (this.routinesByVehicle()[order.vehicleId] ?? []).find(item => item.id === order.routineId);
    return routine?.name ?? 'Rutina de mantenimiento';
  }

  allRoutines(): Routine[] {
    return Object.values(this.routinesByVehicle()).flat();
  }

  private act(request: Observable<Order>): void {
    request.subscribe({
      next: updated => {
        this.showSuccess(`La orden quedó ${statusLabel(updated.status).toLowerCase()}.`);
        this.load();
      },
      error: error => {
        this.showFailure(error);
        this.load();
      }
    });
  }

  private fetchSnapshot(): Observable<Snapshot> {
    return forkJoin({
      vehicles: this.vehiclesApi.list(),
      orders: this.ordersApi.list()
    }).pipe(
      switchMap(({ vehicles, orders }) => {
        if (vehicles.length === 0) return of({ vehicles, orders, routinesByVehicle: {} });

        return forkJoin(vehicles.map(vehicle => this.routinesApi.list(vehicle.id).pipe(
          map(routines => [vehicle.id, routines] as const)
        ))).pipe(
          map(entries => ({
            vehicles,
            orders,
            routinesByVehicle: Object.fromEntries(entries)
          }))
        );
      })
    );
  }

  private applySnapshot(snapshot: Snapshot): void {
    const selected = this.selectedId();
    if (selected && !snapshot.vehicles.some(vehicle => vehicle.id === selected)) {
      this.selectedId.set(null);
      this.routines.set([]);
    }

    if (!same(this.vehicles(), snapshot.vehicles)) this.vehicles.set(snapshot.vehicles);
    if (!same(this.orders(), snapshot.orders)) this.orders.set(snapshot.orders);
    if (!same(this.routinesByVehicle(), snapshot.routinesByVehicle)) {
      this.routinesByVehicle.set(snapshot.routinesByVehicle);
    }

    const currentId = this.selectedId();
    if (!currentId) return;
    const routines = snapshot.routinesByVehicle[currentId] ?? [];
    if (!same(this.routines(), routines)) this.routines.set(routines);
  }

  private showSuccess(message: string): void {
    this.toasts.show(message, 'success');
  }

  private showFailure(error: unknown): void {
    this.toasts.show(problemDetail(error), 'danger');
  }
}

function same(left: unknown, right: unknown): boolean {
  return JSON.stringify(left) === JSON.stringify(right);
}
