using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using FlawlClient.Flawl.DataModels.Events;

namespace FlawlClient.Flawl.Json;

public class EventJsonConverter : JsonConverter<Event>
{
    public override void Write(Utf8JsonWriter writer, Event value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }

    public override Event? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);

        var root = doc.RootElement;

        switch (root.GetProperty("type").GetString())
        {
            case "nickname-updated":
                return JsonSerializer.Deserialize<NicknameChangedEvent>(root.GetRawText(), options);
            case "avatar-updated":
                return JsonSerializer.Deserialize<MessageSentEvent>(root.GetRawText(), options);
            case "chat-image-updated":
                return JsonSerializer.Deserialize<ChatAvatarChangedEvent>(root.GetRawText(), options);
            case "chat-created":
                return JsonSerializer.Deserialize<ChatCreatedEvent>(root.GetRawText(), options);
            case "message-sent":
                return JsonSerializer.Deserialize<MessageSentEvent>(root.GetRawText(), options);
            default:
                return null;
        }
    }
}