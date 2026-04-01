import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { TelemetryTraceComponent, TelemetrySample } from './telemetry-trace.component';
import { TelemetryTraceComponentHarness } from './telemetry-trace.component.harness';
import { provideZonelessChangeDetection } from '@angular/core';
import { provideCharts, withDefaultRegisterables } from 'ng2-charts';
import { describe, beforeEach, it, expect } from 'vitest';

describe('TelemetryTraceComponent', () => {
  let fixture: ComponentFixture<TelemetryTraceComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [
        provideCharts(withDefaultRegisterables()),
        provideZonelessChangeDetection(),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(TelemetryTraceComponent);
  });

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should set chart type to line', () => {
    expect((fixture.componentInstance as unknown as { chartType: string }).chartType).toBe('line');
  });

  it('should render the chart container', async () => {
    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, TelemetryTraceComponentHarness);
    fixture.detectChanges();

    const hasChartContainer = await harness.hasChartContainer();
    expect(hasChartContainer).toBe(true);
  });

  it('should have chart options configured', () => {
    const chartOptions = (fixture.componentInstance as unknown as { chartOptions: unknown }).chartOptions;
    expect(chartOptions).toBeTruthy();
    expect((chartOptions as { responsive: boolean }).responsive).toBe(true);
    expect((chartOptions as { animation: boolean }).animation).toBe(false);
  });

  describe('telemetrySample input', () => {
    it('should ignore samples with undefined values', () => {
      const component = fixture.componentInstance;
      const sample: TelemetrySample = {
        brakePct: undefined as unknown as number,
        throttlePct: 50,
        steeringPct: 50,
      };

      expect(() => {
        component.telemetrySample = sample;
        fixture.detectChanges();
      }).not.toThrow();
    });

    it('should accept valid samples', () => {
      const component = fixture.componentInstance;
      const sample: TelemetrySample = {
        brakePct: 50,
        throttlePct: 75,
        steeringPct: 50,
      };

      expect(() => {
        component.telemetrySample = sample;
        fixture.detectChanges();
      }).not.toThrow();
    });
  });
});
