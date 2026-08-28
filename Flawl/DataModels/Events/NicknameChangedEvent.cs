using System.Text.Json.Serialization;

namespace FlawlClient.Flawl.DataModels.Events;

public class NicknameChangedEvent : Event
{
    [JsonPropertyName("nickname")] public required string Nickname { get; set; }
}