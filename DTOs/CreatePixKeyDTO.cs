using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace pixAPI.DTOs;

[DataContract]
public class KeyCreatePixKeySchema
{
  [Required(ErrorMessage = "{0} é obrigatório.")]
  [DataMember(Name = "value")]
  public string Value { get; set; } = null!;

  [Required(ErrorMessage = "{0} é obrigatório.")]
  [RegularExpression(@"^(CPF|Email|Phone|Random)$", ErrorMessage = "{0} precisa ser: CPF, Email, Phone ou Random.")]
  [DataMember(Name = "type")]
  public string Type { get; set; } = null!;
}

[DataContract]
public class UserCreatePixKeySchema
{
  [Required(ErrorMessage = "{0} é obrigatório.")]
  [RegularExpression(@"^\d+$", ErrorMessage = "{0} precisa ser um CPF válido")]
  [StringLength(11, MinimumLength = 11, ErrorMessage = "{0} precisa ter 11 dígitos.")]
  [DataMember(Name = "cpf")]
  public string CPF { get; set; } = null!;
}

[DataContract]
public class AccountCreatePixKeySchema
{
  [Required(ErrorMessage = "{0} é obrigatório.")]
  [RegularExpression(@"^\d+$", ErrorMessage = "{0} precisa ser um número de conta válido")]
  [StringLength(8, MinimumLength = 4, ErrorMessage = "{0} precisa estar entre 4 e 8 caracteres.")]
  [DataMember(Name = "number")]
  public string Number { get; set; } = null!;

  [Required(ErrorMessage = "{0} é obrigatório.")]
  [RegularExpression(@"^\d+$", ErrorMessage = "{0} precisa ser um número de agência válido")]
  [StringLength(4, MinimumLength = 3, ErrorMessage = "{0} precisa estar entre 3 e 4 caracteres.")]
  [DataMember(Name = "agency")]
  public string Agency { get; set; } = null!;
}

[DataContract]
public class CreatePixKeyDTO
{
  [Required(ErrorMessage = "{0} é obrigatório.")]
  [DataMember(Name = "key")]
  public KeyCreatePixKeySchema Key { get; set; } = null!;

  [Required(ErrorMessage = "{0} é obrigatório.")]
  [DataMember(Name = "user")]
  public UserCreatePixKeySchema User { get; set; } = null!;

  [Required(ErrorMessage = "{0} é obrigatório.")]
  [DataMember(Name = "account")]
  public AccountCreatePixKeySchema Account { get; set; } = null!;
}
