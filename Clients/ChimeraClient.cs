using HtmlAgilityPack;
using scalp_fighter.Data;
using System.Text.RegularExpressions;

namespace scalp_fighter.Clients
{

    public class ChimeraClient
    {
        private static readonly HttpClient client = new();
        private static readonly string ChimeraSearchUrl = "https://chimeragamingonline.com/search?options%5Bprefix%5D=last&type=product&q={0}";
        private static readonly string ChimeraBaseUrl = "https://chimeragamingonline.com";
        private static readonly List<string> Keywords = new()
        {
            "Prismatic Evolutions Booster",
            "Obsidian Flames Booster",
            "Surging Sparks Booster",
            "Journey Together Booster",
            "Stellar Crown Booster",
            "Shrouded Fable Booster",
            "Twilight Masquerade Booster",
            "Temportal Forces Booster"
        };

        public async Task<List<Search>> GetPokemon()
        {
            var searchList = new List<Search>();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

            try
            {
                foreach (var keyword in Keywords)
                {
                    var content = await client.GetStringAsync(string.Format(ChimeraSearchUrl, keyword));
                    var doc = new HtmlDocument();
                    doc.LoadHtml(content);

                    var products = doc.DocumentNode.SelectNodes("//div[contains(@class, 'grid-view-item__link')]");
                    var inStockProducts = new List<Product>();

                    if (products == null || products.Count == 0)
                    {
                        Console.WriteLine($"No {keyword} found");
                        continue;
                    }

                    foreach (var product in products)
                    {
                        var name = product.SelectSingleNode(".//div[@class='h4 grid-view-item__title']").InnerText.Trim();
                        var price = product.SelectSingleNode(".//span[contains(@class, 'product-price__price') and contains(@class, 'is-bold') and contains(@class, 'qv-regularprice')]").InnerText.Trim();
                        var url = ChimeraBaseUrl + product.SelectSingleNode(".//a[contains(@href, '/products/')]").GetAttributeValue("href", "");
                        var availability = product.SelectSingleNode(".//span[@class='value']").InnerText.Trim().ToUpper().Contains("SOLD OUT");

                        if (name.Contains("pokemon") && double.Parse(price) > 20)
                        {
                            inStockProducts.Add(new Product(name, price, url));
                        }
                    }

                    if (inStockProducts.Count > 0)
                    {
                        searchList.Add(new Search(keyword, "Chimera", inStockProducts));
                    }

                    Console.WriteLine($"Found {products.Count} {keyword} products with {inStockProducts.Count} in stock");
                    Thread.Sleep(5000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching webpage: {ex.Message}");
            }
            return searchList;
        }
    }
}