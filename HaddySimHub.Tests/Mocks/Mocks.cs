using HaddySimHub.Interfaces;
using HaddySimHub.Models;
using HaddySimHub.Displays;
using System;
using System.Net.Sockets;
using SCSSdkClient; // Added this line
using SCSSdkClient.Object;

namespace HaddySimHub.Tests.Mocks
{
    // Mocks for IGameDataProvider
    public class MockGameDataProvider<T> : IGameDataProvider<T>
    {
        public event EventHandler<T>? DataReceived;
        public void Start() { }
        public void Stop() { }
        public void InvokeDataReceived(T data) => DataReceived?.Invoke(this, data);
    }

    // Mocks for IDataConverter
    public class MockDataConverter<TInput, TOutput> : IDataConverter<TInput, TOutput>
    {
        private readonly Func<TInput, TOutput> _convertFunc;
        public MockDataConverter(Func<TInput, TOutput> convertFunc)
        {
            _convertFunc = convertFunc;
        }
        public TOutput Convert(TInput input) => _convertFunc(input);
    }

    // Specific Mock for IUdpClientFactory (as it's still used by Dirt2GameDataProvider)
    public class MockUdpClientFactory : IUdpClientFactory
    {
        public UdpClient Create(int port) => null!;
    }
    
    // Specific Mock for ISCSTelemetryFactory (as it's still used by EtsGameDataProvider)
    public class MockSCSTelemetryFactory : ISCSTelemetryFactory
    {
        public SCSSdkTelemetry Create() => null!; // Return null or a mock SCSSdkTelemetry
    }

    /// <summary>
    /// Stands in for the native SimConnect connection so the MSFS provider can be
    /// tested without Windows or the simulator.
    /// </summary>
    public class MockSimConnectClient : HaddySimHub.Displays.Msfs.ISimConnectClient
    {
        private HaddySimHub.Displays.Msfs.MsfsTelemetry? _pending;

        public bool IsConnected { get; private set; }

        /// <summary>When set, Connect() leaves the client disconnected.</summary>
        public bool ConnectFails { get; set; }

        public int ConnectCallCount { get; private set; }

        public int DisconnectCallCount { get; private set; }

        public bool Disposed { get; private set; }

        public void Connect()
        {
            ConnectCallCount++;
            IsConnected = !ConnectFails;
        }

        public void Disconnect()
        {
            DisconnectCallCount++;
            IsConnected = false;
        }

        /// <summary>Queues one telemetry block for the next read, as a dispatch would.</summary>
        public void QueueTelemetry(HaddySimHub.Displays.Msfs.MsfsTelemetry telemetry) => _pending = telemetry;

        public bool TryReadTelemetry(out HaddySimHub.Displays.Msfs.MsfsTelemetry telemetry)
        {
            if (_pending is null)
            {
                telemetry = default;
                return false;
            }

            telemetry = _pending.Value;
            _pending = null;
            return true;
        }

        public void Dispose() => Disposed = true;
    }

    // Mock for IDisplayUpdateSender
    public class MockDisplayUpdateSender : IDisplayUpdateSender
    {
        public DisplayUpdate LastSentUpdate { get; private set; } = null!;
        public Task SendDisplayUpdate(DisplayUpdate displayUpdate)
        {
            LastSentUpdate = displayUpdate;
            return Task.CompletedTask;
        }
    }
}
