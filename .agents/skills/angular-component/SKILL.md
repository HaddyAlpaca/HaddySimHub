# Modern Angular Component Skill

This skill generates Angular components following project conventions.

## Component Checklist

- [ ] Use standalone components (default - no `standalone: true` needed)
- [ ] Set `changeDetection: ChangeDetectionStrategy.OnPush`
- [ ] Use `input()` and `input.required()` for inputs
- [ ] Use `output()` for outputs
- [ ] Use `inject()` instead of constructor injection
- [ ] Use `signal()` and `computed()` for reactive state
- [ ] Use native control flow (`@if`, `@for`, `@switch`)
- [ ] Use `class` bindings, NOT `ngClass`
- [ ] Use `style` bindings, NOT `ngStyle`
- [ ] Use `NgOptimizedImage` for static images

## Essential Imports

```typescript
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { NgOptimizedImage } from '@angular/common';
```

---

## Component Anatomy

```typescript
import { ChangeDetectionStrategy, Component, computed, input, output, inject, signal } from '@angular/core';

@Component({
  selector: 'app-component-name',
  templateUrl: './component-name.component.html',
  styleUrl: './component-name.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [], // pipes, NgOptimizedImage, etc.
})
export class ComponentNameComponent {
  // Inputs
  public readonly value = input<string>();
  public readonly requiredValue = input.required<number>();

  // Outputs
  protected readonly valueChange = output<string>();

  // Signals
  protected readonly internalState = signal<boolean>(false);

  // Computed
  protected readonly derived = computed(() => this.requiredValue() * 2);

  // Injected services
  private readonly _service = inject(SomeService);

  // Methods
  protected handleClick(): void {
    this.internalState.update(v => !v);
    this.valueChange.emit('new value');
  }
}
```

---

## Patterns

### Simple Component with Inputs

```typescript
import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'app-speedometer',
  templateUrl: './speedometer.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SpeedometerComponent {
  public readonly speed = input.required<number>();
  public readonly rpm = input.required<number>();
  public readonly gear = input.required<string>();
}
```

### Component with Computed Values

```typescript
import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-waypoint',
  template: '<div>{{ description() }}</div>',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WaypointComponent {
  public readonly city = input<string>();
  public readonly company = input<string>();

  protected readonly description = computed(() => {
    if (this.city() && this.company()) {
      return `${this.city()} (${this.company()})`;
    }
    return this.city() ?? '-';
  });
}
```

### Component with Injected Service

```typescript
import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { ClockService } from './clock.service';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-clock',
  templateUrl: './clock.component.html',
  imports: [DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClockComponent {
  private readonly _clockService = inject(ClockService);

  protected currentTime = this._clockService.currentTime;
}
```

### Component with Output Event

```typescript
import { ChangeDetectionStrategy, Component, output } from '@angular/core';

@Component({
  selector: 'app-search',
  template: `
    <input (input)="onInput($event)" />
    <button (click)="search.emit(query())">Search</button>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SearchComponent {
  protected readonly query = signal('');

  protected readonly search = output<string>();

  protected onInput(event: Event): void {
    this.query.set((event.target as HTMLInputElement).value);
  }
}
```

### Component with Signal State

```typescript
import { ChangeDetectionStrategy, Component, signal } from '@angular/core';

@Component({
  selector: 'app-toggle',
  template: `
    <button (click)="toggle()">
      {{ isOn() ? 'ON' : 'OFF' }}
    </button>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ToggleComponent {
  protected readonly isOn = signal(false);

  protected toggle(): void {
    this.isOn.update(v => !v);
  }
}
```

---

## Template Patterns

### Native Control Flow

```html
@if (isLoading()) {
  <span>Loading...</span>
} @else if (error()) {
  <span class="error">{{ error() }}</span>
} @else {
  <span>{{ data() }}</span>
}

@for (item of items(); track item.id) {
  <div>{{ item.name }}</div>
} @empty {
  <span>No items</span>
}

@switch (status()) {
  @case ('active') { <span>Active</span> }
  @case ('inactive') { <span>Inactive</span> }
  @default { <span>Unknown</span> }
}
```

### Class Bindings (NOT ngClass)

```html
<button [class.active]="isActive()">Click</button>

<div [class]="'base-class ' + (variant())">Content</div>
```

### Style Bindings (NOT ngStyle)

```html
<div [style.color]="color()">Colored</div>

<div [style.width.px]="width()" [style.height.px]="height()">Sized</div>
```

### NgOptimizedImage

```typescript
import { NgOptimizedImage } from '@angular/common';

@Component({
  imports: [NgOptimizedImage],
})
export class MyComponent {
  protected readonly imageUrl = 'https://example.com/image.png';
}
```

```html
<img [ngSrc]="imageUrl" alt="Description" width="100" height="100" priority />
```

---

## Best Practices

1. **Keep components small** - Single responsibility principle
2. **Use inline templates** for simple components (< 20 lines)
3. **Private members with underscore prefix** (`_service`)
4. **Protected/public for template access** - Use `protected` for methods called from template
5. **Prefer signals over observables** for component state
6. **Always use `track` in `@for`** - Track by unique identifier

## File Structure

```
component-name/
├── component-name.component.ts
├── component-name.component.html (or inline template)
├── component-name.component.scss (optional)
├── component-name.component.spec.ts
└── component-name.component.harness.ts (optional)
```
