import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'deltatime',
})
export class DeltaTimePipe implements PipeTransform {
  private readonly _numberFormat = new Intl.NumberFormat('en-US', { minimumFractionDigits: 1 });

  public transform(value: number | undefined, showSign = true): string {
    if (!value) {
      return '';
    }

    if (showSign) {
      return (value >= 0 ? '+' : '') + this._numberFormat.format(value);
    } else {
      return this._numberFormat.format(Math.abs(value));
    }
  }
}
