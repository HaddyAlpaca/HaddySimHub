import { ComponentHarness } from '@angular/cdk/testing';

export class RallyDisplayComponentHarness extends ComponentHarness {
  public static hostSelector = 'app-rally-display';

  public async getCompletedText(): Promise<string> {
    const element = await this.locatorFor(':contains("Completed")')();
    return element.text();
  }

  public async getTravelledText(): Promise<string> {
    const element = await this.locatorFor(':contains("Travelled")')();
    return element.text();
  }

  public async getSector1Text(): Promise<string> {
    const element = await this.locatorFor(':contains("Sector 1")')();
    return element.text();
  }

  public async getSector2Text(): Promise<string> {
    const element = await this.locatorFor(':contains("Sector 2")')();
    return element.text();
  }

  public async hasMaxRpmClass(): Promise<boolean> {
    const element = await this.locatorFor('.speedometer')();
    return element.hasClass('max-rpm');
  }
}
