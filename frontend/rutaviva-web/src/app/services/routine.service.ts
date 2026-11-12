import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_URL } from '../constants/api';
import { Routine } from '../shared/models/routine';

@Injectable({ providedIn: 'root' })
export class RoutineService {
  private readonly http = inject(HttpClient);

  list(vehicleId: string): Observable<Routine[]> {
    return this.http.get<Routine[]>(`${API_URL}/api/vehicles/${vehicleId}/routines`);
  }

  add(vehicleId: string, name: string, kilometers: number | null, months: number | null): Observable<Routine> {
    return this.http.post<Routine>(`${API_URL}/api/vehicles/${vehicleId}/routines`, {
      name,
      kilometers,
      months
    });
  }
}
