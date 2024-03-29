using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pixAPI.Models;

public class PixKey : BaseEntity
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public long Id { get; set; }

  [ForeignKey(nameof(PaymentProviderAccount))]
  public long PaymentProviderAccountId { get; set; }

  public required KeyType Type { get; set; }
  public required string Value { get; set; }

  public PaymentProviderAccount PaymentProviderAccount { get; set; } = null!;
  public ICollection<Payments> Payments { get; set; } = null!;
}