import { ComponentHarness } from '@angular/cdk/testing';
import { DataElementComponentHarness } from 'src/app/shared/data-element/data-element.component.harness';
import { SpeedometerComponentHarness } from 'src/app/shared/speedometer/speedometer.component.harness';

export class RaceDisplayComponentHarness extends ComponentHarness {
  public static hostSelector = 'app-race-display';

  public async getSpeedoHarness(): Promise<SpeedometerComponentHarness> {
    const harness = await this.locatorFor(SpeedometerComponentHarness)();

    return harness;
  }

  public async elementHasClass(selector: string, className: string): Promise<boolean> {
    const elm = await this.locatorFor(selector)();
    const hidden = await elm.hasClass(className);
    return hidden;
  }

  public async getDataElementHarness(id: string): Promise<DataElementComponentHarness> {
    const harness = await this.locatorFor(DataElementComponentHarness.with({ id }))();
    return harness;
  }

  public async getElementText(selector: string): Promise<string> {
    const elm = await this.locatorFor(selector)();
    const text = await elm.text();
    return text;
  }

  public async hasElement(selector: string): Promise<boolean> {
    const elm = await this.locatorForOptional(selector)();
    return !!elm;
  }
}
