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
  public async Task<IActionResult> ConcilliationCheck(ConcilliationCheckDTO dto)
  {
    PaymentProvider? bankData = (PaymentProvider?)HttpContext.Items["bankData"];
    ConcilliationOutputDTO outputDTO = await _concilliationService.ConcilliationCheck(bankData, dto);
    return CreatedAtAction(nameof(ConcilliationCheck), null, new
    {
      DatabaseToFile = outputDTO.DatabaseToFile,
      FileToDatabase = outputDTO.FileToDatabase,
      DifferentStatus = outputDTO.DifferentStatus,
    });
  }
}