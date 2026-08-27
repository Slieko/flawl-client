using System.Text.Json.Serialization;

namespace FlawlClient.Flawl.DataModels;

public class LoginCredentialsModel
{
    public LoginCredentialsModel(string username, string password)
    {
        Username = username;
        Password = password;
    }

    [JsonPropertyName("username")] public string Username { get; set; }
    [JsonPropertyName("password")] public string Password { get; set; }
}