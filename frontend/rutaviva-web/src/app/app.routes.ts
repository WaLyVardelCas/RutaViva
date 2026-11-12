import { Routes } from '@angular/router';
import { Dashboard } from './features/dashboard/dashboard';
import { Fleet } from './features/fleet/fleet';
import { Orders } from './features/orders/orders';
import { Routines } from './features/routines/routines';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'client/home' },
  {
    path: 'client',
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'home' },
      { path: 'home', component: Dashboard, title: 'Inicio · RutaViva', data: { heading: 'Inicio' } },
      { path: 'vehicles', component: Fleet, title: 'Mis vehículos · RutaViva', data: { heading: 'Mis vehículos' } },
      { path: 'routines', component: Routines, title: 'Rutinas · RutaViva', data: { heading: 'Rutinas' } },
      {
        path: 'orders',
        component: Orders,
        title: 'Órdenes de servicio · RutaViva',
        data: { heading: 'Órdenes de servicio', view: 'client' }
      }
    ]
  },
  {
    path: 'workshop',
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'home' },
      { path: 'home', component: Dashboard, title: 'Inicio · RutaViva', data: { heading: 'Inicio' } },
      { path: 'routines', component: Routines, title: 'Rutinas · RutaViva', data: { heading: 'Rutinas' } },
      {
        path: 'orders',
        component: Orders,
        title: 'Órdenes asignadas · RutaViva',
        data: { heading: 'Órdenes asignadas', view: 'assigned' }
      },
      {
        path: 'quotes',
        component: Orders,
        title: 'Cotizaciones · RutaViva',
        data: { heading: 'Cotizaciones', view: 'quotes' }
      },
      { path: 'in-progress', redirectTo: 'orders' }
    ]
  },
  { path: '**', redirectTo: 'client/home' }
];
