using Newtonsoft.Json;

namespace Dictobot.Configuration
{
    public sealed class APIStructure : JSONReader<APIStructure>
    {
        [JsonProperty("token")]
        public string Token { get; set; } = string.Empty;
        public static async Task InitializeAsync()
        {
            await Read("api-config.json");
        }
    }
}
