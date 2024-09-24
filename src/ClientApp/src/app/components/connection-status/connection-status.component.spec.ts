import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ConnectionStatus } from '@services/game-data.service';
import { ConnectionStatusComponent } from './connection-status.component';
import { ConnectionStatusComponentHarness } from './connection-status.component.harness';
import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { Component } from '@angular/core';

describe('ConnectionStatusComponent tests', () => {
  let fixture: ComponentFixture<ConnectionStatusTestComponent>;
  let component: ConnectionStatusTestComponent;

  beforeEach(async () => {
    fixture = TestBed.createComponent(ConnectionStatusTestComponent);
    component = fixture.componentInstance;
  });

  it('Disconnected state is displayed', async () => {
    component.status = { status: ConnectionStatus.Disconnected };
    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, ConnectionStatusComponentHarness);

    expect(await harness.getConnectionStatusText()).toEqual('Disconnected');
  });

  it('Connecting state is displayed', async () => {
    component.status = { status: ConnectionStatus.Connecting };
    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, ConnectionStatusComponentHarness);

    expect(await harness.getConnectionStatusText()).toEqual('Connecting...');
  });

  it('Connection error state is displayed', async () => {
    component.status = { status: ConnectionStatus.ConnectionError };
    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, ConnectionStatusComponentHarness);

    expect(await harness.getConnectionStatusText()).toEqual('Error connecting');
  });

  it('Connected state is displayed', async () => {
    component.status = { status: ConnectionStatus.Connected };
    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, ConnectionStatusComponentHarness);

    expect(await harness.getConnectionStatusText()).toEqual('Connected, waiting for game...');
  });


});

@Component({
  template: '<app-connection-status [status]="status" />',
  standalone: true,
  imports: [ConnectionStatusComponent],
})
export class ConnectionStatusTestComponent {
  public status = { status: ConnectionStatus.Disconnected };
}
