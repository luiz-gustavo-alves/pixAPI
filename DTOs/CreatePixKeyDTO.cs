using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace pixAPI.DTOs;

[DataContract]
public class Key
{
  [Required(ErrorMessage = "{0} é obrigatório.")]
  [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "{0} precisa ser um número válido.")]
  [Range(0, 9999999999999999.99, ErrorMessage = "{0} precisa ser um valor positivo.")]
  [DataMember(Name = "value")]
  public string Value { get; set; } = null!;

  [Required(ErrorMessage = "{0} é obrigatório.")]
  [RegularExpression(@"^(CPF|Email|Phone|Random)$", ErrorMessage = "{0} precisa ser: CPF, Email, Phone ou Random.")]
  [DataMember(Name = "type")]
  public string Type { get; set; } = null!;
}

[DataContract]
public class User
{
  [Required(ErrorMessage = "{0} é obrigatório.")]
  [StringLength(11, MinimumLength = 11, ErrorMessage = "{0} precisa ter 11 dígitos.")]
  [DataMember(Name = "cpf")]
  public string CPF { get; set; } = null!;
}

[DataContract]
public class Account
{
  [Required(ErrorMessage = "{0} é obrigatório.")]
  [StringLength(8, MinimumLength = 4, ErrorMessage = "{0} precisa estar entre 4 e 8 caracteres.")]
  [DataMember(Name = "number")]
  public string Number { get; set; } = null!;

  [Required(ErrorMessage = "{0} é obrigatório.")]
  [StringLength(4, MinimumLength = 3, ErrorMessage = "{0} precisa estar entre 3 e 4 caracteres.")]
  [DataMember(Name = "agency")]
  public string Agency { get; set; } = null!;
}

[DataContract]
public class CreatePixKeyDTO
{
  [Required(ErrorMessage = "{0} é obrigatório.")]
  [DataMember(Name = "key")]
  public Key Key { get; set; } = null!;

  [Required(ErrorMessage = "{0} é obrigatório.")]
  [DataMember(Name = "user")]
  public User User { get; set; } = null!;

  [Required(ErrorMessage = "{0} é obrigatório.")]
  [DataMember(Name = "account")]
  public Account Account { get; set; } = null!;
}
