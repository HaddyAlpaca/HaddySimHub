namespace HaddySimHub.DirtRally2;

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