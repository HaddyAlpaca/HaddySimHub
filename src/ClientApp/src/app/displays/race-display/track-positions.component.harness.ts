import { ComponentHarness } from '@angular/cdk/testing';

export class TrackPositionsComponentHarness extends ComponentHarness {
  static hostSelector = 'app-track-positions';

  public async getTrackItems(): Promise<{ style: string | null }[]> {
    const elements = await this.locatorForAll('div.track-item')();

    const result: { style: string | null }[] = [];
    for (const elm of elements) {
      const style = await elm.getAttribute('style');
      result.push({ style });
    }

    return result;
  }
}
