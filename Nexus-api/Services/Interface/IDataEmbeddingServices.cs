using System;
using Nexus_api.Dtos;

namespace Nexus_api.Services.Interface;

public interface IDataEmbeddingServices
{
    /// <summary>
    /// Método para vectorizar PDF en Qdrant
    /// </summary>
    /// <param name="pdf"></param>
    /// <returns></returns>
    Task<bool> PdfEmbeddings( DataEmbeddingsDto.Pdf pdf);

}
