using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using Avalonia.Media;

namespace FlawlClient.Flawl.DataModels;

public class ChatModel
{
    [JsonIgnore] public readonly ObservableCollection<MessageModel> Messages = new();

    public ChatModel(long id, string name, HashSet<long> participantsIds)
    {
        Id = id;
        Name = name;
        ParticipantsIds = participantsIds;
    }

    [JsonIgnore] public IImage? ChatImage { get; set; }

    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("participantsIds")] public HashSet<long> ParticipantsIds { get; set; }
}