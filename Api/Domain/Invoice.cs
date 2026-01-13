namespace Api.Domain;

public sealed class Invoice
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AccountId { get; set; }
    public Account? Account { get; set; }

    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; } // v0: inclusive

    public string Status { get; set; } = "draft"; // draft|final

    public long SubtotalCents { get; set; }
    public long TaxCents { get; set; }
    public long TotalCents { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<InvoiceLine> Lines { get; set; } = new();
}
