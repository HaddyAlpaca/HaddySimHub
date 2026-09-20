import { afterEach, describe, expect, it, vi } from 'vitest';
import { ConnectionStatus, DisplayType, SseService } from './sse.service';

class EventSourceMock {
  public static instance?: EventSourceMock;
  public static readonly CONNECTING = 0;
  public static readonly OPEN = 1;
  public static readonly CLOSED = 2;
  public readyState = EventSourceMock.CONNECTING;
  public onopen?: () => void;
  public onmessage?: (event: MessageEvent<string>) => void;
  public onerror?: () => void;
  public close = vi.fn();

  public constructor() {
    EventSourceMock.instance = this;
  }
}

describe('SseService', () => {
  afterEach(() => {
    vi.unstubAllGlobals();
    vi.useRealTimers();
  });

  it('publishes connection changes and display messages', () => {
    vi.stubGlobal('EventSource', EventSourceMock);
    const updates: unknown[] = [];
    const statuses: ConnectionStatus[] = [];
    const service = new SseService((update) => updates.push(update));
    const source = EventSourceMock.instance;
    expect(source).toBeDefined();
    if (!source) {
      throw new Error('EventSource was not created');
    }
    const unsubscribe = service.subscribe((info) => statuses.push(info.status));

    source.readyState = EventSourceMock.OPEN;
    source.onopen?.();
    source.onmessage?.({ data: JSON.stringify({ type: DisplayType.RaceDashboard }) } as MessageEvent<string>);

    expect(statuses).toEqual([ConnectionStatus.Connecting, ConnectionStatus.Connected]);
    expect(updates).toEqual([{ type: DisplayType.RaceDashboard }]);
    unsubscribe();
    service.dispose();
  });

  it('reports reconnecting when EventSource enters connecting state', () => {
    vi.stubGlobal('EventSource', EventSourceMock);
    const messages: string[] = [];
    const service = new SseService(() => undefined);
    const source = EventSourceMock.instance;
    expect(source).toBeDefined();
    if (!source) {
      throw new Error('EventSource was not created');
    }
    service.subscribe((info) => {
      if (info.message) {
        messages.push(info.message);
      }
    });

    source.readyState = EventSourceMock.CONNECTING;
    source.onerror?.();

    expect(messages).toEqual(['Reconnecting...']);
    service.dispose();
  });
});
