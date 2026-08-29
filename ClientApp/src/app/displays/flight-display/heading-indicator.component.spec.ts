import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { describe, beforeEach, it, expect } from 'vitest';
import { HeadingIndicatorComponent } from './heading-indicator.component';

describe('HeadingIndicatorComponent', () => {
  let fixture: ComponentFixture<HeadingIndicatorComponent>;
  let component: HeadingIndicatorComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection()],
    }).compileComponents();

    fixture = TestBed.createComponent(HeadingIndicatorComponent);
    component = fixture.componentInstance;
  });

  const render = (heading: number, headingBug: number | null = null, groundTrack: number | null = null): void => {
    fixture.componentRef.setInput('heading', heading);
    fixture.componentRef.setInput('headingBug', headingBug);
    fixture.componentRef.setInput('groundTrack', groundTrack);
    fixture.detectChanges();
  };

  it('should place the current heading under the centre pointer', () => {
    render(90);

    const centreTick = component.ticks().find((tick) => tick.x === 300);
    expect(centreTick?.label).toBe('09');
  });

  it('should put ticks on absolute multiples of five degrees, not relative to the heading', () => {
    render(92);

    // 92 is not a multiple of 5, so no tick lands exactly on the pointer.
    expect(component.ticks().some((tick) => tick.x === 300)).toBe(false);
    // 90 sits two degrees left of centre, at 6 px per degree.
    expect(component.ticks().find((tick) => tick.label === '09')?.x).toBe(288);
  });

  it('should wrap tick labels past north', () => {
    render(357);

    const labels = component.ticks().filter((tick) => tick.label).map((tick) => tick.label);
    expect(labels).toContain('35');
    expect(labels).toContain('00');
    expect(labels).toContain('01');
  });

  it('should take the short way round for a bug just past north', () => {
    render(350, 10);

    // 010 is 20 degrees right of 350, not 340 degrees left.
    expect(component.bugMarker()).toEqual({ x: 420, clamped: false });
  });

  it('should pin an off-tape bug to the edge and flag it', () => {
    render(90, 200);

    const marker = component.bugMarker();
    expect(marker?.clamped).toBe(true);
    // Clamped to the 48 degree half span: 300 + 48 * 6.
    expect(marker?.x).toBe(588);
  });

  it('should place the ground track marker relative to the heading', () => {
    render(90, null, 96);

    expect(component.trackMarker()).toEqual({ x: 336, clamped: false });
  });

  it('should hide the markers that have no value', () => {
    render(90);

    expect(component.bugMarker()).toBeNull();
    expect(component.trackMarker()).toBeNull();

    const svg = fixture.nativeElement as HTMLElement;
    expect(svg.querySelector('.bug-marker')).toBeNull();
    expect(svg.querySelector('.track-marker')).toBeNull();
  });

  it('should render both markers when they have values', () => {
    render(90, 100, 96);

    const svg = fixture.nativeElement as HTMLElement;
    expect(svg.querySelector('.bug-marker')).not.toBeNull();
    expect(svg.querySelector('.track-marker')).not.toBeNull();
  });
});
