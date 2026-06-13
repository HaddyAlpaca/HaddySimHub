import { ComponentFixture, TestBed } from '@angular/core/testing';
import { GaugeComponent } from './gauge.component';
import { provideZonelessChangeDetection } from '@angular/core';
import { describe, beforeEach, it, expect } from 'vitest';

describe('GaugeComponent', () => {
  let fixture: ComponentFixture<GaugeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(GaugeComponent);
  });

  describe('degreesNum calculation', () => {
    it('should return -125 for value 0', () => {
      fixture.componentRef.setInput('value', 0);
      fixture.componentRef.setInput('max', 100);
      fixture.detectChanges();

      expect(fixture.componentInstance.degreesNum()).toBe(-125);
    });

    it('should return -125 for value at 0%', () => {
      fixture.componentRef.setInput('value', 0);
      fixture.componentRef.setInput('max', 2400);
      fixture.detectChanges();

      expect(fixture.componentInstance.degreesNum()).toBe(-125);
    });

    it('should return 125 for value at 100%', () => {
      fixture.componentRef.setInput('value', 2400);
      fixture.componentRef.setInput('max', 2400);
      fixture.detectChanges();

      expect(fixture.componentInstance.degreesNum()).toBe(125);
    });

    it('should return 0 for negative value', () => {
      fixture.componentRef.setInput('value', -10);
      fixture.componentRef.setInput('max', 100);
      fixture.detectChanges();

      expect(fixture.componentInstance.degreesNum()).toBe(0);
    });

    it('should return 0 when max is zero or negative', () => {
      fixture.componentRef.setInput('value', 50);
      fixture.componentRef.setInput('max', 0);
      fixture.detectChanges();

      expect(fixture.componentInstance.degreesNum()).toBe(0);
    });

    it('should clamp value above max to 100%', () => {
      fixture.componentRef.setInput('value', 3000);
      fixture.componentRef.setInput('max', 2400);
      fixture.detectChanges();

      expect(fixture.componentInstance.degreesNum()).toBe(125);
    });

    it('should calculate correct degrees for 50% value', () => {
      fixture.componentRef.setInput('value', 1200);
      fixture.componentRef.setInput('max', 2400);
      fixture.detectChanges();

      expect(fixture.componentInstance.degreesNum()).toBe(0);
    });
  });

  describe('max-rpm class', () => {
    it('should apply max-rpm class when value equals max', () => {
      fixture.componentRef.setInput('value', 2400);
      fixture.componentRef.setInput('max', 2400);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const ring = compiled.querySelector('.ring');
      expect(ring?.classList.contains('max-rpm')).toBe(true);
    });

    it('should not apply max-rpm class when value is less than max', () => {
      fixture.componentRef.setInput('value', 2399);
      fixture.componentRef.setInput('max', 2400);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const ring = compiled.querySelector('.ring');
      expect(ring?.classList.contains('max-rpm')).toBe(false);
    });
  });

  describe('needle styling', () => {
    it('should set needle transform via degreesNum', () => {
      fixture.componentRef.setInput('value', 1200);
      fixture.componentRef.setInput('max', 2400);
      fixture.detectChanges();

      expect(fixture.componentInstance.degreesNum()).toBe(0);
    });
  });

  describe('zones', () => {
    it('should return green/yellow/red zones when greenStart and greenEnd are set', () => {
      fixture.componentRef.setInput('value', 1500);
      fixture.componentRef.setInput('max', 3000);
      fixture.componentRef.setInput('greenStart', 1250);
      fixture.componentRef.setInput('greenEnd', 2000);
      fixture.detectChanges();

      const zones = fixture.componentInstance.zones();
      expect(zones.length).toBe(3);
      expect(zones[0].color).toBe('#00cc66');
      expect(zones[1].color).toBe('#ffcc00');
      expect(zones[2].color).toBe('#ff3300');
    });

    it('should return only red zone when greenStart/greenEnd are 0', () => {
      fixture.componentRef.setInput('value', 1500);
      fixture.componentRef.setInput('max', 3000);
      fixture.detectChanges();

      const zones = fixture.componentInstance.zones();
      expect(zones.length).toBe(1);
      expect(zones[0].color).toBe('#ff3300');
    });
  });

  describe('ticks and labels', () => {
    it('should generate ticks for max=3000', () => {
      fixture.componentRef.setInput('value', 0);
      fixture.componentRef.setInput('max', 3000);
      fixture.detectChanges();

      const ticks = fixture.componentInstance.ticks();
      expect(ticks.length).toBeGreaterThan(0);
      expect(ticks.some(t => t.major)).toBe(true);
    });

    it('should generate labels for max=3000', () => {
      fixture.componentRef.setInput('value', 0);
      fixture.componentRef.setInput('max', 3000);
      fixture.detectChanges();

      const labels = fixture.componentInstance.labels();
      expect(labels.length).toBeGreaterThan(0);
      expect(labels[0].text).toBe('0');
    });
  });
});
