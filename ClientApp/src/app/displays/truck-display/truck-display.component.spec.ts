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
      patchData({ jobCargoName: 'Helicopter', jobCargoMass: 2500, jobCargoDamage: 0 });

      expect(await harness.getElementText('#jobCargoName')).toEqual('Helicopter (2.500 kg) · 0% schade');
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

      const elm = await harness.locatorForElement('#fuel .fuel-info > div');
      expect(await elm.hasClass('fuel-range-too-short')).toBe(false);
    });

    it('Warning when fuel range is below the remaining distance', async () => {
      patchData({ fuelDistance: 200, distanceRemaining: 500 });

      const elm = await harness.locatorForElement('#fuel .fuel-info > div');
      expect(await elm.hasClass('fuel-range-too-short')).toBe(true);
    });

    it('No warning when there is no active route', async () => {
      patchData({ fuelDistance: 0, distanceRemaining: 0 });

      const elm = await harness.locatorForElement('#fuel .fuel-info > div');
      expect(await elm.hasClass('fuel-range-too-short')).toBe(false);
    });
  });

  describe('Arrival time', () => {
    it('Shows clock time from game time plus remaining route time', async () => {
      patchData({ gameTime: 13 * 60 + 30, timeRemaining: 90 });

      expect(await harness.getElementText('#arrivalTime')).toContain('15:00');
    });

    it('Wraps past midnight', async () => {
      patchData({ gameTime: 23 * 60, timeRemaining: 120 });

      expect(await harness.getElementText('#arrivalTime')).toContain('01:00');
    });
  });

  describe('Rest stop conflict', () => {
    it('No warning when arrival is before the next rest', async () => {
      patchData({ restTimeRemaining: 180, timeRemaining: 90 });

      const elm = await harness.locatorForElement('#nextRest');
      expect(await elm.hasClass('rest-before-arrival')).toBe(false);
    });

    it('Warning when a rest is required before arrival', async () => {
      patchData({ restTimeRemaining: 60, timeRemaining: 120 });

      const elm = await harness.locatorForElement('#nextRest');
      expect(await elm.hasClass('rest-before-arrival')).toBe(true);
    });
  });

  describe('Gear advice', () => {
    it('Shows advice when the recommended gear differs from the current gear', async () => {
      patchData({ gear: '7', recommendedGear: '9' });

      expect(await harness.getElementText('#gear-advice')).toContain('9');
      const elm = await harness.locatorForElement('#gear-advice');
      expect(await elm.hasClass('hidden')).toBe(false);
    });

    it('Shows an up arrow when the recommended gear is higher', async () => {
      patchData({ gear: '7', recommendedGear: '9' });

      const arrow = await harness.locatorForElement('#shift-arrow');
      expect(await arrow.hasClass('shift-arrow-up')).toBe(true);
      expect(await arrow.hasClass('shift-arrow-down')).toBe(false);
    });

    it('Shows a down arrow when the recommended gear is lower', async () => {
      patchData({ gear: '9', recommendedGear: '7' });

      const arrow = await harness.locatorForElement('#shift-arrow');
      expect(await arrow.hasClass('shift-arrow-up')).toBe(false);
      expect(await arrow.hasClass('shift-arrow-down')).toBe(true);
    });

    it('Shows no arrow direction when the current gear is not numeric', async () => {
      patchData({ gear: 'C1', recommendedGear: '2' });

      const arrow = await harness.locatorForElement('#shift-arrow');
      expect(await arrow.hasClass('shift-arrow-up')).toBe(false);
      expect(await arrow.hasClass('shift-arrow-down')).toBe(false);
    });

    it('Hides advice when the recommended gear matches the current gear', async () => {
      patchData({ gear: '9', recommendedGear: '9' });

      const elm = await harness.locatorForElement('#gear-advice');
      expect(await elm.hasClass('hidden')).toBe(true);
    });

    it('Hides advice when there is no recommendation', async () => {
      patchData({ gear: '9', recommendedGear: '' });

      const elm = await harness.locatorForElement('#gear-advice');
      expect(await elm.hasClass('hidden')).toBe(true);
    });
  });

  const patchData = (value: Record<string, unknown>): void => {
    mockStore.truckData.set(value as unknown as TruckData);
  };
});
