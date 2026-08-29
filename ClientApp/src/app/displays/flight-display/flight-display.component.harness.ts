import { ComponentHarness } from '@angular/cdk/testing';

export class FlightDisplayComponentHarness extends ComponentHarness {
  public static hostSelector = 'app-flight-display';

  public async getSpeedPanelText(): Promise<string> {
    return (await this.locatorFor('.speed-panel')()).text();
  }

  public async getAltitudePanelText(): Promise<string> {
    return (await this.locatorFor('.altitude-panel')()).text();
  }

  public async getNavPanelText(): Promise<string> {
    return (await this.locatorFor('.nav-panel')()).text();
  }

  public async getEnginePanelText(): Promise<string> {
    return (await this.locatorFor('.engine-panel')()).text();
  }

  public async getConfigPanelText(): Promise<string> {
    return (await this.locatorFor('.config-panel')()).text();
  }

  public async getAutopilotModes(): Promise<string[]> {
    const modes = await this.locatorForAll('.autopilot-panel .mode')();
    return Promise.all(modes.map((mode) => mode.text()));
  }

  public async hasStallWarning(): Promise<boolean> {
    return (await this.locatorFor('.speed-panel')()).hasClass('warning');
  }

  public async hasOverspeedWarning(): Promise<boolean> {
    return (await this.locatorFor('.speed-panel')()).hasClass('overspeed');
  }

  public async hasLowFuelWarning(): Promise<boolean> {
    const rows = await this.locatorForAll('.engine-panel .bar-row')();
    const flags = await Promise.all(rows.map((row) => row.hasClass('warning')));
    return flags.some(Boolean);
  }

  public async isEnginePanelShown(): Promise<boolean> {
    return !!(await this.locatorForOptional('.bar-fill.engine')());
  }

  public async isFlightPlanShown(): Promise<boolean> {
    return !(await this.locatorForOptional('.nav-empty')());
  }

  public async getLitLights(): Promise<string[]> {
    const lights = await this.locatorForAll('.light-row .light')();
    const lit = await Promise.all(
      lights.map(async (light) => ((await light.hasClass('on')) ? light.text() : null)),
    );
    return lit.filter((name): name is string => name !== null);
  }
}
