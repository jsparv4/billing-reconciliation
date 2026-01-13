namespace Api.Services;

public sealed class Reconciler
{
    public ReconciliationResult Reconcile(long expectedTotalCents, long actualTotalCents)
    {
        var variance = actualTotalCents - expectedTotalCents;
        var status = variance == 0 ? "match" : "mismatch";
        return new ReconciliationResult(expectedTotalCents, actualTotalCents, variance, status);
    }
}

public sealed record ReconciliationResult(
    long ExpectedTotalCents,
    long ActualTotalCents,
    long VarianceCents,
    string Status
);
