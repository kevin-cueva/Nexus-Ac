using System;
using Microsoft.AspNetCore.Mvc;
using Nexus_api.Services;

namespace Nexus_api.Controllers;


[ApiController]
[Route("[controller]")]
public class AgentsController(AgentsServices agentServices) : Controller
{
    public async Task<IActionResult> Get(string message)
    {
        var result = await agentServices.Chat(message);
        return Ok(result);
    }
}

