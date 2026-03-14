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
    Task<bool> PdfEmbeddingsQdrant( DataEmbeddingsDto.Pdf pdf);
    /// <summary>
    /// Método para vectorizar PDF en Pinecone
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    Task<bool> TextEmbeddingsPinecone(DataEmbeddingsDto.Pdf text);

}
