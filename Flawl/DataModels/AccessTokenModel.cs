using System.Text.Json.Serialization;

namespace FlawlClient.Flawl.DataModels;

public class AccessTokenModel
{
    public AccessTokenModel(string accessToken)
    {
        AccessToken = accessToken;
    }

    [JsonPropertyName("accessToken")] public string AccessToken { get; set; }
}