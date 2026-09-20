import { describe, expect, it } from 'vitest';
import { ConnectionStatus } from '../../sse.service';
import './connection-status.element';

describe('haddy-connection-status', () => {
  it('renders the connection message and countdown', async () => {
    const element = document.createElement('haddy-connection-status') as HTMLElement & {
      connectionInfo: { status: ConnectionStatus; message: string; reloadSeconds: number };
    };
    document.body.append(element);
    element.connectionInfo = {
      status: ConnectionStatus.ConnectionError,
      message: 'Connection lost',
      reloadSeconds: 7,
    };
    await element.updateComplete;

    expect(element.textContent).toContain('Error connecting');
    expect(element.textContent).toContain('Connection lost');
    expect(element.textContent).toContain('7');
    element.remove();
  });
});
