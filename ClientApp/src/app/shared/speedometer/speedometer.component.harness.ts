import { ComponentHarness } from '@angular/cdk/testing';

export class SpeedometerComponentHarness extends ComponentHarness {
  public static hostSelector = 'app-speedometer';

  public async getSpeed(): Promise<string> {
    return this.getText('.speed');
  }

  public async getRpm(): Promise<string> {
    return this.getText('.rpm');
  }

  public async getGear(): Promise<string> {
    return this.getText('.gear');
  }

  public async hasRpmGreen(): Promise<boolean> {
    const elm = await this.locatorFor('.rpm')();
    return elm.hasClass('rpm-green');
  }

  public async hasRpmRed(): Promise<boolean> {
    const elm = await this.locatorFor('.rpm')();
    return elm.hasClass('rpm-red');
  }

  public async hasRpmMax(): Promise<boolean> {
    const elm = await this.locatorFor('.rpm')();
    return elm.hasClass('rpm-max');
  }

  private async getText(selector: string): Promise<string> {
    const elm = await this.locatorFor(selector)();
    const text = await elm.text();

    return text;
  }
}
