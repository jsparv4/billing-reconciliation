namespace Api.Domain;

public sealed class ExpectedTotal
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AccountId { get; set; }
    public Account? Account { get; set; }

    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }

    public long ExpectedTotalCents { get; set; }
    public string Source { get; set; } = "manual_expectation";
    public string? ExternalRef { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
