using HtmlAgilityPack;
using scalp_fighter.Data;
using System.Text.RegularExpressions;

namespace scalp_fighter.Clients
{

    public class JJClient
    {
        private static readonly HttpClient client = new();
        private static readonly string jjSearchUrl = "https://shop.jjcards.com/search.asp?keyword={0}+tcg&sortby=2&page=1&catid=";
        private static readonly string jjAddToCartUrl = "https://shop.jjcards.com/add_cart.asp?quick=1&item_id={0}&cat_id=0";
        private static readonly List<string> Keywords = new()
        {
            "Prismatic Evolutions",
            "Obsidian Flames",
            "Surging Sparks",
            "Journey Together",
            "Stellar Crown",
            "Shrouded Fable",
            "Twilight Masquerade",
            "Temportal Forces"
        };

        public async Task<List<Search>> GetProducts()
        {
            Console.WriteLine("JJClient.GetProducts: START");
            var searchList = new List<Search>();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

            try
            {
                foreach (var keyword in Keywords)
                {
                    var searchUrl = string.Format(jjSearchUrl, keyword);
                    string content = await client.GetStringAsync(searchUrl);
                    var doc = new HtmlDocument();
                    doc.LoadHtml(content);

                    var products = doc.DocumentNode.SelectNodes("//div[contains(@class, 'product-content')]");
                    var inStockProducts = new List<Product>();

                    if (products == null || products.Count == 0)
                    {
                        Console.WriteLine($"JJClient.GetProducts: No {keyword} found");
                        continue;
                    }

                    foreach (var product in products)
                    {
                        var nameNode = product.SelectSingleNode(".//a");
                        var priceNode = product.SelectSingleNode(".//span[contains(@class, 'price')]");
                        var availabilityNode = product.SelectSingleNode(".//span[contains(@class, 'availability')]");

                        if (nameNode != null && priceNode != null && availabilityNode != null)
                        {
                            string productName = nameNode.InnerText.Trim();
                            var baseUrl = nameNode.Attributes["href"].Value;
                            var itemId = Regex.Match(baseUrl, @"_(\d+)\.html$");

                            var url = string.Format(jjAddToCartUrl, itemId.Groups[1].Value);
                            string productPrice = priceNode.InnerText.Trim();
                            string availability = availabilityNode.InnerText.Trim();

                            if (availability.Trim().ToUpper() == "IN STOCK.")
                            {
                                inStockProducts.Add(new Product(productName, productPrice, url));
                            }
                        }
                    }

                    if (inStockProducts.Count > 0)
                    {
                        searchList.Add(new Search(keyword, "JJ", inStockProducts));
                    }

                    Console.WriteLine($"JJClient.GetProducts: Found {products.Count} {keyword} products with {inStockProducts.Count} in stock");
                    Thread.Sleep(5000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JJClient.GetProducts: Error fetching webpage: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("JJClient.GetProducts: END");
            }
            return searchList;
        }
    }
}