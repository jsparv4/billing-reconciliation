using Api.Data;
using Api.Domain;
using Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Api.Endpoints;

public static class InvoicesEndpoints
{
    public static void MapInvoices(this WebApplication app)
    {
        app.MapPost(
            "/accounts/{accountId:guid}/invoices/generate",
            async (
                BillingDbContext db,
                InvoiceGenerator generator,
                Guid accountId,
                DateOnly start,
                DateOnly end) =>
            {
                if (end < start)
                    return Results.BadRequest(new { error = "end must be >= start" });

                var accountExists = await db.Accounts.AnyAsync(a => a.Id == accountId);
                if (!accountExists)
                    return Results.NotFound(new { error = "Account not found" });

                var startDt = start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                var endDt = end.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

                var transactions = await db.Transactions
                    .Where(t =>
                        t.AccountId == accountId &&
                        t.OccurredAt >= startDt &&
                        t.OccurredAt <= endDt)
                    .ToListAsync();

                var generated = generator.Generate(
                    accountId,
                    start,
                    end,
                    transactions);

                var invoice = new Invoice
                {
                    AccountId = generated.AccountId,
                    PeriodStart = generated.PeriodStart,
                    PeriodEnd = generated.PeriodEnd,
                    Status = "draft",
                    SubtotalCents = generated.SubtotalCents,
                    TaxCents = generated.TaxCents,
                    TotalCents = generated.TotalCents,
                    Lines = generated.Lines.Select(l => new InvoiceLine
                    {
                        Code = l.Code,
                        Description = l.Description,
                        AmountCents = l.AmountCents
                    }).ToList()
                };

                db.Invoices.Add(invoice);
                await db.SaveChangesAsync();

                return Results.Created(
                    $"/invoices/{invoice.Id}",
                    new
                    {
                        invoice.Id,
                        invoice.AccountId,
                        invoice.PeriodStart,
                        invoice.PeriodEnd,
                        invoice.Status,
                        invoice.SubtotalCents,
                        invoice.TotalCents,
                        Lines = invoice.Lines.Select(l => new
                        {
                            l.Code,
                            l.Description,
                            l.AmountCents
                        })
                    });
            });
    }
}
