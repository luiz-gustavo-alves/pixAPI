using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace pixAPI.DTOs;

[DataContract]
public class UpdatePaymentStatusDTO
{
  [Required(ErrorMessage = "{0} é obrigatório.")]
  [RegularExpression(@"^(SUCCESS|FAILED)$", ErrorMessage = "{0} precisa ser: SUCCESS ou FAILED.")]
  [DataMember(Name = "status")]
  public string Status { get; set; } = null!;
}