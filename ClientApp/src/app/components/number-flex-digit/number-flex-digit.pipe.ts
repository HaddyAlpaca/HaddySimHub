import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'numberFlexDigit',
})
export class NumberFlexDigitPipe implements PipeTransform {
  private readonly _numberFormat = new Intl.NumberFormat('en-US');

  public transform(value: number): string {
    if (value >= 10) {
      value = Math.floor(value);
    }

    return this._numberFormat.format(value).replace(',', '.');
  }
}
