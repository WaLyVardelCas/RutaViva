import { Role } from './role';

export type NavIcon = 'home' | 'vehicles' | 'routines' | 'orders' | 'quotes';

export interface AppSection {
  path: string;
  heading: string;
  documentTitle: string;
  description: string;
  icon: NavIcon;
}

export interface NavItem {
  label: string;
  path: string;
  icon: NavIcon;
}

export const clientSections: AppSection[] = [
  {
    path: 'home',
    heading: 'Inicio',
    documentTitle: 'Inicio · RutaViva',
    description: '',
    icon: 'home'
  },
  {
    path: 'vehicles',
    heading: 'Mis vehículos',
    documentTitle: 'Mis vehículos · RutaViva',
    description: '',
    icon: 'vehicles'
  },
  {
    path: 'orders',
    heading: 'Órdenes de servicio',
    documentTitle: 'Órdenes de servicio · RutaViva',
    description: '',
    icon: 'orders'
  }
];

export const workshopSections: AppSection[] = [
  {
    path: 'home',
    heading: 'Inicio',
    documentTitle: 'Inicio · RutaViva',
    description: '',
    icon: 'home'
  },
  {
    path: 'routines',
    heading: 'Rutinas',
    documentTitle: 'Rutinas · RutaViva',
    description: '',
    icon: 'routines'
  },
  {
    path: 'orders',
    heading: 'Órdenes asignadas',
    documentTitle: 'Órdenes asignadas · RutaViva',
    description: '',
    icon: 'orders'
  }
];

export function navigationFor(role: Role): NavItem[] {
  const sections = role === 'workshop' ? workshopSections : clientSections;
  const prefix = role === 'workshop' ? '/workshop' : '/client';
  return sections.map(section => ({
    label: section.heading,
    path: `${prefix}/${section.path}`,
    icon: section.icon
  }));
}
