using Api.Data;
using Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Api.Endpoints;

public static class AccountsEndpoints
{
    public static void MapAccounts(this WebApplication app)
    {
        app.MapPost("/accounts", async (BillingDbContext db, CreateAccountRequest req) =>
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return Results.BadRequest(new { error = "Name is required." });

            var account = new Account
            {
                Name = req.Name.Trim(),
                ExternalRef = req.ExternalRef?.Trim()
            };

            db.Accounts.Add(account);
            await db.SaveChangesAsync();

            return Results.Created($"/accounts/{account.Id}", new { account.Id, account.Name, account.ExternalRef });
        });

        app.MapGet("/accounts/{id:guid}", async (BillingDbContext db, Guid id) =>
        {
            var a = await db.Accounts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return a is null
                ? Results.NotFound()
                : Results.Ok(new { a.Id, a.Name, a.ExternalRef });
        });
    }

    public sealed record CreateAccountRequest(string Name, string? ExternalRef);
}
