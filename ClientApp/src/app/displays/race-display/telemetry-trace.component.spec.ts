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

  describe('chart line rendering', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should have brake dataset with red border color', () => {
      const chartData = (fixture.componentInstance as unknown as { chartData: () => { datasets: { label: string; borderColor: string }[] } }).chartData();
      const brakeDataset = chartData.datasets[0];
      expect(brakeDataset.label).toBe('Brake');
      expect(brakeDataset.borderColor).toBe('red');
    });

    it('should have throttle dataset with green border color', () => {
      const chartData = (fixture.componentInstance as unknown as { chartData: () => { datasets: { label: string; borderColor: string }[] } }).chartData();
      const throttleDataset = chartData.datasets[1];
      expect(throttleDataset.label).toBe('Throttle');
      expect(throttleDataset.borderColor).toBe('green');
    });

    it('should have steering dataset with yellow border color', () => {
      const chartData = (fixture.componentInstance as unknown as { chartData: () => { datasets: { label: string; borderColor: string }[] } }).chartData();
      const steeringDataset = chartData.datasets[2];
      expect(steeringDataset.label).toBe('Steering');
      expect(steeringDataset.borderColor).toBe('yellow');
    });

    it('should update brake line when telemetry sample is received', () => {
      const component = fixture.componentInstance;
      component.telemetrySample = { brakePct: 80, throttlePct: 20, steeringPct: 50 };
      fixture.detectChanges();

      const chartData = (fixture.componentInstance as unknown as { chartData: () => { datasets: { data: number[] }[] } }).chartData();
      const brakeData = chartData.datasets[0].data;
      expect(brakeData[brakeData.length - 1]).toBe(80);
    });

    it('should update throttle line when telemetry sample is received', () => {
      const component = fixture.componentInstance;
      component.telemetrySample = { brakePct: 30, throttlePct: 90, steeringPct: 50 };
      fixture.detectChanges();

      const chartData = (fixture.componentInstance as unknown as { chartData: () => { datasets: { data: number[] }[] } }).chartData();
      const throttleData = chartData.datasets[1].data;
      expect(throttleData[throttleData.length - 1]).toBe(90);
    });

    it('should update steering line when telemetry sample is received', () => {
      const component = fixture.componentInstance;
      component.telemetrySample = { brakePct: 0, throttlePct: 50, steeringPct: -25 };
      fixture.detectChanges();

      const chartData = (fixture.componentInstance as unknown as { chartData: () => { datasets: { data: number[] }[] } }).chartData();
      const steeringData = chartData.datasets[2].data;
      expect(steeringData[steeringData.length - 1]).toBe(-25);
    });

    it('should add multiple data points to all lines', () => {
      const component = fixture.componentInstance;
      component.telemetrySample = { brakePct: 10, throttlePct: 20, steeringPct: 30 };
      fixture.detectChanges();
      component.telemetrySample = { brakePct: 40, throttlePct: 50, steeringPct: 60 };
      fixture.detectChanges();
      component.telemetrySample = { brakePct: 70, throttlePct: 80, steeringPct: 90 };
      fixture.detectChanges();

      const chartData = (fixture.componentInstance as unknown as { chartData: () => { datasets: { data: number[] }[] } }).chartData();
      expect(chartData.datasets[0].data.length).toBe(3);
      expect(chartData.datasets[1].data.length).toBe(3);
      expect(chartData.datasets[2].data.length).toBe(3);
    });

    it('should have all datasets configured with pointRadius 0 for line rendering', () => {
      const chartData = (fixture.componentInstance as unknown as { chartData: () => { datasets: { pointRadius: number }[] } }).chartData();
      chartData.datasets.forEach(dataset => {
        expect(dataset.pointRadius).toBe(0);
      });
    });

    it('should have all datasets configured with fill false', () => {
      const chartData = (fixture.componentInstance as unknown as { chartData: () => { datasets: { fill: boolean }[] } }).chartData();
      chartData.datasets.forEach(dataset => {
        expect(dataset.fill).toBe(false);
      });
    });
  });
});
