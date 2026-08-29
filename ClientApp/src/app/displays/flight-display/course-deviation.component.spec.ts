import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { describe, beforeEach, it, expect } from 'vitest';
import { CourseDeviationComponent } from './course-deviation.component';

describe('CourseDeviationComponent', () => {
  let fixture: ComponentFixture<CourseDeviationComponent>;
  let component: CourseDeviationComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection()],
    }).compileComponents();

    fixture = TestBed.createComponent(CourseDeviationComponent);
    component = fixture.componentInstance;
  });

  const render = (lateral: number | null, glideslope: number | null = null): void => {
    fixture.componentRef.setInput('lateral', lateral);
    fixture.componentRef.setInput('glideslope', glideslope);
    fixture.detectChanges();
  };

  it('should centre the needle when on course', () => {
    render(0);

    expect(component.needleX()).toBe(component.centreX);
  });

  it('should swing the needle opposite to the deviation', () => {
    // Right of course means the course is to the left, so the needle goes left.
    render(0.5);

    expect(component.needleX()).toBe(component.centreX - 60);
  });

  it('should swing the needle right when left of course', () => {
    render(-1);

    expect(component.needleX()).toBe(component.centreX + 120);
  });

  it('should keep an unclamped deviation on the scale', () => {
    render(4);

    expect(component.needleX()).toBe(component.centreX - 120);
  });

  it('should drop the glideslope marker when above the path', () => {
    render(0, 1);

    // Screen coordinates grow downwards, so "fly down" is a lower marker.
    expect(component.glideslopeY()).toBe(component.centreY + 32);
  });

  it('should raise the glideslope marker when below the path', () => {
    render(0, -0.5);

    expect(component.glideslopeY()).toBe(component.centreY - 16);
  });

  it('should hide the needle when there is no lateral guidance', () => {
    render(null);

    expect(component.hasLateral()).toBe(false);
    expect((fixture.nativeElement as HTMLElement).querySelector('.needle')).toBeNull();
  });

  it('should hide the glideslope scale when none is received', () => {
    render(0.2);

    expect(component.hasGlideslope()).toBe(false);
    expect((fixture.nativeElement as HTMLElement).querySelector('.glideslope-marker')).toBeNull();
  });

  it('should draw the glideslope scale when one is received', () => {
    render(0.2, 0.1);

    expect(component.hasGlideslope()).toBe(true);
    expect((fixture.nativeElement as HTMLElement).querySelector('.glideslope-marker')).not.toBeNull();
  });

  it('should place four dots either side of centre at half and full scale', () => {
    render(0);

    expect(component.lateralDots()).toEqual([50, 110, 230, 290]);
    expect(component.glideslopeDots()).toEqual([13, 29, 61, 77]);
  });

  it('should always show the fixed aircraft symbol', () => {
    render(null);

    expect((fixture.nativeElement as HTMLElement).querySelector('.aircraft')).not.toBeNull();
  });
});
