namespace iRacingSDK;

/// <summary>
/// Remote controll the sim by sending these windows messages
/// camera and replay commands only work when you are out of your car, 
/// pit commands only work when in your car
/// </summary>
public enum BroadcastMessage
{
    /// <summary>
    /// The camera switch position : car position, group, camera
    /// </summary>
    CameraSwitchPos = 0,

    /// <summary>
    /// The camera switch number : driver #, group, camera
    /// </summary>
    CameraSwitchNum,

    /// <summary>
    /// The state of the camera set: CameraState, unused, unused
    /// </summary>
    CameraSetState,

    /// <summary>
    /// The replay set play speed: speed, slowMotion, unused
    /// </summary>
    ReplaySetPlaySpeed,

    /// <summary>
    /// The replay set play position: ReplayPositionMode, Frame Number (high, low)
    /// </summary>
    ReplaySetPlayPosition,

    /// <summary>
    /// The replay search : ReplaySearchMode, unused, unused
    /// </summary>
    ReplaySearch,

    /// <summary>
    /// The state of the replay set : ReplayStateMode, unused, unused
    /// </summary>
    ReplaySetState,

    /// <summary>
    /// The reload textures : ReloadTexturesMode, carIdx, unused
    /// </summary>
    ReloadTextures,

    /// <summary>
    /// The chat comand : ChatCommandMode, subCommand, unused
    /// </summary>
    ChatComand,

    /// <summary>
    /// The pit command : PitCommandMode, parameter
    /// this only works when the driver is in the car
    /// </summary>
    PitCommand,

    /// <summary>
    /// The Telemetry Command : TelemCommandMode, ...
    /// You can call this any time, but telemtry only records when driver is in there car
    /// </summary>
    BroadcastTelemetryCommand,

    /// <summary>
    /// value (float, high, low)
    /// </summary>
    BroadcastFFBCommand,
    
    /// <summary>
    /// sessionTimeMS (high, low)
    /// </summary>
    BroadcastReplaySearchSessionTime
};
