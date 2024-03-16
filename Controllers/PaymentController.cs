using Microsoft.AspNetCore.Mvc;
using pixAPI.DTOs;
using pixAPI.Models;
using pixAPI.Services;

namespace pixAPI.Controllers;

[ApiController]
[Route("/payments")]
public class PaymentController(PaymentService paymentService) : ControllerBase
{
  private readonly PaymentService _paymentService = paymentService;

  [HttpPost]
  public async Task<IActionResult> MakePayment(MakePaymentDTO dto)
  {
    PaymentProvider? bankData = (PaymentProvider?)HttpContext.Items["bankData"];
    Payments createdPayment = await _paymentService.MakePayment(bankData, dto);
    return CreatedAtAction(nameof(MakePayment), null, new
    {
      Id = createdPayment.Id,
      Type = dto.Destiny.Key.Type,
      Value = dto.Destiny.Key.Value,
      Status = "PROCESSING",
      Amount = createdPayment.Amount,
      Description = createdPayment.Description,
      CreatedAt = createdPayment.CreatedAt,
    });
  }

  [HttpPatch("{paymentId}/{paymentStatus}")]
  public async Task<IActionResult> UpdatePaymentStatus(long paymentId, string paymentStatus)
  {
    Payments updatedPayment = await _paymentService.UpdatePaymentStatus(paymentId, paymentStatus);
    return Ok(new
    {
      Id = updatedPayment.Id,
      Status = paymentStatus,
      UpdatedAt = updatedPayment.UpdatedAt
    });
  }
}