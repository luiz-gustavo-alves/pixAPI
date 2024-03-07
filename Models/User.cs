using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace pixAPI.Models;

public class User
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public long Id { get; set; }

  [StringLength(11, MinimumLength = 11)]
  public string CPF { get; set; } = null!;

  public string Name { get; set; } = null!;

  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  [DefaultValue("getutcdate()")]
  public DateTime CreatedAt { get; set; }

  [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
  [DefaultValue("getutcdate()")]
  public DateTime UpdatedAt { get; set; }

  public ICollection<PaymentProviderAccount> PaymentProviderAccounts { get; set; } = null!;
}