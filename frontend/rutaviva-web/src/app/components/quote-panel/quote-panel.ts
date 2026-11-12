import { Component, input, output } from '@angular/core';
import { Button } from '../button/button';
import { TextField } from '../text-field/text-field';
import { OrderStatus } from '../../constants/order-status';
import { Role } from '../../constants/role';
import { formatMoney, parseOptionalNumber } from '../../shared/format';
import { Order } from '../../shared/models/order';

@Component({
  selector: 'app-quote-panel',
  imports: [Button, TextField],
  templateUrl: './quote-panel.html',
  styleUrl: './quote-panel.css'
})
export class QuotePanel {
  readonly order = input.required<Order>();
  readonly role = input.required<Role>();
  readonly amount = input<number | null>(null);
  readonly amountChange = output<number | null>();
  readonly send = output<void>();
  readonly approve = output<void>();
  readonly reject = output<void>();

  readonly roles = Role;
  readonly orderStatus = OrderStatus;
  readonly money = formatMoney;

  changeAmount(value: string): void {
    this.amountChange.emit(parseOptionalNumber(value));
  }
}
