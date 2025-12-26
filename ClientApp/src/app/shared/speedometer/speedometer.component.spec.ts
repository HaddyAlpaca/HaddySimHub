import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SpeedometerComponent } from './speedometer.component';
import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { SpeedometerComponentHarness } from './speedometer.component.harness';
import { inputBinding, provideZonelessChangeDetection } from '@angular/core';
import { describe, beforeEach, it, expect } from 'vitest';

describe('SpeedometerComponent tests', () => {
  let fixture: ComponentFixture<SpeedometerComponent>;
  let rpm: number;
  let speed: number;
  let gear: number;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
      ],
    }).compileComponents();

    rpm = 0;
    speed = 0;
    gear = 0;

    fixture = TestBed.createComponent(SpeedometerComponent, {
      bindings: [
        inputBinding('rpm', () => rpm),
        inputBinding('speed', () => speed),
        inputBinding('gear', () => gear),
      ],
    });
  });

  it('should display the current gear', async () => {
    gear = 2;

    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, SpeedometerComponentHarness);

    expect(await harness.getGear()).toEqual('2');
  });
});
