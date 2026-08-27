using System.Text.Json.Serialization;
using Avalonia.Media;

namespace FlawlClient.Flawl.DataModels;

public class UserModel
{
    public UserModel(long userId, string username, string nickname)
    {
        UserId = userId;
        Username = username;
        Nickname = nickname;
    }

    [JsonPropertyName("userId")] public long UserId { get; set; }
    [JsonPropertyName("username")] public string Username { get; set; }
    [JsonPropertyName("nickname")] public string Nickname { get; set; }

    [JsonIgnore] public required IImage Avatar { get; set; }
}