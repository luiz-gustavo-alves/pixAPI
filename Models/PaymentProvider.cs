using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pixAPI.Models;

public class PaymentProvider : BaseEntity
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public long Id { get; set; }

  [StringLength(64, MinimumLength = 64)]
  public string Token { get; set; } = null!;

  public string BankName { get; set; } = null!;
  public ICollection<PaymentProviderAccount> PaymentProviderAccounts { get; set; } = null!;
}
