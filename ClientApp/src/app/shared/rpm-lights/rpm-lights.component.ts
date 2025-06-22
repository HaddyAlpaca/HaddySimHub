import { ChangeDetectionStrategy, Component, effect, input, signal, viewChildren, ViewEncapsulation } from '@angular/core';

@Component({
  selector: 'app-rpm-light',
  template: '<div class="light" [style.background-color]="color()" [class.flash]="flash()"></div>',
  encapsulation: ViewEncapsulation.None,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RpmLightComponent {
  public readonly index = input.required<number>();
  public readonly color = signal('black');
  public readonly flash = signal(false);
}

@Component({
  selector: 'app-rpm-lights',
  templateUrl: './rpm-lights.component.html',
  styleUrl: './rpm-lights.component.scss',
  imports: [RpmLightComponent],
  encapsulation: ViewEncapsulation.None,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RpmLightsComponent {
  private readonly _lightComponents = viewChildren(RpmLightComponent);

  public readonly lights = input.required<{ rpm: number; color: string }[]>();
  public readonly rpm = input.required<number>();
  public readonly rpmMax = input.required<number>();
  public readonly biDirectional = input<boolean>(false);

  public constructor() {
    effect(() => {
      const rpm = this.rpm();
      const rpmMax = this.rpmMax();

      for(const light of this._lightComponents()) {
        const l = this.lights()[light.index()];
        light.color.set(rpm >= l.rpm ? l.color : 'black');
        light.flash.set(rpmMax > 0 && rpm >= rpmMax);
      }
    });
  }
}
