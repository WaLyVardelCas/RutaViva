import { Component, computed, inject, input, signal } from '@angular/core';
import { Badge } from '../../components/badge/badge';
import { Button } from '../../components/button/button';
import { Card } from '../../components/card/card';
import { Paginator } from '../../components/paginator/paginator';
import { QuotePanel } from '../../components/quote-panel/quote-panel';
import { TextField } from '../../components/text-field/text-field';
import { canCancelOrder, OrderStatus } from '../../constants/order-status';
import { Role } from '../../constants/role';
import { statusLabel, statusTone } from '../../constants/status-labels';
import { Portal } from '../../services/portal';
import { formatKilometers, parseOptionalNumber } from '../../shared/format';
import { clampPage, pageOf } from '../../shared/pagination';
import { Order } from '../../shared/models/order';

export type OrderView = 'client' | 'assigned' | 'quotes';

@Component({
  selector: 'app-orders',
  imports: [Badge, Button, Card, QuotePanel, TextField, Paginator],
  templateUrl: './orders.html',
  styleUrl: './orders.css'
})
export class Orders {
  readonly portal = inject(Portal);
  readonly view = input<OrderView>('client');
  readonly roles = Role;
  readonly orderStatus = OrderStatus;
  readonly label = statusLabel;
  readonly tone = statusTone;
  readonly kilometers = formatKilometers;

  readonly page = signal(1);
  amounts: Record<string, number | null> = {};
  actualMileages: Record<string, number | null> = {};
  actualDates: Record<string, string> = {};

  readonly visible = computed(() => {
    const orders = this.portal.orders();
    switch (this.view()) {
      case 'quotes':
        return orders.filter(order => order.status === OrderStatus.Created || order.status === OrderStatus.Quoted);
      case 'assigned':
        return orders.filter(order => order.status !== OrderStatus.Completed && order.status !== OrderStatus.Canceled);
      default:
        return orders;
    }
  });

  readonly currentPage = computed(() => clampPage(this.page(), this.visible().length));
  readonly pageOrders = computed(() => pageOf(this.visible(), this.currentPage()));

  readonly emptyMessage = computed(() => {
    switch (this.view()) {
      case 'quotes':
        return 'No hay órdenes por cotizar ni cotizaciones pendientes.';
      case 'assigned':
        return 'No hay órdenes abiertas.';
      default:
        return 'No hay órdenes. Una lectura de telemetría o la evaluación diaria las generan cuando una rutina vence.';
    }
  });

  setAmount(orderId: string, value: string): void {
    this.amounts[orderId] = parseOptionalNumber(value);
  }

  setMileage(orderId: string, value: string): void {
    this.actualMileages[orderId] = parseOptionalNumber(value);
  }

  setDate(orderId: string, value: string): void {
    this.actualDates[orderId] = value;
  }

  dateOf(orderId: string): string {
    return this.actualDates[orderId] || '';
  }

  canCancel(order: Order): boolean {
    return canCancelOrder(order.status);
  }

  quote(order: Order): void {
    this.portal.quote(order, this.amounts[order.id] ?? null);
  }

  complete(order: Order): void {
    this.portal.complete(order, this.actualMileages[order.id] ?? null, this.actualDates[order.id] ?? '');
  }
}
