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
    public class CanadaComputersClient
    {
        private static readonly HttpClient client = new();
        private static readonly string ccSearchUrl = "https://www.canadacomputers.com/{0}";
        private static readonly string jjAddToCartUrl = "https://shop.jjcards.com/add_cart.asp?quick=1&item_id={0}&cat_id=0";
        private static readonly List<string> Keywords = new List<string>() { "en/powered-by-nvidia/268144/gigabyte-aorus-geforce-rtx-5080-master-16g-gddr7-gv-n5080aorus-m-16gd.html" };

        public async Task<List<Search>> GetPokemon()
        {
            var searchList = new List<Search>();

            try
            {
                foreach (var keyword in Keywords)
                {
                    var searchUrl = string.Format(ccSearchUrl, keyword);
                    string content = await client.GetStringAsync(searchUrl);
                    var doc = new HtmlDocument();
                    doc.LoadHtml(content);
                    var soldOutNode = doc.DocumentNode.SelectSingleNode("//p[@class='mt-1 text-dark f-16 fm-xs-SF-Pro-Display-Medium']").InnerText;

                    if (soldOutNode.Trim().ToUpper().Contains("AVAILABLE"))
                    {
                        var inStockProducts = new List<Product>();
                        inStockProducts.Add(new Product("RTX 5080", "$1899", true, searchUrl));

                        searchList.Add(new Search("Gigabyte RTX 5080", inStockProducts));
                    }                
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