using System.ComponentModel.DataAnnotations.Schema;

namespace pixAPI.Models;

public class BaseEntity
{
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public DateTime? CreatedAt { get; set; }

  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public DateTime? UpdatedAt { get; set; }
}