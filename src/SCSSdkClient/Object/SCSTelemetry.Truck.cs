namespace SCSSdkClient.Object {
    public partial class SCSTelemetry {
        /// <summary>
        ///     Truck telemetry specific values
        /// </summary>
        public partial class Truck {
            /// <summary>
            ///     Config Values, doesn't change most of the time
            /// </summary>
            public Constants ConstantsValues { get; internal set; } = new();

            /// <summary>
            ///     Truck channel values, change a lot
            /// </summary>
            public Current CurrentValues { get; internal set; } = new();

            /// <summary>
            ///     Contains position data of the cabin, head and hook
            /// </summary>
            public PositionData Positioning { get; internal set; } = new();
        }
    }
}