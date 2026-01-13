namespace Api.Domain;

public sealed class Account
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string? ExternalRef { get; set; }

    public List<Transaction> Transactions { get; set; } = new();
    public List<Invoice> Invoices { get; set; } = new();
    public List<ExpectedTotal> ExpectedTotals { get; set; } = new();
    public List<Reconciliation> Reconciliations { get; set; } = new();
}
