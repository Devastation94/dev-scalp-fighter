using HtmlAgilityPack;
using scalp_fighter.Data;
using System.Text.RegularExpressions;
using System.Web;

namespace scalp_fighter.Clients {

    public class ChimeraClient {
        private static readonly HttpClient client = new ();
        private static readonly string ChimeraSearchUrl = "https://shop.jjcards.com/search.asp?keyword={0}+tcg&sortby=2&page=1&catid=";
        private static readonly string jjAddToCartUrl = "https://shop.jjcards.com/add_cart.asp?quick=1&item_id={0}&cat_id=0";
        private static readonly List<string> Keywords = new List<string> () { "Pokemon TCG" };

        public async Task<List<Search>> GetPokemon () {
            var searchList = new List<Search>();

            try {
                foreach (var keyword in Keywords)
                {
                    string content = await client.GetStringAsync(string.Format(ChimeraSearchUrl, keyword));
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
                        var nameNode = doc.DocumentNode.SelectSingleNode("//div[@class='h4 grid-view-item__title']");
                        var priceNode = doc.DocumentNode.SelectSingleNode("//span[contains(@class, 'product-price__price') and contains(@class, 'is-bold') and contains(@class, 'qv-regularprice')]");
                        var availabilityNode = doc.DocumentNode.SelectSingleNode("//span[@id='tag-container' and @class='outstock-overlay']");

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
            } catch (Exception ex) {
                Console.WriteLine ($"Error fetching webpage: {ex.Message}");
            }
            return searchList;
        }
    }
}