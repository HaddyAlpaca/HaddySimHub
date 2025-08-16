import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'time',
})
export class TimePipe implements PipeTransform {
  public transform(seconds: number): string {
    // Conver the total seconds to a formatted time string
    const totalSeconds = Math.abs(seconds);
    const hours = Math.floor(totalSeconds / 3600);
    const minutes = Math.floor((totalSeconds % 3600) / 60);
    const secs = Math.floor(totalSeconds % 60);
    const sign = seconds < 0 ? '-' : '';
    const paddedHours = hours.toString().padStart(2, '0');
    const paddedMinutes = minutes.toString().padStart(2, '0');
    const paddedSeconds = secs.toString().padStart(2, '0');

    if (hours === 0) {
      return `${sign}${paddedMinutes}:${paddedSeconds}`;
    }

    return `${sign}${paddedHours}:${paddedMinutes}:${paddedSeconds}`;
  }
}
