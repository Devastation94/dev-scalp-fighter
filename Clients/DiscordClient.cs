using System.Text;
using System.Text.Json;

namespace scalp_fighter.Data
{
    public class DiscordClient
    {
        public async Task PostWebHook(string message)
        {
            using var client = new HttpClient();
            var discordBody = JsonSerializer.Serialize(new DiscordRequest(message));
            var response = await client.PostAsync(Constants.BOT_HOOK_URL, new StringContent(discordBody, Encoding.UTF8, "application/json"));
            var responseContent = response.Content.ReadAsStringAsync();

            return;
        }
    }
}