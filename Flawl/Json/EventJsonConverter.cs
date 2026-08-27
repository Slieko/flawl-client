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

        if (root.GetProperty("type").GetString() == "message-sent")
            return JsonSerializer.Deserialize<MessageSentEvent>(root.GetRawText(), options);

        return JsonSerializer.Deserialize<Event>(root.GetRawText(), options);
    }
}