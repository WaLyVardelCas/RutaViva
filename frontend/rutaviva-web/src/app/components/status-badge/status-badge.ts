import { Component, input } from '@angular/core';
import { statusLabel } from '../../constants/status-labels';

@Component({
  selector: 'app-status-badge',
  template: `<span class="status" [attr.data-status]="status()">{{ label(status()) }}</span>`,
  styleUrl: './status-badge.css'
})
export class StatusBadge {
  readonly status = input.required<string>();
  readonly label = statusLabel;
}
