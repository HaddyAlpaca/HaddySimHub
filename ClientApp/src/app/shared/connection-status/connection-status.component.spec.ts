import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ConnectionStatusComponent } from './connection-status.component';
import { ConnectionStatusComponentHarness } from './connection-status.component.harness';
import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { provideZonelessChangeDetection } from '@angular/core';
import { MockSignalRService } from 'src/testing/mock-signalr.service';
import { ConnectionStatus, SignalRService } from 'src/app/signalr.service';

describe('ConnectionStatusComponent tests', () => {
  let fixture: ComponentFixture<ConnectionStatusComponent>;
  let mockSignalRService: MockSignalRService;

  beforeEach(async () => {
    mockSignalRService = new MockSignalRService();

    await TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        { provide: SignalRService, useValue: mockSignalRService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ConnectionStatusComponent);
  });

  it('Disconnected state is displayed', async () => {
    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, ConnectionStatusComponentHarness);
    mockSignalRService.connectionStatus.set({ status: ConnectionStatus.Disconnected });

    expect(await harness.getConnectionStatusText()).toEqual('Disconnected');
  });

  it('Connecting state is displayed', async () => {
    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, ConnectionStatusComponentHarness);
    mockSignalRService.connectionStatus.set({ status: ConnectionStatus.Connecting });

    expect(await harness.getConnectionStatusText()).toEqual('Connecting...');
  });

  it('Connection error state is displayed', async () => {
    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, ConnectionStatusComponentHarness);
    mockSignalRService.connectionStatus.set({ status: ConnectionStatus.ConnectionError });

    expect(await harness.getConnectionStatusText()).toEqual('Error connecting');
  });

  it('Connected state is displayed', async () => {
    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, ConnectionStatusComponentHarness);
    mockSignalRService.connectionStatus.set({ status: ConnectionStatus.Connected });

    expect(await harness.getConnectionStatusText()).toEqual('Connected, waiting for game...');
  });
});
