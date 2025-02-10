using scalp_fighter.Clients;
using scalp_fighter.Data;
using System.Text.Json;
using Timer = System.Timers.Timer;

class WebpageMonitor {

    private static Timer timer;
    private static List<Search> OldProductsInStock = new();
    private static List<Search> ProductsInStock = new();
    private static JJClient jJClient = new ();
    private static PokemonCenterClient PokemonCenterClient = new();
    private static DiscordClient discordClient = new();
    private static _401GamesClient _401GamesClient = new();
    private static CanadaComputersClient CanadaComputersClient = new();
    private static ChimeraClient ChimeraClient = new();

    static async Task Main () {
        Console.WriteLine ("Program.Main: Starting in stock monitor...");
        OldProductsInStock = JsonSerializer.Deserialize<List<Search>>(File.ReadAllText("Cache.json"));
        timer = new Timer (60000); // Check every 60 seconds
        timer.Elapsed += async (sender, e) => await HandleElapsedEvent();
       
        timer.Start ();

        ProductsInStock = new();
        await ScanJJ (); // Initial check
        await ScanCanadaComputers();
        //await ScanChimera();
       // await ScanPokemonCenter();
       // await ScanChimera();
      //  await Scan401Games();
      await PostResults();
        Console.ReadLine (); // Keep the program running
    }

   static async Task HandleElapsedEvent()
    {
        Console.WriteLine("Program.HandleElapsedEvent: Waiting 60 seconds");
        ProductsInStock = new();
        await ScanJJ();
        await ScanCanadaComputers();
       // await ScanChimera();
        await PostResults();
        //   await ScanPokemonCenter();
        // await ScanChimera();
        //  await Scan401Games();
    }

    private static async Task PostResults()
    {
        var productsInStockJson = JsonSerializer.Serialize(ProductsInStock);
        var oldProductsInStockJson = JsonSerializer.Serialize(OldProductsInStock);

        if (productsInStockJson != oldProductsInStockJson)
        {
            var newItemsInStock = GetNewItemsInStock();
            var webhookValue = "";

            if (newItemsInStock.Count > 0)
            {
                foreach (var itemInStock in newItemsInStock)
                {
                    if (itemInStock.Products.Count > 0)
                    {
                        webhookValue += $"{itemInStock.Keyword} Products:\n";
                    }
                    foreach (var product in itemInStock.Products)
                    {
                        var productInfo = $"New Item Now In Stock: {product.Name}, Price: {product.Price}";
                        Console.WriteLine($"Program.PostResults: {productInfo}");
                        webhookValue += product.Url;
                    }
                }

                await discordClient.PostWebHook(webhookValue);

            }
            
            File.WriteAllText("Cache.json", productsInStockJson);
            OldProductsInStock = ProductsInStock;
        }
    }

    private static async Task ScanCanadaComputers()
    {
        var gpuResults = await CanadaComputersClient.GetProducts();
        ProductsInStock.AddRange(gpuResults);
    }

    private static async Task ScanChimera()
    {
        var pokemonResults = await ChimeraClient.GetPokemon();
    }

    private static async Task Scan401Games()
    {
        var pokemonCenterResults = await _401GamesClient.GetPokemon();
    }

    private static async Task ScanPokemonCenter()
    {
        var pokemonCenterResults = await PokemonCenterClient.GetPokemon();
    }

    private static async Task ScanJJ () {
        try {
            var jjResults = await jJClient.GetProducts();
            ProductsInStock.AddRange(jjResults);

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