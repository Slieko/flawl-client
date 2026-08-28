using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FlawlClient.Flawl.DataModels;

public partial class ChatModel : ObservableObject
{
    [JsonIgnore] public readonly ObservableCollection<MessageModel> Messages = new();

    [JsonIgnore] [ObservableProperty] private IImage? _chatImage;

    [JsonConstructor]
    public ChatModel(long id, string name, HashSet<long> participantsIds)
    {
        Id = id;
        Name = name;
        ParticipantsIds = participantsIds;
    }

    public ChatModel(string name, HashSet<long> participantsIds)
    {
        Name = name;
        ParticipantsIds = participantsIds;
    }

    [JsonPropertyName("id")] public long Id { get; }
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("participantsIds")] public HashSet<long> ParticipantsIds { get; set; }
}