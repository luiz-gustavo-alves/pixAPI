using Microsoft.AspNetCore.Mvc;
using pixAPI.DTOs;
using pixAPI.Models;
using pixAPI.Services;

namespace pixAPI.Controllers;

[ApiController]
[Route("/concilliation")]
public class ConcilliationController(ConcilliationService concilliationService) : ControllerBase
{
  private readonly ConcilliationService _concilliationService = concilliationService;

  [HttpPost]
  public IActionResult ConcilliationRequest(ConcilliationRequestDTO dto)
  {
    PaymentProvider? bankData = (PaymentProvider?)HttpContext.Items["bankData"];
    _concilliationService.ConcilliationRequest(bankData, dto);
    return Ok($"Request for concilliation in progress");
  }

  [HttpPost("check")]
  public async Task<IActionResult> ConcilliationCheck(ConcilliationMessageServiceDTO dto)
  {
    PaymentProvider? bankData = (PaymentProvider?)HttpContext.Items["bankData"];
    ConcilliationOutputDTO outputDTO = await _concilliationService.ConcilliationOutput(bankData, dto);
    return CreatedAtAction(nameof(ConcilliationCheck), null, new
    {
      DatabaseToFile = outputDTO.DatabaseToFile,
      FileToDatabase = outputDTO.FileToDatabase,
      DifferentStatus = outputDTO.DifferentStatus,
    });
  }

  [HttpPost("finish")]
  public IActionResult ConcilliationFinish(ConcilliationMessageServiceDTO dto)
  {
    PaymentProvider? bankData = (PaymentProvider?)HttpContext.Items["bankData"];
    _concilliationService.ConcilliationFinish(bankData, dto);
    return NoContent();
  }
}