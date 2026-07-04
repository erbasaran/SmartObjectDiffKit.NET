namespace SmartObjectDiffKit.UnitTests.TestModels;

public class SimpleObject
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public Address? Address { get; set; }
    public List<Order>? Orders { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public string Product { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public List<OrderItem>? Items { get; set; }
}

public class OrderItem
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class IgnoredPropertyModel
{
    public int Id { get; set; }

    [DiffIgnore]
    public DateTime LastModified { get; set; }

    public string Name { get; set; } = string.Empty;
}

public class KeyedItem
{
    [DiffKey]
    public string Id { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class OrderedModel
{
    [DiffOrder(1)]
    public string First { get; set; } = string.Empty;

    [DiffOrder(2)]
    public string Second { get; set; } = string.Empty;

    [DiffOrder(3)]
    public string Third { get; set; } = string.Empty;
}

public class DisplayModel
{
    [DiffDisplayName("Customer Name")]
    public string Name { get; set; } = string.Empty;
}

public record PersonRecord(string Name, int Age);

public struct PointStruct
{
    public double X { get; set; }
    public double Y { get; set; }
}

public class CircularA
{
    public string Name { get; set; } = string.Empty;
    public CircularB? B { get; set; }
}

public class CircularB
{
    public string Name { get; set; } = string.Empty;
    public CircularA? A { get; set; }
}

public class Animal
{
    public string Name { get; set; } = string.Empty;
}

public class Dog : Animal
{
    public string Breed { get; set; } = string.Empty;
}

public class Cat : Animal
{
    public bool IsIndoor { get; set; }
}

public enum Color
{
    Red,
    Green,
    Blue
}
