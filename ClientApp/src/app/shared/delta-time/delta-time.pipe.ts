import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'deltatime',
})
export class DeltaTimePipe implements PipeTransform {
  public transform(value: number | undefined, fractionDigits = 3): string {
    if (!value) {
      return '';
    }

    const numberFormat = new Intl.NumberFormat('en-US', { minimumFractionDigits: fractionDigits });

    return (value >= 0 ? '+' : '') + numberFormat.format(value);
  }
}
