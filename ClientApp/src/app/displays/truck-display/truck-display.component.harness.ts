import { ComponentHarness } from '@angular/cdk/testing';
import { SpeedometerComponentHarness } from 'src/app/shared/speedometer/speedometer.component.harness';

export class TruckDashComponentHarness extends ComponentHarness {
  public static hostSelector = 'app-truck-dash';

  public async getSpeedoHarness(): Promise<SpeedometerComponentHarness> {
    const harness = await this.locatorFor(SpeedometerComponentHarness)();

    return harness;
  }

  public async getWarning(selector: string): Promise<boolean> {
    const elm = await this.locatorFor(`${selector} > img`)();
    const warning = await elm.hasClass('filter-orange');
    return warning;
  }

  public async getElementText(selector: string): Promise<string> {
    const elm = await this.locatorFor(selector)();
    const text = await elm.text();
    return text;
  }
}
