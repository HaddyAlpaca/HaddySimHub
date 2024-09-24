import { SpeedometerComponentHarness } from '@components/speedometer/speedometer.component.harness';
import { ComponentHarnessBase } from '@testing/component-harness-base';

export class TruckDashComponentHarness extends ComponentHarnessBase {
  static hostSelector = 'app-truck-dash';

  public async getSpeedoHarness(): Promise<SpeedometerComponentHarness> {
    const harness = await this.locatorFor(SpeedometerComponentHarness)();

    return harness;
  }

  public async getWarning(selector: string): Promise<boolean> {
    const elm = await this.locatorFor(`${selector} > img`)();
    const warning = await elm.hasClass('filter-orange');
    return warning;
  }
}
