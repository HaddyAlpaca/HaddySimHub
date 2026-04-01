# C# Displays Skill

This skill creates game telemetry displays following the provider-converter-display architecture.

## Architecture

```
Game (Shared Memory / UDP)
         |
         v
GameDataProvider<TTelemetry>
         |
         v
DataConverter<TTelemetry, DisplayUpdate>
         |
         v
DisplayUpdateSender
         |
         v
SignalR Hub → Clients
```

## Quick Start

1. Create telemetry struct matching game data format
2. Create DataConverter to transform raw data to DTO
3. Create GameDataProvider (SharedMemory or UDP)
4. Create Display extending `DisplayBase<T>`
5. Register in `DisplayFactory`

## Reference Files

| Topic | File |
|-------|------|
| Architecture & Interfaces | [references/architecture.md](references/architecture.md) |
| Core Interfaces | [references/interfaces.md](references/interfaces.md) |
| Data Models | [references/models.md](references/models.md) |
| UDP Provider Pattern | [references/pattern-udp-provider.md](references/pattern-udp-provider.md) |
| Shared Memory Provider Pattern | [references/pattern-sharedmemory-provider.md](references/pattern-sharedmemory-provider.md) |
| Display Implementation | [references/pattern-display.md](references/pattern-display.md) |
| Test Display Pattern | [references/pattern-test-display.md](references/pattern-test-display.md) |
| Registration | [references/registration.md](references/registration.md)
