namespace Api.Domain;

public sealed class InvoiceLine
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    public string Code { get; set; } = "";
    public string Description { get; set; } = "";
    public long AmountCents { get; set; }
}
