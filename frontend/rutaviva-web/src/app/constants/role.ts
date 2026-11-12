export const Role = {
  Client: 'client',
  Workshop: 'workshop'
} as const;

export type Role = (typeof Role)[keyof typeof Role];
