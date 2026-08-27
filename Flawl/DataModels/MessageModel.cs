using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FlawlClient.Flawl.DataModels;

public class MessageModel
{
    [JsonPropertyName("messageId")] public long MessageId { get; set; }
    [JsonPropertyName("senderId")] public long SenderId { get; set; }
    [JsonPropertyName("chatId")] public long ChatId { get; set; }
    [JsonPropertyName("content")] public string? Content { get; set; }
    [JsonPropertyName("attachmentsRefs")] public HashSet<string>? Attachments { get; set; }
    [JsonPropertyName("sendTime")] public DateTimeOffset SendTime { get; set; }

    [JsonIgnore] public UserModel? SenderModel { get; set; }
}