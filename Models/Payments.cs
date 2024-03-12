using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pixAPI.Models;

public class Payments : BaseEntity
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public long Id { get; set; }

  public PaymentStatus Status { get; set; }

  [ForeignKey(nameof(PixKey))]
  public long PixKeyId { get; set; }

  [ForeignKey(nameof(PaymentProviderAccount))]
  public long PaymentProviderAccountId { get; set; }

  public int Amount { get; set; }

  [StringLength(256)]
  public string? Description { get; set; }

  public PixKey PixKey { get; set; } = null!;
  public PaymentProviderAccount PaymentProviderAccount { get; set; } = null!;
}