import { Routine } from './models/routine';

const cop = new Intl.NumberFormat('es-CO', {
  style: 'currency',
  currency: 'COP',
  maximumFractionDigits: 0
});

export function formatMoney(amount: number | null): string {
  if (amount == null) return 'Sin cotización';
  return cop.format(amount);
}

export function formatKilometers(value: number): string {
  return value.toLocaleString('es-CO');
}

export function parseOptionalNumber(value: string): number | null {
  if (value.trim() === '') return null;
  const parsed = Number(value);
  return Number.isFinite(parsed) ? parsed : null;
}

export function formatInterval(routine: Routine): string {
  const parts = [];
  if (routine.kilometers != null) parts.push(`${formatKilometers(routine.kilometers)} km`);
  if (routine.months != null) parts.push(`${routine.months} meses`);
  return parts.join(' o ');
}
