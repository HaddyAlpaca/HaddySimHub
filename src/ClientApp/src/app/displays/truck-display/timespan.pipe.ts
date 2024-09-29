import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'timespan',
  standalone: true,
})
export class TimespanPipe implements PipeTransform {
  public transform(value: number): string {
    const totalMinutes = Math.abs(value);
    const hours = Math.floor(totalMinutes / 60);
    const minutes = Math.abs(totalMinutes % 60);

    const sign = Math.sign(value) === -1 ? '-' : '';
    if (hours) {
      return `${sign}${hours}h ${minutes}min`;
    } else {
      return `${sign}${minutes}min`;
    }
  }
}
