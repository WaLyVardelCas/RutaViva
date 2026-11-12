import { Component, DestroyRef, HostListener, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { filter, map, startWith } from 'rxjs';
import { Role } from '../../constants/role';
import { Portal } from '../../services/portal';
import { RoleContext } from '../../services/role-context';
import { Toaster } from '../toast/toaster';
import { Header } from './header';
import { Sidebar } from './sidebar';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, Sidebar, Header, Toaster],
  templateUrl: './shell.html',
  styleUrl: './shell.css'
})
export class Shell {
  private readonly router = inject(Router);
  private readonly roles = inject(RoleContext);
  readonly portal = inject(Portal);
  private readonly destroyRef = inject(DestroyRef);
  private readonly mobileQuery = window.matchMedia('(max-width: 959px)');

  readonly menuOpen = signal(false);
  readonly mobile = signal(this.mobileQuery.matches);
  readonly heading = toSignal(
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
      startWith(null),
      map(() => this.currentHeading())
    ),
    { initialValue: 'Inicio' }
  );

  constructor() {
    const onChange = (event: MediaQueryListEvent) => {
      this.mobile.set(event.matches);
      if (!event.matches) this.menuOpen.set(false);
    };
    this.mobileQuery.addEventListener('change', onChange);
    this.destroyRef.onDestroy(() => this.mobileQuery.removeEventListener('change', onChange));

    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
      startWith(null),
      takeUntilDestroyed()
    ).subscribe(() => {
      const role = this.router.url.startsWith('/workshop') ? Role.Workshop : Role.Client;
      if (this.roles.role() !== role) this.roles.role.set(role);
      this.closeAfterNavigation();
    });

    this.portal.load();
  }

  @HostListener('document:keydown.escape')
  closeOnEscape(): void {
    this.menuOpen.set(false);
  }

  toggleMenu(): void {
    this.menuOpen.update(open => !open);
  }

  closeAfterNavigation(): void {
    if (this.mobile()) this.menuOpen.set(false);
  }

  private currentHeading(): string {
    let route = this.router.routerState.snapshot.root;
    let heading = 'Inicio';
    while (route) {
      const value = route.data['heading'];
      if (typeof value === 'string' && value) heading = value;
      if (!route.firstChild) break;
      route = route.firstChild;
    }
    return heading;
  }
}
