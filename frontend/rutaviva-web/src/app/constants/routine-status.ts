export const RoutineStatus = {
  UpToDate: 'UpToDate',
  DueSoon: 'DueSoon',
  Overdue: 'Overdue'
} as const;

export type RoutineStatus = (typeof RoutineStatus)[keyof typeof RoutineStatus];
