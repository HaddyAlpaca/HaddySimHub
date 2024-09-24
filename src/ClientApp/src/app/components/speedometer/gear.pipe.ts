import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'gear',
    standalone: true,
})
export class GearPipe implements PipeTransform {

  transform(value: number, multiReverse = false): string {
    if (value === undefined) return '?';

    //Neutral
    if (value === 0) return 'N';

    //N/A (Automatic)
    if (value === 999) return 'N/A';

    //Reserve
    if (value < 0) return multiReverse ? `R${Math.abs(value)}` : `R`;

    return value.toString();
  }
}
