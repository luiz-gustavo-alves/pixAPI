using Microsoft.AspNetCore.Mvc;
using pixAPI.DTOs;
using pixAPI.Models;
using pixAPI.Services;

namespace pixAPI.Controllers;

[ApiController]
[Route("/keys")]
public class PixKeyController(PixKeyService pixKeyService) : ControllerBase
{
  private readonly PixKeyService _pixKeyService = pixKeyService;

  [HttpPost]
  public async Task<IActionResult> CreatePixKey(CreatePixKeyDTO dto)
  {
    PaymentProvider? bankData = (PaymentProvider?)HttpContext.Items["bankData"];
    PixKey createdPixKey = await _pixKeyService.CreatePixKey(bankData, dto);
    return CreatedAtAction(nameof(CreatePixKey), null, new { 
      Id = createdPixKey.Id, 
      Type = dto.Key.Type, 
      Value = dto.Key.Value, 
      CreatedAt = createdPixKey.CreatedAt
    });
  }

  [HttpGet("{type}/{value}")]
  public async Task<IActionResult> GetPixKey(string type, string value)
  {
    GetPixKeyDTO pixKeyDetails = await _pixKeyService.GetPixKey(type, value);
    return Ok(pixKeyDetails);
  }
}