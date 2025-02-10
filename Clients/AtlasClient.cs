using HtmlAgilityPack;
using scalp_fighter.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scalp_fighter.Clients
{
    public class AtlasClient
    {
        private static readonly HttpClient client = new();
        private static readonly string AtlasSearchUrl = "https://www.atlascollectables.com/catalog/pokemon-pokemon_sealed_products-pokemon_booster_boxes/386?filter_by_stock=in-stock";
        private static readonly string AtlasBaseUrl = "https://www.atlascollectables.com";
        private static readonly List<string> Keywords = new()
        {
            "Pokemon Booster Boxes",
        };

        public async Task<List<Search>> GetPokemon()
        {
            Console.WriteLine("AtlasClient.GetProducts: START");
            var searchList = new List<Search>();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

            try
            {
                foreach (var keyword in Keywords)
                {
                    var content = await client.GetStringAsync(AtlasSearchUrl);
                    var doc = new HtmlDocument();
                    doc.LoadHtml(content);

                    var products = doc.DocumentNode.SelectNodes("//li[contains(@class, 'product')]");
                    var inStockProducts = new List<Product>();

                    if (products == null || products.Count == 0)
                    {
                        Console.WriteLine($"No {keyword} found");
                        continue;
                    }

                    foreach (var product in products)
                    {
                        var name = product.SelectSingleNode(".//h4[contains(@class, 'name')]").InnerText.Trim();
                        var price = product.SelectSingleNode(".//div[contains(@class, 'product-price-qty')]//span[contains(@class, 'price')]").InnerText.Trim();
                        var url = AtlasBaseUrl + product.SelectSingleNode(".//a[@itemprop='url']").GetAttributeValue("href", "");

                        inStockProducts.Add(new Product(name, price[4..], url));
                    }

                    if (inStockProducts.Count > 0)
                    {
                        searchList.Add(new Search(keyword, "Atlas", inStockProducts));
                    }

                    Console.WriteLine($"AtlasClient.GetProducts: Found {products.Count} {keyword} products with {inStockProducts.Count} in stock");
                    Thread.Sleep(5000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AtlasClient.GetProducts: Error fetching webpage: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("AtlasClient.GetProducts: END");
            }
            return searchList;
        }
    }
}
