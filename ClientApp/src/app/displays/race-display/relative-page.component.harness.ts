import { ComponentHarness } from '@angular/cdk/testing';

export interface TimingEntry {
  isPlayer: boolean;
  isSafetyCar: boolean;
  isInPits: boolean;
  position: string;
  carNumber: string;
  driverName: string;
  license: string;
  iRating: string;
  timeToPlayer: string;
}

export class RelativePageComponentHarness extends ComponentHarness {
  public static hostSelector = 'app-relative-page';

  public async getRows(): Promise<TimingEntry[]> {
    const rows = await this.locatorForAll(RelativePageComponentRowHarness)();
    const timingEntries: TimingEntry[] = await Promise.all(
        rows.map(async row => {
            const isPlayer = await row.isPLayer();
            const isSafetyCar = await row.isSafetyCar();
            const isInPits = await row.isInPits();
            const cells = await row.getCells();
    
            return {
                isPlayer,
                isSafetyCar,
                isInPits,
                position: cells[0],
                carNumber: cells[1],
                driverName: cells[2],
                license: cells[3],
                iRating: cells[4],
                timeToPlayer: cells[5],
            };
        }));  
    
    return timingEntries;
  }
}

class RelativePageComponentRowHarness extends ComponentHarness {
  public static hostSelector = 'tr';

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

  public async getCells(): Promise<string[]> {
    const cells = await this.locatorForAll('td')();
    const cellTexts = await Promise.all(cells.map(cell => cell.text()));
    return cellTexts;
  }
}
