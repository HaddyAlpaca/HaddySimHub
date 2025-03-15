import { ComponentHarness } from '@angular/cdk/testing';

export class ClockComponentHarness extends ComponentHarness {
  public static hostSelector = 'app-clock';

  public async getCurrentTime(): Promise<string> {
    const elm = await this.locatorFor('#currentTime')();
    const text = await elm.text();

    return text;
  }
}
