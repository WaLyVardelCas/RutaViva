import { Component, inject } from '@angular/core';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-toaster',
  templateUrl: './toaster.html',
  styleUrl: './toaster.css'
})
export class Toaster {
  readonly toasts = inject(ToastService);
}
