import { ChangeDetectionStrategy, Component, Input, signal } from '@angular/core';
import { ChartConfiguration } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';

export interface TelemetrySample {
  brakePct: number;
  throttlePct: number;
  steeringPct: number;
}

@Component({
  selector: 'app-telemetry-trace',
  templateUrl: './telemetry-trace.component.html',
  styleUrl: './telemetry-trace.component.scss',
  imports: [BaseChartDirective],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TelemetryTraceComponent {
  private readonly _maxFrames = 1_000;
  private _frame = 0;

  @Input()
  public set telemetrySample(sample: TelemetrySample) {
    if (sample.brakePct === undefined || sample.throttlePct === undefined || sample.steeringPct === undefined) {
      return;
    }

    this.addData(sample.throttlePct, sample.brakePct, sample.steeringPct);
  }

  public constructor() {
    // Initialize the chart with max frames
    this._labels.set(Array.from({ length: this._maxFrames }, (_, i) => i));
    this._brakeData.set(Array.from({ length: this._maxFrames }, (_, _i) => 0));
    this._throttleData.set(Array.from({ length: this._maxFrames }, (_, _i) => 0));
    this._steeringData.set(Array.from({ length: this._maxFrames }, (_, _i) => 50));
  }

  private readonly _labels = signal<number[]>([]);
  private readonly _brakeData = signal<number[]>([]);
  private readonly _throttleData = signal<number[]>([]);
  private readonly _steeringData = signal<number[]>([]);
  protected readonly chartType = 'line';

  protected readonly chartData = signal<ChartConfiguration<'line'>['data']>({
    labels: this._labels(),
    datasets: [
      {
        data: this._brakeData(),
        label: 'Brake',
        borderColor: 'red',
        fill: false,
        pointRadius: 0,
      },
      {
        data: this._throttleData(),
        label: 'Throttle',
        borderColor: 'green',
        fill: false,
        pointRadius: 0,
      },
      {
        data: this._steeringData(),
        label: 'Steering',
        borderColor: 'yellow',
        fill: false,
        pointRadius: 0,
      },
    ],
  });

  protected readonly chartOptions: ChartConfiguration<'line'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    animation: false,
    scales: {
      x: {
        ticks: {
          display: false,
        },
        grid: {
          drawTicks: false,
        },
      },
      y: {
        min: 0,
        max: 100,
        ticks: {
          display: false,
        },
        grid: {
          color: 'gray',
          drawTicks: false,
        },
      },
    },
    plugins: {
      legend: {
        display: false,
      },
    },
  };

  private addData(throttle: number, brake: number, steering: number): void {
    const frame = this._frame++;
    const labels = [...this._labels(), frame];
    const brakeData = [...this._brakeData(), brake];
    const throttleData = [...this._throttleData(), throttle];
    const steeringData = [...this._steeringData(), steering];

    if (labels.length > this._maxFrames) {
      labels.shift();
      brakeData.shift();
      throttleData.shift();
      steeringData.shift();
    }

    this._labels.set(labels);
    this._brakeData.set(brakeData);
    this._throttleData.set(throttleData);
    this._steeringData.set(steeringData);

    this.chartData.set({
      labels,
      datasets: [
        { ...this.chartData().datasets[0], data: brakeData },
        { ...this.chartData().datasets[1], data: throttleData },
        { ...this.chartData().datasets[2], data: steeringData },
      ],
    });
  }
}
