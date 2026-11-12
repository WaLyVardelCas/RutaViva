import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_URL } from '../constants/api';
import { Order } from '../shared/models/order';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly http = inject(HttpClient);

  list(): Observable<Order[]> {
    return this.http.get<Order[]>(`${API_URL}/api/orders`);
  }

  quote(orderId: string, amount: number | null): Observable<Order> {
    return this.http.post<Order>(`${API_URL}/api/orders/${orderId}/quote`, { amount });
  }

  approve(orderId: string): Observable<Order> {
    return this.http.post<Order>(`${API_URL}/api/orders/${orderId}/approval`, {});
  }

  reject(orderId: string): Observable<Order> {
    return this.http.post<Order>(`${API_URL}/api/orders/${orderId}/rejection`, {});
  }

  cancel(orderId: string): Observable<Order> {
    return this.http.post<Order>(`${API_URL}/api/orders/${orderId}/cancellation`, {});
  }

  start(orderId: string): Observable<Order> {
    return this.http.post<Order>(`${API_URL}/api/orders/${orderId}/start`, {});
  }

  complete(orderId: string, mileage: number | null, date: string): Observable<Order> {
    return this.http.post<Order>(`${API_URL}/api/orders/${orderId}/completion`, { mileage, date });
  }
}
