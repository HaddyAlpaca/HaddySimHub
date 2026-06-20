import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { TruckData, TruckDisplayComponent } from './truck-display.component';
import { TruckDashComponentHarness } from './truck-display.component.harness';
import { provideZonelessChangeDetection } from '@angular/core';
import { MockAppStore } from '../../../testing/mock-app.store';
import { APP_STORE } from '../../state/app.store';
import { describe, beforeEach, it, expect } from 'vitest';

describe('TruckDisplayComponent', () => {
  let fixture: ComponentFixture<TruckDisplayComponent>;
  let harness: TruckDashComponentHarness;
  let mockStore: MockAppStore;

  beforeEach(async () => {
    mockStore = new MockAppStore();

    await TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        { provide: APP_STORE, useValue: mockStore },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(TruckDisplayComponent);
    harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, TruckDashComponentHarness);
  });

  it('Gears should be displayed', async () => {
    //Reverse 1
    patchData({ gear: 'R1' });
    expect(await harness.getElementText('.gear')).toEqual('R1');

    //Neutral
    patchData({ gear: 'N' });
    expect(await harness.getElementText('.gear')).toEqual('N');

    //Forward
    patchData({ gear: '7' });
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

  describe('Damage highlight', () => {
    it('No severity class below the warning threshold', async () => {
      patchData({ damageTruckEngine: 10 });

      const elm = await harness.locatorForElement('#damageTruckEngine');
      expect(await elm.hasClass('damage-warning')).toBe(false);
      expect(await elm.hasClass('damage-critical')).toBe(false);
    });

    it('Warning class between the warning and critical thresholds', async () => {
      patchData({ damageTruckEngine: 30 });

      const elm = await harness.locatorForElement('#damageTruckEngine');
      expect(await elm.hasClass('damage-warning')).toBe(true);
      expect(await elm.hasClass('damage-critical')).toBe(false);
    });

    it('Critical class at or above the critical threshold', async () => {
      patchData({ damageTrailerCargo: 80 });

      const elm = await harness.locatorForElement('#damageTrailerCargo');
      expect(await elm.hasClass('damage-warning')).toBe(false);
      expect(await elm.hasClass('damage-critical')).toBe(true);
    });
  });

  describe('Fuel reachability', () => {
    it('No warning when fuel range covers the remaining distance', async () => {
      patchData({ fuelDistance: 800, distanceRemaining: 500 });

      const elm = await harness.locatorForElement('#fuel > div');
      expect(await elm.hasClass('fuel-range-too-short')).toBe(false);
    });

    it('Warning when fuel range is below the remaining distance', async () => {
      patchData({ fuelDistance: 200, distanceRemaining: 500 });

      const elm = await harness.locatorForElement('#fuel > div');
      expect(await elm.hasClass('fuel-range-too-short')).toBe(true);
    });

    it('No warning when there is no active route', async () => {
      patchData({ fuelDistance: 0, distanceRemaining: 0 });

      const elm = await harness.locatorForElement('#fuel > div');
      expect(await elm.hasClass('fuel-range-too-short')).toBe(false);
    });
  });

  const patchData = (value: Record<string, unknown>): void => {
    mockStore.truckData.set(value as unknown as TruckData);
  };
});
