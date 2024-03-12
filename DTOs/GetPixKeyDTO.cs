using System.Runtime.Serialization;

namespace pixAPI.DTOs;

[DataContract(Name = "PixKeyInformation")]
public class GetPixKeyDTO
{
  [DataMember(Name = "key")]
  public KeyGetPixKeySchema Key { get; set; } = null!;

  [DataMember(Name = "user")]
  public UserGetPixKeySchema User { get; set; } = null!;

  [DataMember(Name = "account")]
  public AccountGetPixKeySchema Account { get; set; } = null!;
}

public class KeyGetPixKeySchema
{
  [DataMember(Name = "value")]
  public string Value { get; set; } = null!;

  [DataMember(Name = "type")]
  public string Type { get; set; } = null!;
}

public class UserGetPixKeySchema
{
  [DataMember(Name = "name")]
  public string Name { get; set; } = null!;

  [DataMember(Name = "maskedCpf")]
  public string MaskedCpf { get; set; } = null!;
}

public class AccountGetPixKeySchema
{
  [DataMember(Name = "number")]
  public string Number { get; set; } = null!;

  [DataMember(Name = "agency")]
  public string Agency { get; set; } = null!;

  [DataMember(Name = "bankName")]
  public string BankName { get; set; } = null!;

  [DataMember(Name = "bankId")]
  public string BankId { get; set; } = null!;
}
