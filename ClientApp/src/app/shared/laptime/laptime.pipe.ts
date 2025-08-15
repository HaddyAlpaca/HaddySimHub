import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'laptime',
})
export class LapTimePipe implements PipeTransform {
  public transform(seconds: number): string {
    if (seconds === 0) {
      return '--:--.---';
    }

    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const fullSeconds = Math.floor(seconds % 60);
    const fraction = Math.round((seconds - Math.floor(seconds)) * 1000);

    if (hours > 0) {
      return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${fullSeconds.toString().padStart(2, '0')}.${fraction.toString().padStart(3, '0')}`;
    }

    return `${minutes.toString().padStart(2, '0')}:${fullSeconds.toString().padStart(2, '0')}.${fraction.toString().padStart(3, '0')}`;
  }
}
