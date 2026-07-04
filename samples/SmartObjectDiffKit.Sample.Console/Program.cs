using SmartObjectDiffKit;

var oldCustomer = new Customer
{
    Id = 1,
    Name = "John Doe",
    Email = "john@example.com",
    Address = new Address { Street = "123 Main St", City = "Springfield", ZipCode = "12345" },
    Orders = new List<Order>
    {
        new() { Id = 1, Product = "Widget", Quantity = 5, Price = 9.99m },
        new() { Id = 2, Product = "Gadget", Quantity = 2, Price = 19.99m }
    }
};

var newCustomer = new Customer
{
    Id = 1,
    Name = "John Smith",
    Email = "john.smith@example.com",
    Address = new Address { Street = "456 Oak Ave", City = "Shelbyville", ZipCode = "67890" },
    Orders = new List<Order>
    {
        new() { Id = 1, Product = "Widget", Quantity = 10, Price = 9.99m },
        new() { Id = 3, Product = "Doohickey", Quantity = 1, Price = 4.99m }
    }
};

Console.WriteLine("=== SmartObjectDiffKit Console Sample ===\n");

var result = ObjectDiffer.Create()
    .MaxDepth(10)
    .Compare(oldCustomer, newCustomer);

Console.WriteLine(result.ToConsole());

Console.WriteLine("\n--- JSON Output ---\n");
Console.WriteLine(result.ToJson());

Console.WriteLine("\n--- Markdown Output ---\n");
Console.WriteLine(result.ToMarkdown());

Console.WriteLine($"\nStatistics:");
Console.WriteLine($"  Elapsed: {result.ElapsedTime.TotalMilliseconds:F2}ms");
Console.WriteLine($"  Objects visited: {result.Statistics.VisitedObjects}");
Console.WriteLine($"  Properties compared: {result.Statistics.ComparedProperties}");
Console.WriteLine($"  Max depth: {result.Statistics.MaximumDepthReached}");

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public Address? Address { get; set; }
    public List<Order>? Orders { get; set; }
}

public class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
}

public class Order
{
    public int Id { get; set; }
    public string Product { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
