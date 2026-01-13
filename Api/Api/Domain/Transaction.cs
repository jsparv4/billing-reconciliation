namespace Api.Domain;

public sealed class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AccountId { get; set; }
    public Account? Account { get; set; }

    public DateTime OccurredAt { get; set; } // UTC
    public string Kind { get; set; } = "";
    public long AmountCents { get; set; } // signed
    public string Source { get; set; } = "import";
    public string? ExternalRef { get; set; }
}
