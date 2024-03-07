using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pixAPI.Models;

public class PaymentProvider
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public long Id { get; set; }

  [StringLength(64, MinimumLength = 64)]
  public string Token { get; set; } = null!;

  public string BankName { get; set; } = null!;

  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  [DefaultValue("getutcdate()")]
  public DateTime CreatedAt { get; set; }

  [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
  [DefaultValue("getutcdate()")]
  public DateTime UpdatedAt { get; set; }

  public ICollection<PaymentProviderAccount> PaymentProviderAccounts { get; set; } = null!;
}