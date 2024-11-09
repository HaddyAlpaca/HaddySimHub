import { formatNumber } from '@angular/common';
import { Component, ViewEncapsulation, computed, input } from '@angular/core';
import { TimespanPipe } from '@displays/truck-display/timespan.pipe';
import { LapTimePipe } from '@components/laptime/laptime.pipe';
import { DeltaTimePipe } from '@components/delta-time/delta-time.pipe';

export enum DataType {
  Unknown,
  Timespan,
  LapTime,
  DeltaTime,
  NumberOneDecimal,
}

@Component({
  selector: 'app-data-element',
  templateUrl: './data-element.component.html',
  standalone: true,
  encapsulation: ViewEncapsulation.None,
})
export class DataElementComponent {
  public description = input.required<string>();
  public value = input.required<number | string>();
  public total = input<number>();
  public unit = input<string>();
  public dataType = input(DataType.Unknown);

  public formattedValue = computed(() => {
    let val = this.value();

    switch(this.dataType()) {
      case DataType.Timespan:
        val = new TimespanPipe().transform(val as number);
        break;

      case DataType.LapTime:
        val = new LapTimePipe().transform(val as number);
        break;

      case DataType.DeltaTime:
        val = new DeltaTimePipe().transform(val as number);
        break;

      case DataType.NumberOneDecimal:
        val = formatNumber(val as number, 'en-US', '1.1-1');
        break;
    }

    const total = this.total();
    if (total) {
      val += '/' + total;
    }

    const unit = this.unit();
    if (unit !== undefined) {
      val += ' ' + unit;
    }

    if (val === undefined) {
      return '';
    }

    return val.toString();
  });
}
