using System.Text.Json.Serialization;

namespace FlawlClient.Flawl.DataModels.Events;

public class MessageSentEvent : Event
{
    [JsonPropertyName("chatId")] public long ChatId { get; set; }

    [JsonPropertyName("messageId")] public long MessageId { get; set; }
}