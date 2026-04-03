using System;
using Microsoft.AspNetCore.Mvc;
using Nexus_api.Services;
using Nexus_api.Services.Interface;

namespace Nexus_api.Controllers;


[ApiController]
[Route("[controller]")]
public class AgentsController(AgentsServices agentServices,
IClienServices clientServices) : Controller
{

    [HttpGet("{message}")]
    [ProducesResponseType(typeof(string), 200)]
    public async Task<IActionResult> Get(string message, [FromQuery] string userId = "default")
    {
        var result = await agentServices.Chat(message, userId);
        return Ok(result);
    }
    [HttpGet]
    public async Task<IActionResult> GetCasesBySector()
    {
        var result = await clientServices.SizeCasesBySector();
        return Ok(result);
    }
}

