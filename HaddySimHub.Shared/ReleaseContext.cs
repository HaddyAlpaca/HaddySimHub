using System.Text.Json.Serialization;

namespace HaddySimHub.Shared;

[JsonSerializable(typeof(Release))]
[JsonSerializable(typeof(Release.Asset))]
public partial class ReleaseContext : JsonSerializerContext
{
}
