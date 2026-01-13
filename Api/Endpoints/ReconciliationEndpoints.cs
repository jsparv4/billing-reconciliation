using Api.Data;
using Api.Domain;
using Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Api.Endpoints;

public static class ReconciliationEndpoints
{
    public static void MapReconciliation(this WebApplication app)
    {
        // Create an expected total for a period
        app.MapPost("/accounts/{accountId:guid}/expected-totals", async (
            BillingDbContext db,
            Guid accountId,
            CreateExpectedTotalRequest req) =>
        {
            if (req.PeriodEnd < req.PeriodStart)
                return Results.BadRequest(new { error = "PeriodEnd must be >= PeriodStart" });

            var exists = await db.Accounts.AnyAsync(a => a.Id == accountId);
            if (!exists) return Results.NotFound(new { error = "Account not found." });

            var expected = new ExpectedTotal
            {
                AccountId = accountId,
                PeriodStart = req.PeriodStart,
                PeriodEnd = req.PeriodEnd,
                ExpectedTotalCents = req.ExpectedTotalCents,
                Source = string.IsNullOrWhiteSpace(req.Source) ? "manual_expectation" : req.Source.Trim(),
                ExternalRef = req.ExternalRef?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            db.ExpectedTotals.Add(expected);
            await db.SaveChangesAsync();

            return Results.Created($"/accounts/{accountId}/expected-totals/{expected.Id}", new
            {
                expected.Id,
                expected.AccountId,
                expected.PeriodStart,
                expected.PeriodEnd,
                expected.ExpectedTotalCents,
                expected.Source,
                expected.ExternalRef
            });
        });

        // Reconcile expected vs invoice total for the period
        app.MapPost("/accounts/{accountId:guid}/reconcile", async (
            BillingDbContext db,
            Reconciler reconciler,
            Guid accountId,
            DateOnly start,
            DateOnly end) =>
        {
            if (end < start)
                return Results.BadRequest(new { error = "end must be >= start" });

            var accountExists = await db.Accounts.AnyAsync(a => a.Id == accountId);
            if (!accountExists) return Results.NotFound(new { error = "Account not found." });

            var expected = await db.ExpectedTotals.AsNoTracking()
                .Where(e => e.AccountId == accountId && e.PeriodStart == start && e.PeriodEnd == end)
                .OrderByDescending(e => e.CreatedAt)
                .FirstOrDefaultAsync();

            if (expected is null)
                return Results.BadRequest(new { error = "No expected total found for this period. Create one first." });

            var invoice = await db.Invoices.AsNoTracking()
                .Where(i => i.AccountId == accountId && i.PeriodStart == start && i.PeriodEnd == end)
                .OrderByDescending(i => i.CreatedAt)
                .FirstOrDefaultAsync();

            if (invoice is null)
                return Results.BadRequest(new { error = "No invoice found for this period. Generate one first." });

            var result = reconciler.Reconcile(expected.ExpectedTotalCents, invoice.TotalCents);

            var rec = new Reconciliation
            {
                AccountId = accountId,
                PeriodStart = start,
                PeriodEnd = end,
                InvoiceId = invoice.Id,
                ExpectedTotalCents = result.ExpectedTotalCents,
                ActualTotalCents = result.ActualTotalCents,
                VarianceCents = result.VarianceCents,
                Status = result.Status,
                CreatedAt = DateTime.UtcNow
            };

            db.Reconciliations.Add(rec);
            await db.SaveChangesAsync();

            return Results.Created($"/reconciliations/{rec.Id}", new
            {
                rec.Id,
                rec.AccountId,
                rec.PeriodStart,
                rec.PeriodEnd,
                rec.InvoiceId,
                rec.ExpectedTotalCents,
                rec.ActualTotalCents,
                rec.VarianceCents,
                rec.Status
            });
        });
    }

    public sealed record CreateExpectedTotalRequest(
        DateOnly PeriodStart,
        DateOnly PeriodEnd,
        long ExpectedTotalCents,
        string? Source,
        string? ExternalRef
    );
}
