using Api.Data;
using Api.Endpoints;
using Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<BillingDbContext>(opt =>
{
    var cs = builder.Configuration.GetConnectionString("BillingDb");

    if (string.IsNullOrWhiteSpace(cs))
        throw new InvalidOperationException("Missing ConnectionStrings:BillingDb in appsettings.json");

    opt.UseSqlite(cs);
});

// Services (business logic)
builder.Services.AddScoped<InvoiceGenerator>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Auto-migrate on startup (fine for local/dev)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
    await db.Database.MigrateAsync();
}

// Middleware
app.UseSwagger();
app.UseSwaggerUI();

// Endpoints
app.MapGet("/health", () => Results.Ok(new { ok = true }));

app.MapAccounts();
app.MapTransactions();
app.MapInvoices();

app.Run();
