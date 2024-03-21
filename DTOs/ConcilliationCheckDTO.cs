using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace pixAPI.DTOs;

[DataContract]
public class ConcilliationCheckDTO
{
  [Required(ErrorMessage = "{0} é obrigatório.")]
  [DataMember(Name = "file")]
  public string File { get; set; } = null!;
}

[DataContract]
public class ConcilliationOutputDTO
{
  [DataMember(Name = "databaseToFile")]
  public ConcilliationFileContent[] DatabaseToFile { get; set; } = null!;

  [DataMember(Name = "fileToDatabase")]
  public ConcilliationFileContent[] FileToDatabase { get; set; } = null!;

  [DataMember(Name = "differentStatus")]
  public ConcilliationPaymentId[] DifferentStatus { get; set; } = null!;
}

public class ConcilliationFileContent
{
  [DataMember(Name = "id")]
  public long Id { get; set; }

  [DataMember(Name = "status")]
  public string Status { get; set; } = null!;
}

public class ConcilliationPaymentId
{
  [DataMember(Name = "id")]
  public long Id { get; set; }
}