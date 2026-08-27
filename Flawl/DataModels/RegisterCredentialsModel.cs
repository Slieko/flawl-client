using System.Text.Json.Serialization;

namespace FlawlClient.Flawl.DataModels;

public class RegisterCredentialsModel
{
    public RegisterCredentialsModel(string username, string nickname, string password)
    {
        Username = username;
        Nickname = nickname;
        Password = password;
    }

    [JsonPropertyName("username")] public string Username { get; set; }
    [JsonPropertyName("nickname")] public string Nickname { get; set; }
    [JsonPropertyName("password")] public string Password { get; set; }
}