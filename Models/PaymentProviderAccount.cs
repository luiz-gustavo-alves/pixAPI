using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pixAPI.Models;

public class PaymentProviderAccount
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public long Id { get; set; }

  [ForeignKey(nameof(User))]
  public long UserId { get; set; }

  [ForeignKey(nameof(Bank))]
  public long BankId { get; set; }

  public string Agency { get; set; } = null!;
  public string Number { get; set; } = null!;

  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  [DefaultValue("getutcdate()")]
  public DateTime CreatedAt { get; set; }

  [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
  [DefaultValue("getutcdate()")]
  public DateTime UpdatedAt { get; set; }

  public User User { get; set; } = null!;
  public PaymentProvider Bank { get; set; } = null!;
  public ICollection<PixKey> PixKeys { get; set; } = null!;
}