import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_URL } from '../constants/api';
import { Vehicle } from '../shared/models/vehicle';

@Injectable({ providedIn: 'root' })
export class VehicleService {
  private readonly http = inject(HttpClient);

  list(): Observable<Vehicle[]> {
    return this.http.get<Vehicle[]>(`${API_URL}/api/vehicles`);
  }

  register(plate: string, mileage: number | null): Observable<Vehicle> {
    return this.http.post<Vehicle>(`${API_URL}/api/vehicles`, { plate, mileage });
  }
}
