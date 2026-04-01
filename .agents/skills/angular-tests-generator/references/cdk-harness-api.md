# CDK Harness API

## Base Methods

| Method | Purpose |
|--------|---------|
| `locatorFor(selector)` | Find single element (throws if not found) |
| `locatorForOptional(selector)` | Find single element (null if not found) |
| `locatorForAll(selector)` | Find all matching elements |
| `hasClass(className)` | Check if host has class |

## Harness Template

```typescript
import { ComponentHarness } from '@angular/cdk/testing';

export class ComponentNameHarness extends ComponentHarness {
  public static hostSelector = 'app-component-name';

  protected getValueLocator = this.locatorFor('.value-class');

  public async getValue(): Promise<string> {
    const elm = await this.getValueLocator();
    return elm.text();
  }

  public async hasStatus(): Promise<boolean> {
    return this.locatorFor('.status')().then(() => true).catch(() => false);
  }
}
```
