using dev_library.Clients;
using dev_library.Data;
using dev_refined.Clients;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Text.Json;

class WebpageMonitor
{
    private static Timer timer;
    private static List<Search> OldProductsInStock = new();
    private static List<Search> ProductsInStock = new();
    private static JJClient jJClient = new();
    private static PokemonCenterClient PokemonCenterClient = new();
    private static DiscordClient discordClient = new();
    private static _401GamesClient _401GamesClient = new();
    private static CanadaComputersClient CanadaComputersClient = new();
    private static ChimeraClient ChimeraClient = new();
    private static AtlasClient AtlasClient = new();
    private static IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());

    static async Task Main()
    {
        Console.WriteLine("Program.Main: Starting to monitor stock...");

        // Start the initial scan and wait for it to complete

        AppSettings.Initialize();

        while (true)
        {
            await ScanStores(); 
            Console.WriteLine("Program.Main: Waiting 60 seconds...");
            Thread.Sleep(300000);
        }
        
        Console.ReadLine(); // Keep the program running
    }

    static async Task ScanStores()
    {
        ProductsInStock = new();

        await ScanAtlas();
        await ScanJJ(); // Initial check
        await ScanCanadaComputers();
        await ScanChimera();
        // await ScanPokemonCenter();
        // await ScanChimera();
        //await Scan401Games();
        await PostResults();
    }

    private static async Task PostResults()
    {
        var productsInStockJson = JsonConvert.SerializeObject(ProductsInStock);
        var oldProductsInStockJson = File.ReadAllText($"{AppSettings.BasePath}/pokemoncache.json");

        if (productsInStockJson != oldProductsInStockJson)
        {
            var newItemsInStock = GetNewItemsInStock();

            if (newItemsInStock.Count > 0)
            {
                await discordClient.PostWebHook(newItemsInStock);

                File.WriteAllText($"{AppSettings.BasePath}/pokemoncache.json", productsInStockJson);
                OldProductsInStock = ProductsInStock;
            }
        }
    }

    private static async Task ScanCanadaComputers()
    {
        try
        {
            var gpuResults = await CanadaComputersClient.GetProducts();
            ProductsInStock.AddRange(gpuResults);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting products for CC: {ex.Message}");
        }
    }

    private static async Task ScanChimera()
    {
        try
        {
            var chimeraResults = await ChimeraClient.GetPokemon();
            ProductsInStock.AddRange(chimeraResults);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting products for Chimera: {ex.Message}");
        }
    }


    private static async Task ScanAtlas()
    {
        try
        {
            var atlasResults = await AtlasClient.GetPokemon();
            ProductsInStock.AddRange(atlasResults);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting products for Atlas: {ex.Message}");
        }
    }

    private static async Task ScanJJ()
    {
        try
        {
            var jjResults = await jJClient.GetProducts();
            ProductsInStock.AddRange(jjResults);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting products for JJ: {ex.Message}");
        }
    }

    private static async Task Scan401Games()
    {
       // var pokemonCenterResults = await _401GamesClient.GetPokemon();
    }

    private static async Task ScanPokemonCenter()
    {
       // var pokemonCenterResults = await PokemonCenterClient.GetPokemon();
    }

   

    public static List<Search> GetNewItemsInStock()
    {
        var result = new List<Search>();

        // Iterate through the new searches
        foreach (var newSearch in ProductsInStock)
        {
            // Find old search with the same keyword and store
            var oldSearch = OldProductsInStock
                .FirstOrDefault(s => s.Keyword == newSearch.Keyword && s.Store == newSearch.Store);

            if (oldSearch != null)
            {
                // Find products in newSearch that are not in oldSearch based on product name
                var newProducts = newSearch.Products
                    .Where(newProduct => !oldSearch.Products.Any(oldProduct => oldProduct.Name == newProduct.Name))
                    .ToList();

                if (newProducts.Any())
                {
                    result.Add(new Search(newSearch.Keyword, newSearch.Store, newProducts));
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