using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pixAPI.Models;

public class User : BaseEntity
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public long Id { get; set; }

  [StringLength(11, MinimumLength = 11)]
  public string CPF { get; set; } = null!;

  public string Name { get; set; } = null!;

  public ICollection<PaymentProviderAccount> PaymentProviderAccounts { get; set; } = null!;
}