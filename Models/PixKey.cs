using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pixAPI.Models;

public class PixKey
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public long Id { get; set; }

  [ForeignKey(nameof(PaymentProviderAccount))]
  public long PaymentProviderAccountId { get; set; }

  public required KeyType Type { get; set; }
  public required float Value { get; set; }

  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  [DefaultValue("getutcdate()")]
  public DateTime CreatedAt { get; set; }

  [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
  [DefaultValue("getutcdate()")]
  public DateTime UpdatedAt { get; set; }

  public PaymentProviderAccount PaymentProviderAccount { get; set; } = null!;
}