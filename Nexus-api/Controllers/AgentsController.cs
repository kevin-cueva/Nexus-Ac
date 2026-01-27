using System;
using Microsoft.AspNetCore.Mvc;
using Nexus_api.Services;

namespace Nexus_api.Controllers;


[ApiController]
[Route("[controller]")]
public class AgentsController(AgentsServices agentServices) : Controller
{
    [HttpGet("{message}")]
    public async Task<IActionResult> Get(string message, [FromQuery] string userId = "default")
    {
        var result = await agentServices.Chat(message, userId);
        return Ok(result);
    }
}

