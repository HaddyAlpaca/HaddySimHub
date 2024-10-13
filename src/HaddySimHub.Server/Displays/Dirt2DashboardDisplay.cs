using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using HaddySimHub.Server.Models;

namespace HaddySimHub.Server.Displays;

internal sealed class Dirt2DashboardDisplay : DisplayBase
{
    private const int PORT = 20777;
    private readonly UdpClient _client = new(PORT);
    private IPEndPoint _endPoint = new(IPAddress.Any, PORT);

    public override string Description => "Dirt Rally 2";
    public override bool IsActive => Functions.IsProcessRunning("dirtrally2");
    public override void Start() => _client.BeginReceive(new AsyncCallback(ReceiveCallback), null);
    public override void Stop() => _client.Close();

    public Dirt2DashboardDisplay(Func<object, Func<object, DisplayUpdate>, Task> receivedDataCallBack) : base(receivedDataCallBack)
    {
    }

    private void ReceiveCallback(IAsyncResult result)
    {
        // Get data we received.
        byte[] data = _client.EndReceive(result, ref this._endPoint!);

        // Start receiving again.
        _client.BeginReceive(new AsyncCallback(ReceiveCallback), null);

        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);

        try
        {
            // Get the header to retrieve the packet ID.
#pragma warning disable CS8605 // Unboxing a possibly null value.
            var packet = (Packet)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Packet));
#pragma warning restore CS8605 // Unboxing a possibly null value.
            this._receivedDataCallBack(packet, GetDisplayUpdate);
        }
        finally
        {
            handle.Free();
        }
    }

    private static DisplayUpdate GetDisplayUpdate(object inputData)
    {
        var typedData = (Packet)inputData;

        var data = new RallyData
        {
            Speed = Convert.ToInt32(typedData.speed_ms * 3.6),
            Rpm = Convert.ToInt32(typedData.rpm * 10),
            MaxRpm = Convert.ToInt32(typedData.max_rpm),
            Gear = Convert.ToInt32(typedData.gear),
            Clutch = Convert.ToInt32(typedData.clutch * 100),
            Brake = Convert.ToInt32(typedData.brakes * 100),
            Throttle = Convert.ToInt32(typedData.throttle * 100),
            CompletedPct = Math.Min(Convert.ToInt32(typedData.progress * 100), 100),
            DistanceTravelled = Math.Max(Convert.ToInt32(typedData.distance), 0),
            Position = Convert.ToInt32(typedData.car_pos),
            Sector1Time = typedData.sector_1_time,
            Sector2Time = typedData.sector_2_time,
            LapTime = typedData.lap_time,
        };

        return new DisplayUpdate { Type = DisplayType.RallyDashboard, Data = data };
    }

#pragma warning disable CS0649
    public struct Packet
    {
        public float run_time;
        public float lap_time;
        public float distance;
        public float progress; // 0-1
        public float pos_x;
        public float pos_y;
        public float pos_z;
        public float speed_ms; // * 3.6 for Km/h
        public float vel_x;  // velocity in world space
        public float vel_y;  // velocity in world space
        public float vel_z;  // velocity in world space
        public float roll_x;
        public float roll_y;
        public float roll_z;
        public float pitch_x;
        public float pitch_y;
        public float pitch_z;
        public float susp_rl;  // Suspension travel aft left
        public float susp_rr; // Suspension travel aft right
        public float susp_fl;  // Suspension travel fwd left
        public float susp_fr;  // Suspension travel fwd right
        public float susp_vel_rl;
        public float susp_vel_rr;
        public float susp_vel_fl;
        public float susp_vel_fr;
        public float wsp_rl;  // Wheel speed aft left
        public float wsp_rr;  // Wheel speed aft right
        public float wsp_fl;  // Wheel speed fwd left
        public float wsp_fr;  // Wheel speed fwd right
        public float throttle;  // Throttle 0-1
        public float steering;  // -1..+1
        public float brakes;  // Brakes 0-1
        public float clutch;  // Clutch 0-1
        public float gear;  // Gear, neutral = 0
        public float g_force_lat;
        public float g_force_lon;
        public float current_lap;  // Current lap, starts at 0
        public float rpm;  // RPM, requires * 10 for realistic values
        public float sli_pro_support;  // ignored
        public float car_pos;
        public float kers_level;  // ignored
        public float kers_max_level;  // ignored
        public float drs;  // ignored
        public float traction_control;  // ignored
        public float anti_lock_brakes;  // ignored
        public float fuel_in_tank;  // ignored
        public float fuel_capacity;  // ignored
        public float in_pit;  // ignored
        public float sector;
        public float sector_1_time;
        public float sector_2_time;
        public float brakes_temp_rl;
        public float brakes_temp_rr;
        public float brakes_temp_fl;
        public float brakes_temp_fr;
        public float tyre_pressure_rl;
        public float tyre_pressure_rr;
        public float tyre_pressure_fl;
        public float tyre_pressure_fr;
        public float laps_completed;
        public float total_laps;
        public float track_length;
        public float last_lap_time;
        public float max_rpm;  // requires * 10 for realistic values
        public float idle_rpm;  // requires * 10 for realistic values
        public float max_gears;
    }
#pragma warning restore CS0649
}
