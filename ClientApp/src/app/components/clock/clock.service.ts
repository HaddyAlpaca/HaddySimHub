import { computed, Injectable, signal } from '@angular/core';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { tap, timer } from 'rxjs';

@UntilDestroy()
@Injectable({
  providedIn: 'root',
})
export class ClockService {
  private readonly _currentTime = signal(new Date());
  public readonly currentTime = computed(() => this._currentTime());

  public constructor() {
    timer(0, 1000).pipe(
      tap(() => this._currentTime.set(new Date())),
      untilDestroyed(this),
    ).subscribe();
  }
}
