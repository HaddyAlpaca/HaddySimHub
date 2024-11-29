import { ComponentHarness } from '@angular/cdk/testing';

export class AppComponentHarness extends ComponentHarness {
  public static hostSelector = 'app-root';

  public async isTruckDisplayVisible(): Promise<boolean> {
    return this.isVisible('app-truck-display');
  }

  public async isRaceDisplayVisible(): Promise<boolean> {
    return this.isVisible('app-race-display');
  }

  public async isRallyDisplayVisible(): Promise<boolean> {
    return this.isVisible('app-rally-display');
  }

  public async isConnectionStatusVisible(): Promise<boolean> {
    return this.isVisible('app-connection-status');
  }

  private async isVisible(selector: string): Promise<boolean> {
    const element = await this.locatorForOptional(selector)();
    return !!element;
  }
}
