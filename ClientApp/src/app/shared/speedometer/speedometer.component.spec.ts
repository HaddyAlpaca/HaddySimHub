import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SpeedometerComponent } from './speedometer.component';
import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { SpeedometerComponentHarness } from './speedometer.component.harness';
import { Component, provideExperimentalZonelessChangeDetection } from '@angular/core';

describe('SpeedometerComponent tests', () => {
  let fixture: ComponentFixture<SpeedometerTestHostComponent>;
  let component: SpeedometerTestHostComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [
        provideExperimentalZonelessChangeDetection(),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(SpeedometerTestHostComponent);
    component = fixture.componentInstance;
  });

  it('should display the current gear', async () => {
    component.gear = 2;

    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, SpeedometerComponentHarness);

    expect(await harness.getGear()).toEqual('2');
  });

  it('should display the RPM in green when within the green range', async () => {
    component.rpm = 2000;
    component.rpmGreen = 1500;
    
    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, SpeedometerComponentHarness);
    expect(await harness.hasRpmGreen()).toBeTrue();
    expect(await harness.hasRpmRed()).toBeFalse();
    expect(await harness.hasRpmMax()).toBeFalse();
  });

  it('should display the RPM in red when above the red limit', async () => {
    component.rpm = 3000;
    component.rpmRed = 2500;

    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, SpeedometerComponentHarness);
    expect(await harness.hasRpmGreen()).toBeFalse();
    expect(await harness.hasRpmRed()).toBeTrue();
    expect(await harness.hasRpmMax()).toBeFalse();
  });

  it('should display the RPM in max when above the max limit', async () => {
    component.rpm = 5000;
    component.rpmRed = 3500;
    component.rpmMax = 4500;

    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, SpeedometerComponentHarness);
    expect(await harness.hasRpmGreen()).toBeFalse();
    expect(await harness.hasRpmRed()).toBeTrue();
    expect(await harness.hasRpmMax()).toBeTrue();
  });

  it('should not display RPM colors when inpus are nog set', async () => {
    component.rpm = 1000;

    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, SpeedometerComponentHarness);
    expect(await harness.hasRpmGreen()).toBeFalse();
    expect(await harness.hasRpmRed()).toBeFalse();
    expect(await harness.hasRpmMax()).toBeFalse();
  });
});

@Component({
  template: `<app-speedometer [rpm]="rpm" [speed]="speed" [gear]="gear" [rpmGreen]="rpmGreen" [rpmRed]="rpmRed" [rpmMax]="rpmMax" />`,
  imports: [SpeedometerComponent],
})
class SpeedometerTestHostComponent {
  public rpm = 0;
  public speed = 0;
  public gear = 0;
  public rpmGreen = 0;
  public rpmRed = 0;
  public rpmMax = 0;
}
