# Architecture

## Provider-Converter-Display Pattern

```
GameDataProvider → DisplayBase → DataConverter → DisplayUpdateSender
```

| Component | Responsibility |
|-----------|---------------|
| **GameDataProvider** | Acquires raw telemetry from game (UDP/SharedMemory) |
| **DisplayBase** | Manages lifecycle, subscribes to provider events |
| **DataConverter** | Transforms raw telemetry to DisplayUpdate DTO |
| **DisplayUpdateSender** | Sends updates to SignalR hub |

## Display Types

| Type | Use Case | Data Model |
|------|----------|------------|
| `TruckDashboard` | ETS/trucking games | `TruckData` |
| `RaceDashboard` | Circuit racing | `RaceData` |
| `RallyDashboard` | Rally games | `RallyData` |

## File Structure

```
Displays/{Game}/
├── Packet.cs                    # Raw telemetry struct
├── {Game}DataConverter.cs       # Converts Packet → DisplayUpdate
├── {Game}GameDataProvider.cs   # Reads from game
├── {Game}SharedMemoryReader.cs  # (if shared memory)
├── Display.cs                   # Production display
└── TestDisplay.cs              # Test display with mock data
```
