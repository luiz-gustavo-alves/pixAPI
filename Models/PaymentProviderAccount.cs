using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pixAPI.Models;

public class PaymentProviderAccount : BaseEntity
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

  public User User { get; set; } = null!;
  public PaymentProvider Bank { get; set; } = null!;
  public ICollection<PixKey> PixKeys { get; set; } = null!;
  public ICollection<Payments> Payments { get; set; } = null!;
}