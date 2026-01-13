namespace Api.Domain;

public sealed class Reconciliation
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AccountId { get; set; }
    public Account? Account { get; set; }

    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }

    public Guid? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    public long ExpectedTotalCents { get; set; }
    public long ActualTotalCents { get; set; }
    public long VarianceCents { get; set; }

    public string Status { get; set; } = "mismatch"; // match|mismatch
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
