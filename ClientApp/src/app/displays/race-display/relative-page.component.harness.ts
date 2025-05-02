import { ComponentHarness } from '@angular/cdk/testing';

export interface RowData {
  isPlayer: boolean;
  isSafetyCar: boolean;
  isInPits: boolean;
  position: string;
  carNumber: string;
  driverName: string;
  license: string;
  iRating: string;
  timeToPlayer: string;
  isLapBehind: boolean;
  isLapAhead: boolean;
}

export class RelativePageComponentHarness extends ComponentHarness {
  public static hostSelector = 'app-relative-page';

  public async getRows(): Promise<RowData[]> {
    const rows = await this.locatorForAll(RelativePageComponentRowHarness)();
    const timingEntries: RowData[] = await Promise.all(
      rows.map(async row => {
        const isPlayer = await row.isPLayer();
        const isSafetyCar = await row.isSafetyCar();
        const isInPits = await row.isInPits();
        const isLapBehind = await row.isLapBehind();
        const isLapAhead = await row.isLapAhead();
        const cells = await row.getCells();

        return {
          position: cells[0],
          carNumber: cells[1],
          driverName: cells[2],
          license: cells[3],
          iRating: cells[4],
          timeToPlayer: cells[5],
          isPlayer,
          isSafetyCar,
          isInPits,
          isLapBehind,
          isLapAhead,
        };
      }));

    return timingEntries;
  }
}

class RelativePageComponentRowHarness extends ComponentHarness {
  public static hostSelector = 'tbody > tr';

  public async isPLayer(): Promise<boolean> {
    const row = await this.host();
    const isPlayer = await row.hasClass('player');
    return isPlayer;
  }

  public async isSafetyCar(): Promise<boolean> {
    const row = await this.host();
    const isSafetyCar = await row.hasClass('safety-car');
    return isSafetyCar;
  }

  public async isInPits(): Promise<boolean> {
    const row = await this.host();
    const isInPits = await row.hasClass('in-pits');
    return isInPits;
  }

  public async isLapBehind(): Promise<boolean> {
    const row = await this.host();
    const isLapBehind = await row.hasClass('lap-behind');
    return isLapBehind;
  }

  public async isLapAhead(): Promise<boolean> {
    const row = await this.host();
    const isLapAhead = await row.hasClass('lap-ahead');
    return isLapAhead;
  }

  public async getCells(): Promise<string[]> {
    const cells = await this.locatorForAll('td')();
    const cellTexts = await Promise.all(cells.map(cell => cell.text()));
    return cellTexts;
  }
}
