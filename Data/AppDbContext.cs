using Microsoft.EntityFrameworkCore;
using pixAPI.Models;

namespace pixAPI.Data;

public class AppDBContext(DbContextOptions<AppDBContext> options) : DbContext(options)
{
  public DbSet<User> User { get; set; } = null!;
  public DbSet<PaymentProvider> PaymentProvider { get; set; } = null!;
  public DbSet<PaymentProviderAccount> PaymentProviderAccount { get; set; } = null!;
  public DbSet<PixKey> PixKey { get; set; } = null!;
  public DbSet<Payments> Payments { get; set; } = null!;

  protected override void OnModelCreating(ModelBuilder builder)
  {
    builder.Entity<User>().HasKey(user => user.Id);
    builder.Entity<PaymentProvider>().HasKey(paymentProvider => paymentProvider.Id);
    builder.Entity<PaymentProviderAccount>().HasKey(paymentProviderAccount => paymentProviderAccount.Id);
    builder.Entity<PixKey>().HasKey(pixKey => pixKey.Id);
    builder.Entity<Payments>().HasKey(payments => payments.Id);
  
    builder.Entity<User>().HasIndex(user => user.CPF).IsUnique();
    builder.Entity<PaymentProvider>().HasIndex(paymentProvider => paymentProvider.Token).IsUnique();
    builder.Entity<PaymentProvider>().HasIndex(paymentProvider => paymentProvider.BankName).IsUnique();

    builder.Entity<User>().Property(b => b.UpdatedAt).HasDefaultValueSql("now()");
    builder.Entity<User>().Property(b => b.CreatedAt).HasDefaultValueSql("now()");
    builder.Entity<PaymentProvider>().Property(b => b.UpdatedAt).HasDefaultValueSql("now()");
    builder.Entity<PaymentProvider>().Property(b => b.CreatedAt).HasDefaultValueSql("now()");
    builder.Entity<PaymentProviderAccount>().Property(b => b.UpdatedAt).HasDefaultValueSql("now()");
    builder.Entity<PaymentProviderAccount>().Property(b => b.CreatedAt).HasDefaultValueSql("now()");
    builder.Entity<PixKey>().Property(b => b.UpdatedAt).HasDefaultValueSql("now()");
    builder.Entity<PixKey>().Property(b => b.CreatedAt).HasDefaultValueSql("now()");
    builder.Entity<Payments>().Property(b => b.UpdatedAt).HasDefaultValueSql("now()");
    builder.Entity<Payments>().Property(b => b.CreatedAt).HasDefaultValueSql("now()");
  }

  public override int SaveChanges()
  {
    var entries = ChangeTracker
        .Entries()
        .Where(e => e.Entity is BaseEntity && (
                e.State == EntityState.Added
                || e.State == EntityState.Modified));

    foreach (var entityEntry in entries)
    {
      ((BaseEntity)entityEntry.Entity).UpdatedAt = DateTime.Now;
      if (entityEntry.State == EntityState.Added)
      {
        ((BaseEntity)entityEntry.Entity).CreatedAt = DateTime.Now;
      }
    }
    return base.SaveChanges();
  }
}