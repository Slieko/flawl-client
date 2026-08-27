using System.Text.Json.Serialization;

namespace FlawlClient.Flawl.DataModels;

public class RefreshTokenModel
{
    public RefreshTokenModel(string refreshToken)
    {
        RefreshToken = refreshToken;
    }

    [JsonPropertyName("refreshToken")] public string RefreshToken { get; set; }
}