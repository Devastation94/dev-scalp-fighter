using scalp_fighter.Clients;
using scalp_fighter.Data;
using System.Text.Json;
using Timer = System.Timers.Timer;

class WebpageMonitor {

    private static Timer timer;
    private static List<Search> OldProductsInStock = new();
    private static List<Search> ProductsInStock = new();
    private static JJClient jJClient = new ();
    private static DiscordClient discordClient = new();

    static async Task Main () {
        Console.WriteLine ("Starting webpage monitor...");
        OldProductsInStock = JsonSerializer.Deserialize<List<Search>>(File.ReadAllText("Cache.json"));
        timer = new Timer (60000); // Check every 60 seconds
        timer.Elapsed += async (sender, e) => await HandleElapsedEvent();
       
        timer.Start ();

        await ScanJJ (); // Initial check
        Console.ReadLine (); // Keep the program running
    }

   static async Task HandleElapsedEvent()
    {
        await ScanJJ();
        await ScanPokemonCenter();
        await ScanChimera();
    }

    private static async Task ScanPokemonCenter()
    {

    }
    
    private static async Task ScanChimera()
    {

    }

    private static async Task ScanJJ () {
        try {
            var jjResults = await jJClient.GetPokemon();
            ProductsInStock = jjResults;
            var productsInStockJson = JsonSerializer.Serialize(ProductsInStock);
            var oldProductsInStockJson = JsonSerializer.Serialize(OldProductsInStock);

            if (productsInStockJson != oldProductsInStockJson) {
                foreach (var set in jjResults)
                {
                    var newItemsInStock = GetNewItemsInStock();
                    var webhookValue = "@everyone\n";

                    if (newItemsInStock.Count > 0)
                    {
                        foreach (var itemInStock in newItemsInStock)
                        {
                            foreach (var product in itemInStock.Products)
                            {
                                var productInfo = $"New Pokemon Item Listed: {product.Name}, Price: {product.Price}, Status: {product.Status}\n";
                                Console.WriteLine(productInfo);
                                webhookValue += product.Url;
                            }
                        }

                        await discordClient.PostWebHook(webhookValue);
                        
                    }
                }
                File.WriteAllText("Cache.json", productsInStockJson);
                OldProductsInStock = ProductsInStock;
            }
        } catch (Exception ex) {
            Console.WriteLine ($"Error fetching webpage: {ex.Message}");
        }
    }

    public static List<Search> GetNewItemsInStock()
    {
        var result = new List<Search>();

        // Iterate through the new searches
        foreach (var newSearch in ProductsInStock)
        {
            var oldSearch = OldProductsInStock.FirstOrDefault(s => s.Keyword == newSearch.Keyword);
            if (oldSearch != null)
            {
                // Find products in newSearch that are not in oldSearch
                var newProducts = newSearch.Products.Except(oldSearch.Products).ToList();
                if (newProducts.Any())
                {
                    result.Add(new Search(newSearch.Keyword, newProducts));
                }
            }
            else
            {
                // If there's no corresponding oldSearch, all new products are considered as differences
                result.Add(newSearch);
            }
        }

        return result;
    }
}