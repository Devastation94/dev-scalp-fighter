namespace scalp_fighter.Data {
    public class Search
    {
        public Search(string keyword, List<Product> products) 
        { 
            Keyword = keyword;
            Products = products;
        }
        public string Keyword { get; set; }
        public List<Product> Products { get; set; }
    }
    public class Product {
        public string Name { get; }
        public string Price { get; }
        public string Status { get; }
        public string Url { get; set; }

        public Product(string name, string price, string status, string url) {
            Name = name;
            Price = price;
            Status = status;
            Url = $"[{Name}]({url})\n";
        }
    }
}