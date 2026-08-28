using System.Text.Json.Serialization;

namespace FlawlClient.Flawl.DataModels.Events;

public class ChatCreatedEvent : Event
{
    [JsonPropertyName("chatId")] public long ChatId { get; set; }
}