using System.Runtime.Serialization;

namespace iRacingSDK;

[Serializable]
public partial class Telemetry : Dictionary<string, object>
{
	public Telemetry()
	{
	}

#pragma warning disable SYSLIB0051 // Type or member is obsolete
public Telemetry(SerializationInfo info, StreamingContext context) : base(info, context) {

	}
#pragma warning restore SYSLIB0051 // Type or member is obsolete
}

[Serializable]
public partial class SessionData
{
	[Serializable]
	public partial class _WeekendInfo
	{
		[Serializable]
		public partial class _WeekendOptions
		{
		}
				
		[Serializable]
		public partial class _TelemetryOptions
		{
		}
	}
			
	[Serializable]
	public partial class _SessionInfo
	{
		[Serializable]
		public partial class _Sessions
		{
			[Serializable]
			public partial class _ResultsPositions
			{
			}

			[Serializable]
			public partial class _ResultsFastestLap
			{
			}
		}
	}

	[Serializable]
	public partial class _CameraInfo
	{
		[Serializable]
		public partial class _Groups
		{
			[Serializable]
			public partial class _Cameras
			{
			}
		}
	}

	[Serializable]
	public partial class _RadioInfo
	{
		[Serializable]
		public partial class _Radios
		{
		}
	}

	[Serializable]
	public partial class _DriverInfo
	{
		[Serializable]
		public partial class _Drivers
		{
		}
	}

	[Serializable]
	public partial class _SplitTimeInfo
	{
		[Serializable]
		public partial class _Sectors
		{
		}
	}
}
