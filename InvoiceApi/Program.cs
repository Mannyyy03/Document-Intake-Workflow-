using Npgsql;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var cs = "Host=localhost;Port=5432;Username=intake;Password=intake;Database=intake";

// The web page is the default (home) page
app.MapGet("/", () =>
    Results.File(Path.Combine(app.Environment.ContentRootPath, "index.html"), "text/html"));

// API: saved invoices
app.MapGet("/api/invoices", async () =>
{
    var list = new List<object>();
    await using var conn = new NpgsqlConnection(cs);
    await conn.OpenAsync();
    await using var cmd = new NpgsqlCommand(
        "SELECT id, invoice_number, vendor, invoice_date, total, category FROM invoices ORDER BY id", conn);
    await using var r = await cmd.ExecuteReaderAsync();
    while (await r.ReadAsync())
        list.Add(new
        {
            id = r.GetInt32(0),
            invoiceNumber = r.GetString(1),
            vendor = r.GetString(2),
            invoiceDate = r.GetDateTime(3).ToString("yyyy-MM-dd"),
            total = r.GetDecimal(4),
            category = r.GetString(5)
        });
    return Results.Ok(list);
});

// API: items flagged for review
app.MapGet("/api/review-queue", async () =>
{
    var list = new List<object>();
    await using var conn = new NpgsqlConnection(cs);
    await conn.OpenAsync();
    await using var cmd = new NpgsqlCommand(
        "SELECT id, error, raw_output FROM review_queue ORDER BY id", conn);
    await using var r = await cmd.ExecuteReaderAsync();
    while (await r.ReadAsync())
        list.Add(new
        {
            id = r.GetInt32(0),
            error = r.IsDBNull(1) ? null : r.GetString(1),
            rawOutput = r.IsDBNull(2) ? null : r.GetString(2)
        });
    return Results.Ok(list);
});

app.Run();