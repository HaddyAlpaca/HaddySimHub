import { ComponentHarness } from '@angular/cdk/testing';

export class GaugeComponentHarness extends ComponentHarness {
  public static hostSelector = 'app-gauge';

  public async hasMaxRpmClass(): Promise<boolean> {
    const ring = await this.locatorFor('.ring')();
    return ring.hasClass('max-rpm');
  }
}
