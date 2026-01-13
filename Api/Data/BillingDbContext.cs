using Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public sealed class BillingDbContext : DbContext
{
    public BillingDbContext(DbContextOptions<BillingDbContext> options) : base(options) { }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<ExpectedTotal> ExpectedTotals => Set<ExpectedTotal>();
    public DbSet<Reconciliation> Reconciliations => Set<Reconciliation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>()
            .HasIndex(a => a.ExternalRef);

        modelBuilder.Entity<Transaction>()
            .HasIndex(t => new { t.AccountId, t.OccurredAt });

        modelBuilder.Entity<Invoice>()
            .HasIndex(i => new { i.AccountId, i.PeriodStart, i.PeriodEnd });

        modelBuilder.Entity<ExpectedTotal>()
            .HasIndex(e => new { e.AccountId, e.PeriodStart, e.PeriodEnd });

        modelBuilder.Entity<Reconciliation>()
            .HasIndex(r => new { r.AccountId, r.PeriodStart, r.PeriodEnd });

        // DateOnly -> string for SQLite
        modelBuilder.Entity<Invoice>().Property(i => i.PeriodStart).HasConversion<string>();
        modelBuilder.Entity<Invoice>().Property(i => i.PeriodEnd).HasConversion<string>();

        modelBuilder.Entity<ExpectedTotal>().Property(e => e.PeriodStart).HasConversion<string>();
        modelBuilder.Entity<ExpectedTotal>().Property(e => e.PeriodEnd).HasConversion<string>();

        modelBuilder.Entity<Reconciliation>().Property(r => r.PeriodStart).HasConversion<string>();
        modelBuilder.Entity<Reconciliation>().Property(r => r.PeriodEnd).HasConversion<string>();
    }
}
