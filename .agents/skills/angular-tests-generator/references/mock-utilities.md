# Available Mock Utilities

| Mock | Location | Use For |
|------|----------|---------|
| `MockAppStore` | `src/testing/mock-app.store.ts` | App state management |
| `MockSignalRService` | `src/testing/mock-signalr.service.ts` | SignalR connection |

## MockAppStore

```typescript
import { MockAppStore } from 'src/testing/mock-app.store';

let mockStore = new MockAppStore();
mockStore.displayType.set(DisplayType.Race);
mockStore.truckData.set({ /* truck data */ });
```

## MockSignalRService

```typescript
import { MockSignalRService } from 'src/testing/mock-signalr.service';

let mockSignalRService = new MockSignalRService();
mockSignalRService.connectionStatus.set({ status: ConnectionStatus.Connected });
```
