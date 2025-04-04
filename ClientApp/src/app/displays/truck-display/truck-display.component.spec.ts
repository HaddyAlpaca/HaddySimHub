import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { TruckData, TruckDisplayComponent } from './truck-display.component';
import { TruckDashComponentHarness } from './truck-display.component.harness';
import { Component, provideExperimentalZonelessChangeDetection, signal } from '@angular/core';

describe('TruckDisplayComponent', () => {
  let fixture: ComponentFixture<TruckDisplayTestComponent>;
  let component: TruckDisplayTestComponent;
  let data: TruckData;
  let harness: TruckDashComponentHarness;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [
        provideExperimentalZonelessChangeDetection(),
      ],
    }).compileComponents();

    //Set default values for the truck data
    data = {} as TruckData;

    fixture = TestBed.createComponent(TruckDisplayTestComponent);
    component = fixture.componentInstance;
    harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, TruckDashComponentHarness);
  });

  it('Gears should be displayed', async () => {
    //Reverse 1
    patchData({ gear: -1 });
    expect(await harness.getElementText('.gear')).toEqual('R1');

    //Neutral
    patchData({ gear: 0 });
    expect(await harness.getElementText('.gear')).toEqual('N');

    //Forward
    patchData({ gear: 7 });
    expect(await harness.getElementText('.gear')).toEqual('7');
  });

  it('Fuel info is displayed', async () => {
    patchData({ fuelDistance: 814, fuelAmount: 1200, fuelWarningOn: false });

    expect(await harness.getElementText('#fuel')).toEqual('814 km (1.200 l)');
    expect(await harness.getWarning('#fuel')).toBe(false);

    patchData({ fuelDistance: 7.2, fuelAmount: 8.3, fuelWarningOn: true });

    expect(await harness.getElementText('#fuel')).toEqual('7.2 km (8.3 l)');
    expect(await harness.getWarning('#fuel')).toBe(true);
  });

  it('AdBlue info is displayed', async () => {
    patchData({ adBlueAmount: 100.2, adBlueWarningOn: false });

    expect(await harness.getElementText('#adBlue')).toEqual('100 l');
    expect(await harness.getWarning('#adBlue')).toBe(false);

    patchData({ adBlueAmount: 7.2, adBlueWarningOn: true });

    expect(await harness.getElementText('#adBlue')).toEqual('7.2 l');
    expect(await harness.getWarning('#adBlue')).toBe(true);
  });

  describe('Job', () => {
    it('Income should display a placeholder when not set', async () => {
      patchData({ jobIncome: 0 });

      expect(await harness.getElementText('#jobIncome')).toEqual('-');
    });

    it('Income should be displayed jobIncome when set', async () => {
      patchData({ jobIncome: 32_145 });

      expect(await harness.getElementText('#jobIncome')).toEqual('€ 32.145');
    });

    it('Cargo name placeholder is shown when no cargo', async () => {
      patchData({ jobCargoName: '' });

      expect(await harness.getElementText('#jobCargoName')).toEqual('-');
    });

    it('Cargo name is shown', async () => {
      patchData({ jobCargoName: 'Helicopter', jobCargoMass: 2500 });

      expect(await harness.getElementText('#jobCargoName')).toEqual('Helicopter (2.500 kg)');
    });
  });

  const patchData = (value: Record<string, unknown>): void => {
    component.data.set({
      ...data,
      ...value,
    });
  };
});

@Component({
  template: '<app-truck-display [data]="data()" />',
  imports: [TruckDisplayComponent],
})
export class TruckDisplayTestComponent {
  public data = signal<TruckData>({} as TruckData);
}
