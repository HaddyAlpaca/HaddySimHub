import { ChangeDetectionStrategy, Component, effect, input, signal, viewChildren, ViewEncapsulation } from '@angular/core';

@Component({
  selector: 'app-rpm-light',
  template: '<div class="light" [style.background-color]="color()" [class.flash]="flash()"></div>',
  encapsulation: ViewEncapsulation.None,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RpmLightComponent {
  public index = input.required<number>();
  public color = signal('black');
  public flash = signal(false);
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

  public lights = input.required<{ rpm: number; color: string }[]>();
  public rpm = input.required<number>();
  public rpmMax = input.required<number>();
  public biDirectional = input<boolean>(false);

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
