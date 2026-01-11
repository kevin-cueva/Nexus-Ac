using System;
using Microsoft.AspNetCore.Mvc;
using Nexus_api.Dtos;

namespace Nexus_api.Controllers;


[ApiController]
[Route("[controller]")]
public class DataEmbeddingControllers: Controller
{
    [HttpPost("pdf")]
    public async Task<IActionResult> PostPdf([FromForm] DataEmbeddingsDto.Pdf pdf)
    {
        return Ok();
    }
}
