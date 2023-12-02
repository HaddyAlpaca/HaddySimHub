#pragma warning disable 1570

namespace SCSSdkClient.Object {
    public partial class SCSTelemetry {
        /// <summary>
        ///     Job values. Income, destination, source, etc.
        /// </summary>
        public class Job {
            /// <summary>
            ///     In-game time to the moment the job delivery window close
            /// </summary>
            public Time DeliveryTime { get; internal set; } = new();

            /// About: RemainingDeliveryTime
            /// The RemainingDeliveryTime is negative if the delivery is to late
            
            /// <summary>
            ///     Remaining in-game time until the job delivery window close
            /// </summary>
            /// <!----> **INFORMATION** <!---->
            /// Negative if the delivery is to late
            /// <!----> **INFORMATION** <!---->
            public Frequency RemainingDeliveryTime { get; protected internal set; } = new();

            public bool CargoLoaded { get; internal set; }
            public bool SpecialJob { get; internal set; }
            public JobMarket Market { get; internal set; } = new();

            /// <summary>
            ///     Planned job distance in simulated kilometers.
            ///     Does not include distance driven using ferry.
            /// </summary>
            public uint PlannedDistanceKm {get;internal set;}



            /// <summary>
            ///     Cargo values of an job
            /// </summary>
            public Cargo CargoValues { get; internal set; } = new();

            /// <summary>
            ///     Id of the destination city for internal use by code.
            ///     Limited to C-identifier characters and dots.
            /// </summary>
            public string CityDestinationId { get; internal set; } = string.Empty;

            /// <summary>
            ///     Name of the destination city for display purposes.
            ///     Localized using the current in-game language.
            /// </summary>
            public string CityDestination { get; internal set; } = string.Empty;

            /// <summary>
            ///     Id of the destination company for internal use by code.
            ///     Limited to C-identifier characters and dots.
            /// </summary>
            public string CompanyDestinationId { get; internal set; } = string.Empty;

            /// <summary>
            ///     Name of the destination company for display purposes.
            ///     Localized using the current in-game language.
            /// </summary>
            public string CompanyDestination { get; internal set; } = string.Empty;

            /// <summary>
            ///     Id of the source city for internal use by code.
            ///     Limited to C-identifier characters and dots.
            /// </summary>
            public string CitySourceId { get; internal set; } = string.Empty;

            /// <summary>
            ///     Name of the source city for display purposes.
            ///     Localized using the current in-game language.
            /// </summary>
            public string CitySource { get; internal set; } = string.Empty;

            /// <summary>
            ///     Id of the source company for internal use by code.
            ///     Limited to C-identifier characters and dots.
            /// </summary>
            public string CompanySourceId { get; internal set; } = string.Empty;

            /// <summary>
            ///     Name of the source company for display purposes.
            ///     Localized using the current in-game language.
            /// </summary>
            public string CompanySource { get; internal set; } = string.Empty;

            /// <summary>
            ///     Reward in internal game-specific currency.
            /// </summary>
            public ulong Income { get; internal set; }

            /// <summary>
            ///     Cargo Values
            /// </summary>
            public class Cargo {
                /// <summary>
                ///     Mass in kilograms
                /// </summary>
                public float Mass { get; internal set; }

                /// <summary>
                ///     Name for internal use by code.
                ///     Limited to C-identifier characters and dots.
                /// </summary>
                public string Id { get; internal set; } = string.Empty;

                /// <summary>
                ///     Name for display purposes.
                ///     Localized using the current in-game language.
                /// </summary>
                public string Name { get; internal set; } = string.Empty;

                /// <summary>
                ///     How many units of the cargo the job consist of.
                /// </summary>
                public uint UnitCount { get; internal set; }

                /// <summary>
                ///     Mass of the single unit of the cargo in kilograms.
                /// </summary>
                public float UnitMass { get; internal set; }

                public float CargoDamage { get; internal set; }
            }
        }
    }
}