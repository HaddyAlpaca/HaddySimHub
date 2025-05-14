import { ChangeDetectionStrategy, Component, Input, input, signal } from '@angular/core';
import { ChartConfiguration } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';

export interface TelemetrySample {
  brakePct: number;
  throttlePct: number;
}

@Component({
  selector: 'app-telemetry-trace',
  templateUrl: './telemetry-trace.component.html',
  styleUrl: './telemetry-trace.component.scss',
  imports: [BaseChartDirective],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TelemetryTraceComponent {
  public maxPoints = input(10000);

  @Input()
  public set telemetrySample(sample: TelemetrySample) {
    if (sample.brakePct === undefined || sample.throttlePct === undefined) {
      return;
    }

    this.addData(sample.throttlePct, sample.brakePct);
  }

  private _labels = signal<number[]>([]);
  private _brakeData = signal<number[]>([]);
  private _throttleData = signal<number[]>([]);
  public readonly chartType = 'line';

  public readonly chartData = signal<ChartConfiguration<'line'>['data']>({
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
    ],
  });

  public readonly chartOptions: ChartConfiguration<'line'>['options'] = {
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

  private addData(throttle: number, brake: number): void {
    const time = new Date().getMilliseconds();

    const labels = [...this._labels(), time];
    const brakeData = [...this._brakeData(), brake];
    const throttleData = [...this._throttleData(), throttle];

    if (labels.length > this.maxPoints()) {
      labels.shift();
      brakeData.shift();
      throttleData.shift();
    }

    this._labels.set(labels);
    this._brakeData.set(brakeData);
    this._throttleData.set(throttleData);

    this.chartData.set({
      labels,
      datasets: [
        { ...this.chartData().datasets[0], data: brakeData },
        { ...this.chartData().datasets[1], data: throttleData },
      ],
    });
  }
}
