namespace pixAPI.DTOs;

public class PaymentMessageServiceDTO
{
  public required long Id { get; set; }
  public required string Status { get; set; } = null!;

  public required string Token { get; set; } = null!;

  public required MakePaymentDTO DTO { get; set; } = null!;
}