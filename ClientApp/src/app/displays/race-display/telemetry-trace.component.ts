import { Component, AfterViewInit, ViewChild, ElementRef, input, effect } from '@angular/core';

@Component({
  selector: 'app-telemetry-trace',
  templateUrl: './telemetry-trace.component.html',
  styleUrl: './telemetry-trace.component.scss',
})
export class TelemetryTraceComponent implements AfterViewInit {
  @ViewChild('canvas', { static: false })
  public canvasRef!: ElementRef<HTMLCanvasElement>;

  private _ctx!: CanvasRenderingContext2D;
  private _brakeDataPoints: number[] = [];
  private _throttleDataPoints: number[] = [];

  public maxDataPoints = input(100);
  public brakePct = input.required<number>();
  public throttlePct = input.required<number>();

  public ngAfterViewInit(): void {
    const canvas = this.canvasRef.nativeElement;
    const context = canvas.getContext('2d');
    if (!context) {
      throw new Error('Failed to get canvas context');
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
    if (this._brakeDataPoints.length > this.maxDataPoints()) {
      this._brakeDataPoints.shift();
    }
    if (this._throttleDataPoints.length > this.maxDataPoints()) {
      this._throttleDataPoints.shift();
    }

    this.drawTraces();
  }

  private drawTraces(): void {
    const canvas = this.canvasRef.nativeElement;
    const ctx = this._ctx;
    // Clear the entire canvas
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    const widthPerPoint = this.maxDataPoints() > 1 ? canvas.width / (this.maxDataPoints() - 1) : 0;

    // Draw Brake Trace in Red
    ctx.beginPath();
    for (let i = 0; i < this._brakeDataPoints.length; i++) {
      const x = i * widthPerPoint;
      // Convert brake percentage to y coordinate (invert y axis so that 100 is at the top)
      const y = canvas.height - (this._brakeDataPoints[i] / 100) * canvas.height;
      if (i === 0) {
        ctx.moveTo(x, y);
      } else {
        ctx.lineTo(x, y);
      }
    }
    ctx.strokeStyle = 'red';
    ctx.lineWidth = 2;
    ctx.stroke();

    // Draw Throttle Trace in Blue
    ctx.beginPath();
    for (let i = 0; i < this._throttleDataPoints.length; i++) {
      const x = i * widthPerPoint;
      // Convert throttle percentage to y coordinate (invert y axis so that 100 is at the top)
      const y = canvas.height - (this._throttleDataPoints[i] / 100) * canvas.height;
      if (i === 0) {
        ctx.moveTo(x, y);
      } else {
        ctx.lineTo(x, y);
      }
    }
    ctx.strokeStyle = 'blue';
    ctx.lineWidth = 2;
    ctx.stroke();
  }
}
