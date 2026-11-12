import { Component, inject, input } from '@angular/core';
import { Router } from '@angular/router';
import { Button } from '../button/button';
import { Role } from '../../constants/role';
import { Portal } from '../../services/portal';
import { RoleContext } from '../../services/role-context';

@Component({
  selector: 'app-header',
  imports: [Button],
  templateUrl: './header.html',
  styleUrl: './header.css',
  host: {
    '[attr.title]': 'null'
  }
})
export class Header {
  private readonly router = inject(Router);
  readonly portal = inject(Portal);
  readonly roles = inject(RoleContext);

  readonly title = input('Inicio');

  readonly role = Role;

  choose(role: Role): void {
    if (this.roles.role() === role) return;
    const path = role === Role.Workshop ? '/workshop/home' : '/client/home';
    void this.router.navigateByUrl(path);
  }
}
