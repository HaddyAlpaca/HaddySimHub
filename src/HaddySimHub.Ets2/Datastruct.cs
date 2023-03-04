using System.Runtime.InteropServices;

namespace HaddySimHub.Ets2;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
readonly struct Datastruct
{
    const int GeneralStringSize = 64;

    public readonly uint time;
    public readonly uint paused;

    public readonly uint ets2_telemetry_plugin_revision;
    public readonly uint ets2_version_major;
    public readonly uint ets2_version_minor;

    // ***** REVISION 1 ****** //

    readonly byte padding1;
    public readonly byte trailer_attached;
    public readonly byte padding2;
    public readonly byte padding3;

    public readonly float speed;
    public readonly float accelerationX;
    public readonly float accelerationY;
    public readonly float accelerationZ;

    public readonly float coordinateX;
    public readonly float coordinateY;
    public readonly float coordinateZ;

    public readonly float rotationX;
    public readonly float rotationY;
    public readonly float rotationZ;

    /// <summary>
    /// Current gear
    /// Positive values are forward gears, negative values are reverse gears
    /// </summary>
    public readonly int gear;
    /// <summary>
    /// Number of forward gears
    /// </summary>
    public readonly int gearsForward;
    /// <summary>
    /// Available gear ranges
    /// </summary>
    public readonly int gearRanges;
    /// <summary>
    /// Current gear range
    /// </summary>
    public readonly int gearRangeActive;

    public readonly float engineRpm;
    public readonly float engineRpmMax;

    public readonly float fuel;
    public readonly float fuelCapacity;
    public readonly float fuelRate;
    public readonly float fuelAvgConsumption;

    public readonly float userSteer;
    public readonly float userThrottle;
    public readonly float userBrake;
    public readonly float userClutch;

    public readonly float gameSteer;
    public readonly float gameThrottle;
    public readonly float gameBrake;
    public readonly float gameClutch;

    public readonly float truckWeight;
    public readonly float trailerWeight;

    public readonly int modelOffset;
    public readonly int modelLength;

    public readonly int trailerOffset;
    public readonly int trailerLength;

    public readonly int timeAbsolute;
    public readonly int gearsReverse;

    public readonly float trailerMass;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = GeneralStringSize)]
    public readonly byte[] trailerId;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = GeneralStringSize)]
    public readonly byte[] trailerName;

    public readonly int jobIncome;
    public readonly int jobDeadline;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = GeneralStringSize)]
    public readonly byte[] jobCitySource;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = GeneralStringSize)]
    public readonly byte[] jobCityDestination;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = GeneralStringSize)]
    public readonly byte[] jobCompanySource;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = GeneralStringSize)]
    public readonly byte[] jobCompanyDestination;

    // ***** REVISION 3 ****** //

    public readonly int retarderBrake;
    public readonly int shifterSlot;
    public readonly int shifterToggle;
    public readonly int padding4;

    public readonly byte cruiseControl;
    public readonly byte wipers;

    public readonly byte parkBrake;
    public readonly byte motorBrake;

    public readonly byte electricEnabled;
    public readonly byte engineEnabled;

    public readonly byte blinkerLeftActive;
    public readonly byte blinkerRightActive;
    public readonly byte blinkerLeftOn;
    public readonly byte blinkerRightOn;

    public readonly byte lightsParking;
    public readonly byte lightsBeamLow;
    public readonly byte lightsBeamHigh;
    public readonly uint lightsAuxFront;
    public readonly uint lightsAuxRoof;
    public readonly byte lightsBeacon;
    public readonly byte lightsBrake;
    public readonly byte lightsReverse;

    public readonly byte batteryVoltageWarning;
    public readonly byte airPressureWarning;
    public readonly byte airPressureEmergency;
    public readonly byte adblueWarning;
    public readonly byte oilPressureWarning;
    public readonly byte waterTemperatureWarning;

    public readonly float airPressure;
    public readonly float brakeTemperature;
    public readonly int fuelWarning;
    public readonly float adblue;
    public readonly float adblueConsumption;
    public readonly float oilPressure;
    public readonly float oilTemperature;
    public readonly float waterTemperature;
    public readonly float batteryVoltage;
    public readonly float lightsDashboard;
    public readonly float wearEngine;
    public readonly float wearTransmission;
    public readonly float wearCabin;
    public readonly float wearChassis;
    public readonly float wearWheels;
    public readonly float wearTrailer;
    public readonly float truckOdometer;
    public readonly float cruiseControlSpeed;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = GeneralStringSize)]
    public readonly byte[] truckMake;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = GeneralStringSize)]
    public readonly byte[] truckMakeId;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = GeneralStringSize)]
    public readonly byte[] truckModel;

    // ***** REVISION 4 ****** //

    public readonly float fuelWarningFactor;
    public readonly float adblueCapacity;
    public readonly float airPressureWarningValue;
    public readonly float airPressureEmergencyValue;
    public readonly float oilPressureWarningValue;
    public readonly float waterTemperatureWarningValue;
    public readonly float batteryVoltageWarningValue;

    public readonly uint retarderStepCount;

    public readonly float cabinPositionX;
    public readonly float cabinPositionY;
    public readonly float cabinPositionZ;
    public readonly float headPositionX;
    public readonly float headPositionY;
    public readonly float headPositionZ;
    public readonly float hookPositionX;
    public readonly float hookPositionY;
    public readonly float hookPositionZ;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public readonly byte[] shifterType;

    public readonly float localScale;
    public readonly int nextRestStop;
    public readonly float trailerCoordinateX;
    public readonly float trailerCoordinateY;
    public readonly float trailerCoordinateZ;
    public readonly float trailerRotationX;
    public readonly float trailerRotationY;
    public readonly float trailerRotationZ;

    public readonly int displayedGear;
    public readonly float navigationDistance;
    public readonly float navigationTime;
    public readonly float navigationSpeedLimit;

    /*
    const int MaxSlotCount = 32; // TODO: need to fix.
    const int MaxWheelCount = 20;
    public uint wheelCount; 
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxWheelCount)]
    public float[] wheelPositionX;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxWheelCount)]
    public float[] wheelPositionY;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxWheelCount)]
    public float[] wheelPositionZ;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxWheelCount)]
    public byte[] wheelSteerable;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxWheelCount)]
    public byte[] wheelSimulated;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxWheelCount)]
    public float[] wheelRadius;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxWheelCount)]
    public byte[] wheelPowered;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxWheelCount)]
    public byte[] wheelLiftable;        
    public uint selectorCount;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxSlotCount)]
		public int[] slotGear;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxSlotCount)]
		public uint[] slotHandlePosition;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxSlotCount)]
		public uint[] slotSelectors;         
    */
}