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
  public async Task<IActionResult> CreatePixKeyAsync(CreatePixKeyDTO dto)
  {
    Console.Write(dto);
    PaymentProvider bankData = (PaymentProvider?)HttpContext.Items["bankData"] ?? throw new UnauthorizedException("Acesso indevido.");
    await _pixKeyService.CreatePixKey(bankData, dto);
    return Created();
  }

  [HttpGet("{type}/{value}")]
  public IActionResult GetHealth(string type, string value)
  {
    KeyType matchValue = EnumHelper.MatchStringToKeyType(type);
    return Ok();
  }
}