import { computed, OnDestroy, Service, signal } from '@angular/core';
import { Subscription, tap, timer } from 'rxjs';

@Service()
export class ClockService implements OnDestroy {
  private readonly _currentTime = signal(new Date());
  private readonly _clockSubscription: Subscription;
  public readonly currentTime = computed(() => this._currentTime());

  public constructor() {
    this._clockSubscription = timer(0, 1000).pipe(
      tap(() => this._currentTime.set(new Date())),
    ).subscribe();
  }

  public ngOnDestroy(): void {
    this._clockSubscription.unsubscribe();
  }
}
