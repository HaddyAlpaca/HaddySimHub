import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { describe, beforeEach, it, expect } from 'vitest';
import { AttitudeIndicatorComponent } from './attitude-indicator.component';

describe('AttitudeIndicatorComponent', () => {
  let fixture: ComponentFixture<AttitudeIndicatorComponent>;
  let component: AttitudeIndicatorComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection()],
    }).compileComponents();

    fixture = TestBed.createComponent(AttitudeIndicatorComponent);
    component = fixture.componentInstance;
  });

  const render = (pitch: number, bank: number, slip = 0): void => {
    fixture.componentRef.setInput('pitch', pitch);
    fixture.componentRef.setInput('bank', bank);
    fixture.componentRef.setInput('slip', slip);
    fixture.detectChanges();
  };

  it('should sit level with no pitch or bank', () => {
    render(0, 0);

    expect(component.horizonTransform()).toBe('rotate(0 100 100) translate(0 0.0)');
  });

  it('should push the horizon down when the nose comes up', () => {
    render(10, 0);

    // 10 degrees at 2.4 px per degree.
    expect(component.horizonTransform()).toBe('rotate(0 100 100) translate(0 24.0)');
  });

  it('should pull the horizon up when the nose drops', () => {
    render(-5, 0);

    expect(component.horizonTransform()).toBe('rotate(0 100 100) translate(0 -12.0)');
  });

  it('should roll the world against the bank', () => {
    render(0, 20);

    // Banking right rolls the aircraft clockwise, so the world rolls anticlockwise.
    expect(component.horizonTransform()).toContain('rotate(-20 100 100)');
    expect(component.bankPointerTransform()).toBe('rotate(-20 100 100)');
  });

  it('should draw a symmetric pitch ladder around the horizon', () => {
    render(0, 0);

    const rungs = component.ladder();
    expect(rungs.map((rung) => rung.label)).toEqual(['30', '20', '10', '10', '20', '30']);
    // The +10 and -10 rungs sit the same distance either side of the horizon at y=100.
    const tenUp = rungs.find((rung) => rung.y < 100 && rung.label === '10');
    const tenDown = rungs.find((rung) => rung.y > 100 && rung.label === '10');
    expect(tenUp?.y).toBe(76);
    expect(tenDown?.y).toBe(124);
  });

  it('should centre the slip ball when the turn is coordinated', () => {
    render(0, 0, 0);

    expect(component.slipBallX()).toBe(100);
  });

  it('should clamp the slip ball to the tube', () => {
    render(0, 0, 3);
    expect(component.slipBallX()).toBe(122);

    render(0, 0, -3);
    expect(component.slipBallX()).toBe(78);
  });

  it('should render the horizon transform onto the svg', () => {
    render(4, 10);

    const group = (fixture.nativeElement as HTMLElement).querySelector('svg g g');
    expect(group?.getAttribute('transform')).toBe(component.horizonTransform());
  });
});
