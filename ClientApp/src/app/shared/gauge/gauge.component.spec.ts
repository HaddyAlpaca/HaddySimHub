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

  describe('degrees calculation', () => {
    it('should return -125 degrees for value 0', () => {
      fixture.componentRef.setInput('value', 0);
      fixture.componentRef.setInput('max', 100);
      fixture.detectChanges();

      expect(fixture.componentInstance.degrees()).toBe('-125deg');
    });

    it('should return -125 degrees for value at 0%', () => {
      fixture.componentRef.setInput('value', 0);
      fixture.componentRef.setInput('max', 2400);
      fixture.detectChanges();

      expect(fixture.componentInstance.degrees()).toBe('-125deg');
    });

    it('should return 125 degrees for value at 100%', () => {
      fixture.componentRef.setInput('value', 2400);
      fixture.componentRef.setInput('max', 2400);
      fixture.detectChanges();

      expect(fixture.componentInstance.degrees()).toBe('125deg');
    });

    it('should return 0 for negative value', () => {
      fixture.componentRef.setInput('value', -10);
      fixture.componentRef.setInput('max', 100);
      fixture.detectChanges();

      expect(fixture.componentInstance.degrees()).toBe(0);
    });

    it('should return 0 when max is zero or negative', () => {
      fixture.componentRef.setInput('value', 50);
      fixture.componentRef.setInput('max', 0);
      fixture.detectChanges();

      expect(fixture.componentInstance.degrees()).toBe(0);
    });

    it('should clamp value above max to 100%', () => {
      fixture.componentRef.setInput('value', 3000);
      fixture.componentRef.setInput('max', 2400);
      fixture.detectChanges();

      expect(fixture.componentInstance.degrees()).toBe('125deg');
    });

    it('should calculate correct degrees for 50% value', () => {
      fixture.componentRef.setInput('value', 1200);
      fixture.componentRef.setInput('max', 2400);
      fixture.detectChanges();

      expect(fixture.componentInstance.degrees()).toBe('0deg');
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
    it('should apply correct needle-deg style', () => {
      fixture.componentRef.setInput('value', 1200);
      fixture.componentRef.setInput('max', 2400);
      fixture.detectChanges();

      expect(fixture.componentInstance.degrees()).toBe('0deg');
    });
  });
});
