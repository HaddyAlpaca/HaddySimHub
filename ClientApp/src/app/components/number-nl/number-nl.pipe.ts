import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'numbernl',
})
export class NumberNlPipe implements PipeTransform {
  private readonly _numberFormat = new Intl.NumberFormat('en-US');

  public transform(value: number): string {
    return this._numberFormat.format(value).replace(',', '.');
  }
}
