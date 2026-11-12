import { HttpErrorResponse } from '@angular/common/http';
import { Problem } from '../shared/models/problem';

export function problemDetail(error: unknown): string {
  if (error instanceof HttpErrorResponse) {
    const problem = error.error as Problem | null;
    return problem?.detail ?? 'La operación fue rechazada.';
  }
  return 'No se pudo contactar la API.';
}
