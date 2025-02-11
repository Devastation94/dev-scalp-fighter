using System.Text;
using System.Text.Json;

namespace scalp_fighter.Data
{
    public class DiscordClient
    {
        public async Task PostWebHook(List<Search> searchResults)
        {
            var webHookValue = string.Empty;

            foreach (var storeGroup in searchResults.GroupBy(sr => sr.Store))
            {
                webHookValue += $"- {storeGroup.Key}\n";

                foreach (var itemInStock in storeGroup)
                {
                    webHookValue += $"  - {itemInStock.Keyword}\n";

                    foreach (var product in itemInStock.Products)
                    {
                        var productInfo = $"New Item Now In Stock: {product.Name}, Price: {product.Price}";
                        Console.WriteLine($"Program.PostResults: {productInfo}");
                        webHookValue += $"      - {product.Url}";
                    }
                }

                using var client = new HttpClient();
                var discordBody = JsonSerializer.Serialize(new DiscordRequest(webHookValue));
                var response = await client.PostAsync(Constants.BOT_HOOK_URL, new StringContent(discordBody, Encoding.UTF8, "application/json"));
                var responseContent = response.Content.ReadAsStringAsync();
            }
            return;
        }
    }
}