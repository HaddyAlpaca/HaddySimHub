import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'numbernl',
    standalone: true,
})
export class NumberNlPipe implements PipeTransform {
  private readonly _numberFormat = new Intl.NumberFormat('en-US');

  transform(value: number): string {
    return this._numberFormat.format(value).replace(',', '.');
  }
}
