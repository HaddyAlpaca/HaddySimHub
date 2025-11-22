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
