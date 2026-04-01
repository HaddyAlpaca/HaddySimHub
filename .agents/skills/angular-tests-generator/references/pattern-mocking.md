# Mocking Patterns

## Mock Service with vitest-mock-extended

```typescript
import { mock, MockedObject } from 'vitest-mock-extended';
import { ServiceName } from './service-name.service';

let mockService: MockedObject<ServiceName>;

beforeEach(async () => {
  mockService = mock<ServiceName>();
  mockService.getData.mockReturnValue({ key: 'value' });

  await TestBed.configureTestingModule({
    providers: [
      provideZonelessChangeDetection(),
      { provide: ServiceName, useValue: mockService },
    ],
  }).compileComponents();
});
```

## Mock AppStore + SignalR

```typescript
import { MockAppStore } from 'src/testing/mock-app.store';
import { MockSignalRService } from 'src/testing/mock-signalr.service';

let mockStore: MockAppStore;
let mockSignalRService: MockSignalRService;

beforeEach(async () => {
  mockStore = new MockAppStore();
  mockStore.displayType.set(DisplayType.Race);
  mockSignalRService = new MockSignalRService();

  await TestBed.configureTestingModule({
    providers: [
      provideZonelessChangeDetection(),
      { provide: AppStore, useValue: mockStore },
      { provide: SignalRService, useValue: mockSignalRService },
    ],
  }).compileComponents();
});
```

## Signal-based Service Mock

```typescript
import { signal } from '@angular/core';

export class MockSomeService {
  public data = signal<DataType>({} as DataType);

  public getData() {
    return this.data();
  }

  public updateData(value: DataType): void {
    this.data.set(value);
  }
}
```
