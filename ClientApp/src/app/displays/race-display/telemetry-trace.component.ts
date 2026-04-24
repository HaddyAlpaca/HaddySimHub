import { ChangeDetectionStrategy, Component, Input, OnInit, signal } from '@angular/core';
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
export class TelemetryTraceComponent implements OnInit {
  private readonly _maxFrames = 1_000;
  private _frame = 0;
  private readonly _labels: number[] = [];
  private readonly _brakeData: number[] = [];
  private readonly _throttleData: number[] = [];
  private readonly _steeringData: number[] = [];

  @Input()
  public set telemetrySample(sample: TelemetrySample) {
    if (sample.brakePct === undefined || sample.throttlePct === undefined || sample.steeringPct === undefined) {
      return;
    }

    this.addData(sample.throttlePct, sample.brakePct, sample.steeringPct);
  }

  public ngOnInit(): void {
    for (let i = 0; i < this._maxFrames; i++) {
      this._labels[i] = i;
      this._brakeData[i] = 0;
      this._throttleData[i] = 0;
      this._steeringData[i] = 50;
    }
  }

  protected readonly chartType = 'line';

  protected readonly chartData = signal<ChartConfiguration<'line'>['data']>({
    labels: [],
    datasets: [
      {
        data: [],
        label: 'Brake',
        borderColor: 'red',
        fill: false,
        pointRadius: 0,
      },
      {
        data: [],
        label: 'Throttle',
        borderColor: 'green',
        fill: false,
        pointRadius: 0,
      },
      {
        data: [],
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
    const idx = this._frame % this._maxFrames;
    this._brakeData[idx] = brake;
    this._throttleData[idx] = throttle;
    this._steeringData[idx] = steering;
    this._frame++;

    const totalFrames = Math.min(this._frame, this._maxFrames);
    const labels = this._labels.slice(0, totalFrames);
    const brakeData = this._brakeData.slice(0, totalFrames);
    const throttleData = this._throttleData.slice(0, totalFrames);
    const steeringData = this._steeringData.slice(0, totalFrames);

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
