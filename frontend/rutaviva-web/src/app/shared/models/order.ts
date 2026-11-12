import { OrderStatus } from '../../constants/order-status';

export interface Order {
  id: string;
  vehicleId: string;
  routineId: string;
  status: OrderStatus;
  amount: number | null;
  automaticApproval: boolean | null;
  actualMileage: number | null;
  actualDate: string | null;
}
