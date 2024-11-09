import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SpeedometerComponent } from './speedometer.component';
import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { SpeedometerComponentHarness } from './speedometer.component.harness';
import { Component } from '@angular/core';

describe('SpeedometerComponent tests', () => {
  let fixture: ComponentFixture<SpeedometerTestHostComponent>;
  let component: SpeedometerTestHostComponent;

  beforeEach(() => {
    fixture = TestBed.createComponent(SpeedometerTestHostComponent);
    component = fixture.componentInstance;
  });

  it('should display the current gear', async () => {
    component.gear = 2;

    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, SpeedometerComponentHarness);

    expect(await harness.getGear()).toEqual('2');
  });
});

@Component({
  template: `<app-speedometer [rpm]="rpm" [speed]="speed" [gear]="gear" />`,
  standalone: true,
  imports: [SpeedometerComponent],
})
class SpeedometerTestHostComponent {
  public rpm = 0;
  public speed = 0;
  public gear = 0;
}
