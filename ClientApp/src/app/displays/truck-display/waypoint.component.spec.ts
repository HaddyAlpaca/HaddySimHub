import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { inputBinding, provideZonelessChangeDetection } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { WaypointComponentHarness } from './waypoint.component.harness';
import { WaypointComponent } from './waypoint.component';

describe('WaypointComponent tests', () => {
  let fixture: ComponentFixture<WaypointComponent>;
  let city: string;
  let company: string;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
      ],
    }).compileComponents();

    city = '';
    company = '';

    fixture = TestBed.createComponent(WaypointComponent, {
      bindings: [
        inputBinding('city', () => city),
        inputBinding('company', () => company),
      ],
    });
  });

  it('When city is not set a placeholder is shown', async () => {
    city = '';

    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, WaypointComponentHarness);
    expect(await harness.getDescription()).toEqual('-');
  });

  it('City is displayed when company is not set', async () => {
    city = 'Berlin';

    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, WaypointComponentHarness);
    expect(await harness.getDescription()).toEqual('Berlin');
  });

  it('City and company are displayed both are set', async () => {
    city = 'Berlin';
    company = 'Company B';

    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, WaypointComponentHarness);
    expect(await harness.getDescription()).toEqual('Berlin (Company B)');
  });
});
