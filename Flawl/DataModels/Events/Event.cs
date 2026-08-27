using System.Text.Json.Serialization;
using FlawlClient.Flawl.Json;

namespace FlawlClient.Flawl.DataModels.Events;

[JsonConverter(typeof(EventJsonConverter))]
public abstract class Event
{
    [JsonPropertyName("issuerId")] public long IssuerId { get; set; }
}