using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace pixAPI.DTOs;

[DataContract]
public class ConcilliationRequestDTO
{
  [Required(ErrorMessage = "{0} é obrigatório.")]
  [DisplayFormat(DataFormatString = "{yyyy-MM-dd}")]
  [DataMember(Name = "date")]
  public required DateTime Date { get; set; }

  [Required(ErrorMessage = "{0} é obrigatório.")]
  [DataMember(Name = "file")]
  public string File { get; set; } = null!;

  [Required(ErrorMessage = "{0} é obrigatório.")]
  [DataMember(Name = "postback")]
  public string Postback { get; set; } = null!;
}

[DataContract]
public class ConcilliationMessageServiceDTO
{
  public required string Token { get; set; } = null!;
  public required DateTime Date { get; set; }
  public required string PSPfile { get; set; } = null!;
  public required string Postback { get; set; } = null!;
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

public class ConcilliationListOutput
{
  [DataMember(Name = "databaseToFile")]
  public List<ConcilliationFileContent> DatabaseToFile { get; set; } = null!;

  [DataMember(Name = "fileToDatabase")]
  public List<ConcilliationFileContent> FileToDatabase { get; set; } = null!;

  [DataMember(Name = "differentStatus")]
  public List<ConcilliationPaymentId> DifferentStatus { get; set; } = null!;
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