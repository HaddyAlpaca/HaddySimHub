import { ComponentHarness } from '@angular/cdk/testing';

export class TelemetryTraceComponentHarness extends ComponentHarness {
  public static hostSelector = 'app-telemetry-trace';

  public async hasChartContainer(): Promise<boolean> {
    const container = await this.locatorForOptional('.chart-container')();
    return !!container;
  }

  public async hasCanvas(): Promise<boolean> {
    const canvas = await this.locatorForOptional('canvas')();
    return !!canvas;
  }
}
