namespace iRacingSDK;

public enum PitCommandMode
{
    /// <summary>
    /// Clear all pit checkboxes
    /// </summary>
    Clear = 0,
    /// <summary>
    /// Clean the winshield, using one tear off
    /// </summary>
    Windshield,
    /// <summary>
    /// Add fuel, optionally specify the amount to add in liters or pass '0' to use existing amount
    /// </summary>
    Fuel,
    /// <summary>
    /// Change the left front tire, optionally specifying the pressure in KPa or pass '0' to use existing pressure
    /// </summary>
    LeftFront,
    /// <summary>
    /// Change the right front tire, optionally specifying the pressure in KPa or pass '0' to use existing pressure
    /// </summary>
    RightFront,
    /// <summary>
    /// Change the left rear tire, optionally specifying the pressure in KPa or pass '0' to use existing pressure
    /// </summary>
    LeftRear,
    /// <summary>
    /// Change the right rear tire, optionally specifying the pressure in KPa or pass '0' to use existing pressure
    /// </summary>
    RightRear,
    /// <summary>
    /// Clear tire pit checkboxes tire, optionally specifying the pressure in KPa or pass '0' to use existing pressure
    /// </summary>
    ClearTires,
    /// <summary>
    /// Request fast repair
    /// </summary>
    FastRepair
};
