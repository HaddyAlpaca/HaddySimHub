import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'laptime',
    standalone: true,
})
export class LapTimePipe implements PipeTransform {
  transform(seconds: number): string {
    if (seconds === 0) {
      return '--:--.---';
    }

    const minutes = Math.floor(seconds / 60);
    const fullSeconds = Math.floor(seconds % 60);
    const fraction = Math.round((seconds - fullSeconds - (minutes * 60)) * 1000);

    return `${minutes.toString().padStart(2, '0')}:${fullSeconds.toString().padStart(2, '0')}.${fraction.toString().padStart(3, '0')}`;
  }
}
