import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ClockComponent } from './clock.component';
import { ClockService } from './clock.service';
import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { ClockComponentHarness } from './clock.component.harness';
import { provideExperimentalZonelessChangeDetection } from '@angular/core';

describe('ClockComponent tests', () => {
  let fixture: ComponentFixture<ClockComponent>;
  let mockClockService: jasmine.SpyObj<ClockService>;

  beforeEach(async () => {
    mockClockService = jasmine.createSpyObj<ClockService>('ClockService', ['currentTime']);
    mockClockService.currentTime.and.returnValue(new Date(2024, 5, 9, 15, 39, 12));

    await TestBed.configureTestingModule({
      providers: [
        provideExperimentalZonelessChangeDetection(),
        { provide: ClockService, useValue: mockClockService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ClockComponent);
  });

  it('Current time is displayed', async () => {
    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, ClockComponentHarness);

    expect(await harness.getCurrentTime()).toEqual('15:39');
  });
});
