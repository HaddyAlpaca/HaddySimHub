import { ComponentHarness } from '@angular/cdk/testing';

export class SpeedometerComponentHarness extends ComponentHarness {
  public static hostSelector = 'app-speedometer';

  public async getSpeed(): Promise<string> {
    return await this.getText('.speed');
  }

  public async getRpm(): Promise<string> {
    return await this.getText('.rpm');
  }

  public async getGear(): Promise<string> {
    return await this.getText('.gear');
  }

  private async getText(selector: string): Promise<string> {
    const elm = await this.locatorFor(selector)();
    const text = await elm.text();

    return text;
  }

}
