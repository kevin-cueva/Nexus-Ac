using System;
using Microsoft.AspNetCore.Mvc;
using Nexus_api.Dtos;
using Nexus_api.Services.Interface;

namespace Nexus_api.Controllers;


[ApiController]
[Route("[controller]")]
public class DataEmbeddingControllers(IDataEmbeddingServices dataEmbeddingServices): Controller
{
    private readonly IDataEmbeddingServices _dataEmbeddingServices = dataEmbeddingServices;
    
    /// <summary>
    /// Método para el embedding de PDF
    /// </summary>
    /// <param name="pdf"></param>
    /// <returns>
    /// <c>bool</c> indicando si se realizo con exito el embedding del PDF
    /// </returns>
    /// <returns></returns>
    [HttpPost("pdf")]
    [ProducesResponseType(typeof(string), 200)]
    public async Task<IActionResult> PostPdf([FromForm] DataEmbeddingsDto.Pdf pdf)
    {
        var result = await _dataEmbeddingServices.PdfEmbeddings(pdf);
        if (!result) return BadRequest();
        return Ok("PDF Embedding realizado con exito");
    }
}
