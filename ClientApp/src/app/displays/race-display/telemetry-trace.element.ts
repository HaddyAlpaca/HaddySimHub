import { Chart, registerables, type ChartConfiguration } from 'chart.js';
import { LitElement, html, type TemplateResult } from 'lit';
import { customElement } from 'lit/decorators.js';

export interface TelemetrySample {
  brakePct: number;
  throttlePct: number;
  steeringPct: number;
}

Chart.register(...registerables);

@customElement('haddy-telemetry-trace')
export class TelemetryTraceElement extends LitElement {
  private readonly _maxFrames = 1_000;
  private _frame = 0;
  private readonly _labels: number[] = [];
  private readonly _brakeData: number[] = [];
  private readonly _throttleData: number[] = [];
  private readonly _steeringData: number[] = [];
  private _chart?: Chart<'line'>;

  public constructor() {
    super();
    for (let i = 0; i < this._maxFrames; i++) {
      this._labels[i] = i;
      this._brakeData[i] = 0;
      this._throttleData[i] = 0;
      this._steeringData[i] = 50;
    }
  }

  public set telemetrySample(sample: TelemetrySample) {
    if (sample.brakePct === undefined || sample.throttlePct === undefined || sample.steeringPct === undefined) {
      return;
    }

    const idx = this._frame % this._maxFrames;
    this._brakeData[idx] = sample.brakePct;
    this._throttleData[idx] = sample.throttlePct;
    this._steeringData[idx] = sample.steeringPct;
    this._frame++;
    this.requestUpdate();
  }

  public get chartData(): ChartConfiguration<'line'>['data'] {
    const totalFrames = Math.min(this._frame, this._maxFrames);

    return {
      labels: this._labels.slice(0, totalFrames),
      datasets: [
        { data: this._brakeData.slice(0, totalFrames), label: 'Brake', borderColor: 'red', fill: false, pointRadius: 0 },
        { data: this._throttleData.slice(0, totalFrames), label: 'Throttle', borderColor: 'green', fill: false, pointRadius: 0 },
        { data: this._steeringData.slice(0, totalFrames), label: 'Steering', borderColor: 'yellow', fill: false, pointRadius: 0 },
      ],
    };
  }

  protected createRenderRoot(): HTMLElement {
    return this;
  }

  protected render(): TemplateResult {
    return html`<div class="chart-container"><canvas></canvas></div>`;
  }

  protected updated(): void {
    const canvas = this.querySelector('canvas');
    const context = canvas?.getContext('2d');
    if (!context) {
      return;
    }

    const config: ChartConfiguration<'line'> = {
      type: 'line',
      data: this.chartData,
      options: {
        responsive: true,
        maintainAspectRatio: false,
        animation: false,
        scales: {
          x: { ticks: { display: false }, grid: { drawTicks: false } },
          y: { min: 0, max: 100, ticks: { display: false }, grid: { color: 'gray', drawTicks: false } },
        },
        plugins: { legend: { display: false } },
      },
    };

    if (this._chart) {
      this._chart.data = config.data;
      this._chart.update();
    } else {
      this._chart = new Chart(context, config);
    }
  }

  public disconnectedCallback(): void {
    this._chart?.destroy();
    super.disconnectedCallback();
  }
}
