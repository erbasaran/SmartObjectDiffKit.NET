using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace SmartObjectDiffKit.Benchmarks;

public class Program
{
    public static void Main(string[] args) => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
}

[MemoryDiagnoser]
public class ComparisonBenchmarks
{
    private SmallObject _smallOld = null!;
    private SmallObject _smallNew = null!;
    private LargeObject _largeOld = null!;
    private LargeObject _largeNew = null!;
    private List<SmallObject> _collectionOld = null!;
    private List<SmallObject> _collectionNew = null!;
    private NestedObject _nestedOld = null!;
    private NestedObject _nestedNew = null!;

    [GlobalSetup]
    public void Setup()
    {
        _smallOld = new SmallObject { Id = 1, Name = "Test", Value = 42.5 };
        _smallNew = new SmallObject { Id = 2, Name = "Changed", Value = 99.9 };

        _largeOld = CreateLargeObject("old");
        _largeNew = CreateLargeObject("new");

        _collectionOld = Enumerable.Range(0, 100).Select(i => new SmallObject { Id = i, Name = $"Item{i}", Value = i }).ToList();
        _collectionNew = Enumerable.Range(0, 100).Select(i => new SmallObject { Id = i, Name = i % 2 == 0 ? $"Item{i}" : $"Changed{i}", Value = i * 2 }).ToList();

        _nestedOld = new NestedObject
        {
            Level1 = new NestedObject
            {
                Level1 = new NestedObject { Small = new SmallObject { Id = 1, Name = "Deep", Value = 1.0 } }
            }
        };
        _nestedNew = new NestedObject
        {
            Level1 = new NestedObject
            {
                Level1 = new NestedObject { Small = new SmallObject { Id = 2, Name = "DeepChanged", Value = 2.0 } }
            }
        };
    }

    [Benchmark]
    public DiffResult SmallObjectComparison()
    {
        return ObjectDiffer.Create().Compare(_smallOld, _smallNew);
    }

    [Benchmark]
    public DiffResult LargeObjectComparison()
    {
        return ObjectDiffer.Create().Compare(_largeOld, _largeNew);
    }

    [Benchmark]
    public DiffResult CollectionComparison()
    {
        return ObjectDiffer.Create().Compare(_collectionOld, _collectionNew);
    }

    [Benchmark]
    public DiffResult NestedObjectComparison()
    {
        return ObjectDiffer.Create().Compare(_nestedOld, _nestedNew);
    }

    [Benchmark]
    public DiffResult EqualObjects()
    {
        return ObjectDiffer.Create().Compare(_smallOld, _smallOld);
    }

    private static LargeObject CreateLargeObject(string prefix)
    {
        return new LargeObject
        {
            Id = 1,
            Name = $"{prefix}_name",
            Description = $"{prefix}_description",
            Value1 = 1.0,
            Value2 = 2.0,
            Value3 = 3.0,
            Date1 = DateTime.Now,
            Date2 = DateTime.Now.AddDays(1),
            Flag1 = true,
            Flag2 = false,
            Items = Enumerable.Range(0, 20).Select(i => new SmallObject { Id = i, Name = $"{prefix}_{i}", Value = i }).ToList(),
            Metadata = Enumerable.Range(0, 10).ToDictionary(i => $"{prefix}_key{i}", i => $"{prefix}_val{i}")
        };
    }
}

public class SmallObject
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
}

public class LargeObject
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Value1 { get; set; }
    public double Value2 { get; set; }
    public double Value3 { get; set; }
    public DateTime Date1 { get; set; }
    public DateTime Date2 { get; set; }
    public bool Flag1 { get; set; }
    public bool Flag2 { get; set; }
    public List<SmallObject> Items { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class NestedObject
{
    public NestedObject? Level1 { get; set; }
    public SmallObject? Small { get; set; }
}
