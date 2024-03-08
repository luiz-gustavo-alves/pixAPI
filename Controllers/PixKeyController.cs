using Microsoft.AspNetCore.Mvc;
using pixAPI.DTOs;
using pixAPI.Models;
using pixAPI.Helpers;
using pixAPI.Repositories;
using pixAPI.Services;
using pixAPI.Exceptions;

namespace pixAPI.Controllers;

[ApiController]
[Route("/keys")]
public class PixKeyController(PixKeyService pixKeyService) : ControllerBase
{
  private readonly PixKeyService _pixKeyService = pixKeyService;

  [HttpPost]
  public async Task<IActionResult> CreatePixKey(CreatePixKeyDTO dto)
  {
    PaymentProvider bankData = (PaymentProvider?)HttpContext.Items["bankData"] ?? throw new UnauthorizedException("Acesso indevido.");
    await _pixKeyService.CreatePixKey(bankData, dto);
    return CreatedAtAction("CreatePixKey", dto);
  }

  [HttpGet("{type}/{value}")]
  public IActionResult GetPixKey(string type, string value)
  {
    return Ok();
  }
}