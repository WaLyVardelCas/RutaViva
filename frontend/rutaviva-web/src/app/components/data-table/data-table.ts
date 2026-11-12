import { Component, ViewEncapsulation } from '@angular/core';

@Component({
  selector: 'app-data-table',
  template: '<div class="rv-table-scroll"><ng-content /></div>',
  styleUrl: './data-table.css',
  encapsulation: ViewEncapsulation.None
})
export class DataTable {}
