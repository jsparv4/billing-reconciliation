using Api.Data;
using Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Api.Endpoints;

public static class TransactionsEndpoints
{
    public static void MapTransactions(this WebApplication app)
    {
        app.MapPost("/accounts/{accountId:guid}/transactions", async (BillingDbContext db, Guid accountId, CreateTransactionRequest req) =>
        {
            var exists = await db.Accounts.AnyAsync(a => a.Id == accountId);
            if (!exists) return Results.NotFound(new { error = "Account not found." });

            if (string.IsNullOrWhiteSpace(req.Kind)) return Results.BadRequest(new { error = "Kind is required." });
            if (string.IsNullOrWhiteSpace(req.Source)) return Results.BadRequest(new { error = "Source is required." });

            var occurredUtc = req.OccurredAtUtc.Kind == DateTimeKind.Utc
                ? req.OccurredAtUtc
                : req.OccurredAtUtc.ToUniversalTime();

            var tx = new Transaction
            {
                AccountId = accountId,
                OccurredAt = occurredUtc,
                Kind = req.Kind.Trim(),
                AmountCents = req.AmountCents,
                Source = req.Source.Trim(),
                ExternalRef = req.ExternalRef?.Trim()
            };

            db.Transactions.Add(tx);
            await db.SaveChangesAsync();

            return Results.Created($"/accounts/{accountId}/transactions/{tx.Id}", new { tx.Id });
        });

        app.MapGet("/accounts/{accountId:guid}/transactions", async (BillingDbContext db, Guid accountId, DateOnly start, DateOnly end) =>
        {
            if (end < start) return Results.BadRequest(new { error = "end must be >= start" });

            var startDt = start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var endDt = end.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

            var items = await db.Transactions.AsNoTracking()
                .Where(t => t.AccountId == accountId && t.OccurredAt >= startDt && t.OccurredAt <= endDt)
                .OrderBy(t => t.OccurredAt)
                .Select(t => new { t.Id, t.OccurredAt, t.Kind, t.AmountCents, t.Source, t.ExternalRef })
                .ToListAsync();

            return Results.Ok(items);
        });
    }

    public sealed record CreateTransactionRequest(
        DateTime OccurredAtUtc,
        string Kind,
        long AmountCents,
        string Source,
        string? ExternalRef
    );
}
