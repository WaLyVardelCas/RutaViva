import { BadgeTone } from '../shared/badge-tone';

const labels: Record<string, string> = {
  UpToDate: 'Al día',
  DueSoon: 'Por vencer',
  Overdue: 'Vencida',
  Created: 'Creada',
  Quoted: 'Cotizada',
  Approved: 'Aprobada',
  InProgress: 'En ejecución',
  Completed: 'Completada',
  Canceled: 'Cancelada'
};

export function statusLabel(status: string): string {
  return labels[status] ?? status;
}

export function statusTone(status: string): BadgeTone {
  switch (status) {
    case 'UpToDate':
    case 'Approved':
    case 'Completed':
      return 'success';
    case 'DueSoon':
    case 'Quoted':
      return 'warning';
    case 'Overdue':
    case 'Canceled':
      return 'danger';
    case 'InProgress':
      return 'progress';
    default:
      return 'info';
  }
}
