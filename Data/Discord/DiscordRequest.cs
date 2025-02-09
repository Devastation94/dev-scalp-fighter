using Newtonsoft.Json;
using System.Text.Json;

namespace scalp_fighter.Data
{
    public class DiscordRequest
    {
        public DiscordRequest(string content)
        {
            this.content = content;
        }

        [JsonProperty("content")]
        public string content { get; set; }
    }
}
