import { ComponentHarness } from '@angular/cdk/testing';

export class ConnectionStatusComponentHarness extends ComponentHarness {
  static hostSelector = 'app-connection-status';

  public async getConnectionStatusText(): Promise<string> {
    const elm = await this.locatorFor('.connection-state')();
    const text = await elm.text();
    return text;
  }
}
