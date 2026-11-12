import { RoutineStatus } from '../../constants/routine-status';

export interface Routine {
  id: string;
  name: string;
  kilometers: number | null;
  months: number | null;
  cycleNumber: number;
  referenceMileage: number;
  referenceDate: string;
  mileageLimit: number | null;
  dateLimit: string | null;
  status: RoutineStatus;
}
