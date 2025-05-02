import { Component, AfterViewInit, ViewChild, ElementRef, input, effect, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-telemetry-trace',
  templateUrl: './telemetry-trace.component.html',
  styleUrl: './telemetry-trace.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TelemetryTraceComponent implements AfterViewInit {
  @ViewChild('canvas', { static: false })
  private _canvasRef!: ElementRef<HTMLCanvasElement>;

  private _ctx!: CanvasRenderingContext2D;
  private _brakeDataPoints: number[] = [];
  private _throttleDataPoints: number[] = [];

  public brakePct = input.required<number>();
  public throttlePct = input.required<number>();

  public ngAfterViewInit(): void {
    const canvas = this._canvasRef.nativeElement;
    const context = canvas.getContext('2d');
    if (!context) {
      console.error('Failed to get canvas context. Telemetry trace will not be displayed.');
      return;
    }
    this._ctx = context;
  }

  public constructor() {
    effect(() => {
      this.addDataPoint(this.brakePct(), this.throttlePct());
    });
  }

  private addDataPoint(brake: number, throttle: number): void {
    this._brakeDataPoints.push(brake);
    this._throttleDataPoints.push(throttle);

    // Maintain a fixed-size buffer by shifting out the oldest data when necessary
    const maxDataPoints = this.getMaxDataPoints();
    if (this._brakeDataPoints.length > maxDataPoints) {
      this._brakeDataPoints.shift();
    }
    if (this._throttleDataPoints.length > maxDataPoints) {
      this._throttleDataPoints.shift();
    }

    this.drawTraces();
  }

  private drawTraces(): void {
    const canvas = this._canvasRef?.nativeElement;
    const ctx = this._ctx;

    if (!canvas || !ctx) {
      return;
    }

    // Clear the entire canvas
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    const maxDataPoints = this.getMaxDataPoints();
    const widthPerPoint = maxDataPoints > 1 ? canvas.width / (maxDataPoints - 1) : 0;

    const drawTrace = (dataPoints: number[], color: string): void => {
      ctx.beginPath();
      for (let i = 0; i < dataPoints.length; i++) {
        const x = i * widthPerPoint;
        // Convert percentage to y coordinate (invert y axis so that 100 is at the top)
        const y = canvas.height - (dataPoints[i] / 100) * canvas.height;
        if (i === 0) {
          ctx.moveTo(x, y);
        } else {
          ctx.lineTo(x, y);
        }
      }
      ctx.strokeStyle = color;
      ctx.lineWidth = 2;
      ctx.stroke();
    };

    // Draw Brake Trace in Red
    drawTrace(this._brakeDataPoints, 'red');

    // Draw Throttle Trace in Green
    drawTrace(this._throttleDataPoints, 'green');
  }

  private getMaxDataPoints(): number {
    return this._canvasRef?.nativeElement.width ?? 500;
  }
}
