export const PAGE_SIZE = 5;

export function pageCount(total: number, pageSize = PAGE_SIZE): number {
  return Math.max(1, Math.ceil(total / pageSize));
}

export function clampPage(page: number, total: number, pageSize = PAGE_SIZE): number {
  return Math.min(Math.max(page, 1), pageCount(total, pageSize));
}

export function pageOf<T>(items: readonly T[], page: number, pageSize = PAGE_SIZE): T[] {
  const current = clampPage(page, items.length, pageSize);
  const start = (current - 1) * pageSize;
  return items.slice(start, start + pageSize);
}
