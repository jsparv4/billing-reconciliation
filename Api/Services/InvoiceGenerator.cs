using Api.Domain;

namespace Api.Services;

public sealed class InvoiceGenerator
{
    public GeneratedInvoice Generate(
        Guid accountId,
        DateOnly periodStart,
        DateOnly periodEnd,
        IReadOnlyList<Transaction> transactions)
    {
        var lines = transactions
            .GroupBy(t => t.Kind)
            .Select(g => new GeneratedInvoiceLine(
                Code: g.Key.ToUpperInvariant(),
                Description: g.Key,
                AmountCents: g.Sum(x => x.AmountCents)
            ))
            .OrderBy(l => l.Code)
            .ToList();

        var subtotal = lines.Sum(l => l.AmountCents);
        var tax = 0L;
        var total = subtotal + tax;

        return new GeneratedInvoice(
            AccountId: accountId,
            PeriodStart: periodStart,
            PeriodEnd: periodEnd,
            Lines: lines,
            SubtotalCents: subtotal,
            TaxCents: tax,
            TotalCents: total
        );
    }
}

public sealed record GeneratedInvoice(
    Guid AccountId,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    IReadOnlyList<GeneratedInvoiceLine> Lines,
    long SubtotalCents,
    long TaxCents,
    long TotalCents
);

public sealed record GeneratedInvoiceLine(
    string Code,
    string Description,
    long AmountCents
);
