using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace pixAPI.DTOs;

[DataContract(Name = "MakePaymentSchema")]
public class MakePaymentDTO
{
  [Required(ErrorMessage = "{0} é obrigatório.")]
  [DataMember(Name = "origin")]
  public OriginMakePaymentSchema Origin { get; set; } = null!;

  [Required(ErrorMessage = "{0} é obrigatório.")]
  [DataMember(Name = "destiny")]
  public DestinyMakePaymentSchema Destiny { get; set; } = null!;

  [Required(ErrorMessage = "{0} é obrigatório.")]
  [Range(1, int.MaxValue)]
  [DataMember(Name = "amount")]
  public int Amount { get; set; }

  [StringLength(256)]
  [DataMember(Name = "description")]
  public string? Description { get; set; }
}

public class OriginMakePaymentSchema
{
  [Required(ErrorMessage = "{0} é obrigatório.")]
  [DataMember(Name = "user")]
  public UserMakePaymentSchema User { get; set; } = null!;

  [Required(ErrorMessage = "{0} é obrigatório.")]
  [DataMember(Name = "account")]
  public AccountMakePaymentSchema Account { get; set; } = null!;
}

public class UserMakePaymentSchema
{
  [Required(ErrorMessage = "{0} é obrigatório.")]
  [RegularExpression(@"^\d+$", ErrorMessage = "{0} precisa ser um CPF válido")]
  [StringLength(11, MinimumLength = 11, ErrorMessage = "{0} precisa ter 11 dígitos.")]
  [DataMember(Name = "cpf")]
  public string CPF { get; set; } = null!;
}

public class AccountMakePaymentSchema
{
  [Required(ErrorMessage = "{0} é obrigatório.")]
  [RegularExpression(@"^\d+$", ErrorMessage = "{0} precisa ser um número de conta válido")]
  [StringLength(8, MinimumLength = 4, ErrorMessage = "{0} precisa ter entre 4 e 8 caracteres.")]
  [DataMember(Name = "number")]
  public string Number { get; set; } = null!;

  [Required(ErrorMessage = "{0} é obrigatório.")]
  [RegularExpression(@"^\d+$", ErrorMessage = "{0} precisa ser um número de agência válido")]
  [StringLength(4, MinimumLength = 3, ErrorMessage = "{0} precisa ter entre 3 e 4 caracteres.")]
  [DataMember(Name = "agency")]
  public string Agency { get; set; } = null!;
}

public class DestinyMakePaymentSchema
{
  [Required(ErrorMessage = "{0} é obrigatório.")]
  [DataMember(Name = "key")]
  public KeyMakePaymentSchema Key { get; set; } = null!;
}

public class KeyMakePaymentSchema
{
  [Required(ErrorMessage = "{0} é obrigatório.")]
  [DataMember(Name = "value")]
  public string Value { get; set; } = null!;

  [Required(ErrorMessage = "{0} é obrigatório.")]
  [RegularExpression(@"^(CPF|Email|Phone|Random)$", ErrorMessage = "{0} precisa ser: CPF, Email, Phone ou Random.")]
  [DataMember(Name = "type")]
  public string Type { get; set; } = null!;
}

