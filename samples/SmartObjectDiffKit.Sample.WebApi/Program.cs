using SmartObjectDiffKit;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ObjectDiffer>(_ => ObjectDiffer.Create().Build());

var app = builder.Build();

app.MapPost("/api/diff", (DiffRequest request, ObjectDiffer differ) =>
{
    var result = differ.Compare(request.OldObject, request.NewObject);
    return Results.Ok(new
    {
        result.IsEqual,
        result.DifferenceCount,
        result.ElapsedTime,
        Differences = result.Differences.Select(d => new
        {
            d.PropertyPath,
            OldValue = d.OldValue?.ToString(),
            NewValue = d.NewValue?.ToString(),
            DifferenceType = d.DifferenceType.ToString(),
            Severity = d.Severity.ToString()
        })
    });
});

app.MapPost("/api/diff/json", (DiffRequest request, ObjectDiffer differ) =>
{
    var result = differ.Compare(request.OldObject, request.NewObject);
    return Results.Content(result.ToJson(), "application/json");
});

app.MapPost("/api/diff/markdown", (DiffRequest request, ObjectDiffer differ) =>
{
    var result = differ.Compare(request.OldObject, request.NewObject);
    return Results.Content(result.ToMarkdown(), "text/markdown");
});

app.MapPost("/api/diff/html", (DiffRequest request, ObjectDiffer differ) =>
{
    var result = differ.Compare(request.OldObject, request.NewObject);
    return Results.Content(result.ToHtml(), "text/html");
});

app.Run();

public record DiffRequest(object? OldObject, object? NewObject);
