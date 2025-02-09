using HtmlAgilityPack;
using scalp_fighter.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace scalp_fighter.Clients
{
    public class _401GamesClient
    {
        private static readonly HttpClient client = new();
        private static readonly string _401SearchUrl = "https://store.401games.ca/collections/pokemon-trading-cards?sort=price_max_to_min&filters=Product+Type,Product+Type_Booster+Boxes,Price_from_to,66-400,In+Stock,True";
        private static readonly string jjAddToCartUrl = "https://shop.jjcards.com/add_cart.asp?quick=1&item_id={0}&cat_id=0";
        private static readonly List<string> Keywords = new List<string>() { "Pokemon TCG" };

        public async Task<List<Search>> GetPokemon()
        {
            var searchList = new List<Search>();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

            try
            {
                foreach (var keyword in Keywords)
                {
                    string content = await client.GetStringAsync(_401SearchUrl);
                    var doc = new HtmlDocument();
                    doc.LoadHtml(content);

                    var jsonMatch = Regex.Match(content, @"\{""name"":.*?\}");

                    var products = doc.DocumentNode.SelectNodes("//div[contains(@class, 'product-container')]");
                    var inStockProducts = new List<Product>();

                    if (products == null || products.Count == 0)
                    {
                        Console.WriteLine($"No {keyword} found");
                        continue;
                    }

                    foreach (var product in products)
                    {
                        var nameNode = doc.DocumentNode.SelectSingleNode("//span[@class='product-title']");
                        var priceNode = doc.DocumentNode.SelectSingleNode("//div[@class='fs-price']");
                        var availabilityNode = doc.DocumentNode.SelectSingleNode("//span[@class='in-stock']");

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
                                inStockProducts.Add(new Product(productName, productPrice, true, url));
                            }
                        }
                    }
                    searchList.Add(new Search(keyword, inStockProducts));
                    Console.WriteLine($"Found {products.Count} {keyword} products with {inStockProducts.Count} in stock");
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
