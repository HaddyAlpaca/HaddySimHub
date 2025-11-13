using SCSSdkClient.Object;
using SCSSdkClient;

namespace HaddySimHub.Displays
{
    public class SCSSdkTelemetryFactory : ISCSTelemetryFactory
    {
        public SCSSdkTelemetry Create()
        {
            return new SCSSdkTelemetry();
        }
    }
}
