import { Component, computed, inject, input, output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { navigationFor } from '../../constants/navigation';
import { Role } from '../../constants/role';
import { RoleContext } from '../../services/role-context';
import { NavIconMark } from './nav-icon';

@Component({
  selector: 'app-sidebar',
  imports: [RouterLink, RouterLinkActive, NavIconMark],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css',
  host: {
    id: 'sidebar',
    '[class.expanded]': 'open()'
  }
})
export class Sidebar {
  private readonly roles = inject(RoleContext);

  readonly open = input(false);
  readonly navigated = output<void>();
  readonly toggled = output<void>();

  readonly items = computed(() => navigationFor(this.roles.role()));
  readonly home = computed(() => (this.roles.role() === Role.Workshop ? '/workshop/home' : '/client/home'));
  readonly viewName = computed(() => (this.roles.role() === Role.Workshop ? 'Vista de taller' : 'Vista de cliente'));
}
