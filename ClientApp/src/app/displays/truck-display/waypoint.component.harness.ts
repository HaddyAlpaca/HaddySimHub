import { ComponentHarness } from '@angular/cdk/testing';

export class WaypointComponentHarness extends ComponentHarness{
  public static hostSelector = 'app-waypoint';

  public async getDescription(): Promise<string> {
    const elm = await this.locatorFor('.data-item')();
    const text = await elm.text();

    return text;
  }
}
