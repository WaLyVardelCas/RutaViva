import { Injectable, signal } from '@angular/core';
import { Role } from '../constants/role';

@Injectable({ providedIn: 'root' })
export class RoleContext {
  readonly role = signal<Role>(Role.Client);
}
