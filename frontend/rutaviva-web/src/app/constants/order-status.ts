export const OrderStatus = {
  Created: 'Created',
  Quoted: 'Quoted',
  Approved: 'Approved',
  InProgress: 'InProgress',
  Completed: 'Completed',
  Canceled: 'Canceled'
} as const;

export type OrderStatus = (typeof OrderStatus)[keyof typeof OrderStatus];

const cancelable: readonly OrderStatus[] = [
  OrderStatus.Created,
  OrderStatus.Quoted,
  OrderStatus.Approved
];

export function canCancelOrder(status: OrderStatus): boolean {
  return cancelable.includes(status);
}
