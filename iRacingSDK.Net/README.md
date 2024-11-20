# iRacingSDK.Net - Alternative
-------------------------------

[![Build status](https://dev.azure.com/MerlinCooper/iRacingSDK_Nuget/_apis/build/status/iRacingSDK_Nuget-.NET%20Desktop-CI?branchName=master)

This dot.net library allows access to the raw data stream from the iRacing Motorsport Racing simulation (www.iracing.com).  It also provides an interface to control certain aspects of the game.  It is based on the official C++ SDK.

It provides access to the game's data interface using conventional dot.net paradigms.

At this point in time, it is heavily focused on supporting the data stream provided by the game during a replay session.  But it should also be useful for live non-replay mode.

There are other dot.net implementations of the iRacing SDK library - see the iRacing Developer's forum for references and downloads for those.

## Accessing game data

You can use the enumeration access of the iRacing class to access a stream of data samples, generated by the game.  For example, to get a single sample you can use the following:

```
var iracing = new iRacingConnection();
var data = iracing.GetDataFeed().First();
```
 
This will return a single `DataSample` value.  

Alternatively you can get a infinite sample set, or a set of samples until some condition is met.

```
foreach( var data in iracing
             .GetDataFeed()
             .TakeWhile( d => d.Telemetry.SessionState != SessionState.CoolDown ))
{
    Console.WriteLine(data.Telemetry.RaceLaps);

    Thread.Sleep(1000);
}

Console.WriteLine("Finished.");

```
For more examples of accessing this data - checkout the Sample project within this repo.

## Access game data via Events

You can access the DataSample in an event handler.  Add your event handler to the NewData event of the iRacing type.

```
public void Setup()
{
    ...
    iracing.NewData += iRacing_NewData;
    iracing.StartListening();
}

void iRacing_NewData(DataSample data)
{
    var tractionControl = data.Telemetry.dcTractionControl;

    ...
}

```
NB:  You can not access the GetDataFeed() enumerator inside an Event Handler - You can not use the two types of accessors to DataSamples - events or enumerators - but not both.

## Sending Messages to the game

You can use the Replay object to control various aspects of the game.  For example, to change the camera to a specific car:

```
var data = iracing.GetDataFeed().First();
var camera = data.SessionData.CameraInfo.Groups.First(g => g.GroupName == "TV3");

var carIdx = 3;
var number = data.SessionData.DriverInfo.Drivers[carIdx].CarNumberRaw;
iRacing.Replay.CameraOnDriver((short)number, (short)camera.GroupNum, 0);
```

## DataSample

This type is the container for all data retrieved from iRacing.  [More details here](Docs/DataSample.md)

## Data Feed Filters

There are a set of extension methods to the `IEnumerable<DataSample>` set to enhance and refine some of the game's data stream.
[More details here](Docs/DataFeedFilters.md)