import { Component, input } from '@angular/core';
import { NavIcon } from '../../constants/navigation';

@Component({
  selector: 'app-nav-icon',
  templateUrl: './nav-icon.html',
  styles: [`
    :host {
      display: inline-flex;
      flex: none;
    }

    svg {
      width: 22px;
      height: 22px;
    }
  `]
})
export class NavIconMark {
  readonly icon = input.required<NavIcon>();
}
