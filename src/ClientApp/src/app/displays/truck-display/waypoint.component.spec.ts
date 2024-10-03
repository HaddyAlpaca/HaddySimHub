import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { WaypointComponentHarness } from './waypoint.component.harness';
import { WaypointComponent } from './waypoint.component';

describe('WaypointComponent tests', () => {
  let fixture: ComponentFixture<WaypointTestHostComponent>;
  let component: WaypointTestHostComponent;

  beforeEach(() => {
    fixture = TestBed.createComponent(WaypointTestHostComponent);
    component = fixture.componentInstance;
  });

  it('When city is not set a placeholder is shown', async () => {
    component.city = '';

    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, WaypointComponentHarness);
    expect(await harness.getDescription()).toEqual('-');
  });

  it('City is displayed when company is not set', async () => {
    component.city = 'Berlin';

    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, WaypointComponentHarness);
    expect(await harness.getDescription()).toEqual('Berlin');
  });

  it('City and company are displayed both are set', async () => {
    component.city = 'Berlin';
    component.company = 'Company B';

    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, WaypointComponentHarness);
    expect(await harness.getDescription()).toEqual('Berlin (Company B)');
  });
});

@Component({
  template: `<app-waypoint [city]="city" [company]="company" />`,
  standalone: true,
  imports: [WaypointComponent],
})
class WaypointTestHostComponent {
  public city = '';
  public company = '';
}
