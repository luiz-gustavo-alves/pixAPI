using Microsoft.EntityFrameworkCore;
using pixAPI.Models;

namespace pixAPI.Data;

public class AppDBContext(DbContextOptions<AppDBContext> options) : DbContext(options)
{
  public DbSet<User> User { get; set; } = null!;
  public DbSet<PaymentProvider> PaymentProvider { get; set; } = null!;
  public DbSet<PaymentProviderAccount> PaymentProviderAccount { get; set; } = null!;
  public DbSet<PixKey> PixKey { get; set; } = null!;

  protected override void OnModelCreating(ModelBuilder builder)
  {
    builder.Entity<User>().HasKey(user => user.Id);
    builder.Entity<PaymentProvider>().HasKey(paymentProvider => paymentProvider.Id);
    builder.Entity<PaymentProviderAccount>().HasKey(paymentProviderAccount => paymentProviderAccount.Id);
    builder.Entity<PixKey>().HasKey(pixKey => pixKey.Id);
    builder.Entity<User>().HasIndex(user => user.CPF).IsUnique();
    builder.Entity<PaymentProvider>().HasIndex(paymentProvider => paymentProvider.Token).IsUnique();
    builder.Entity<PaymentProvider>().HasIndex(paymentProvider => paymentProvider.BankName).IsUnique();
  }
}