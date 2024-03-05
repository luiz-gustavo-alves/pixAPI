using Microsoft.AspNetCore.Mvc;
using pixAPI.Services;

namespace pixAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController(HealthService healthService) : ControllerBase
{
  private readonly HealthService _healthService = healthService;

  [HttpGet]
  public IActionResult GetHealth() 
  {
    string healthMessage = _healthService.GetHealthMessage();
    return Ok(healthMessage);
  }
}