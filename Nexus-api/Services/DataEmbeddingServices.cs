using System;
using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.KernelMemory;
using Nexus_api.Dtos;
using Nexus_api.Services.Interface;

namespace Nexus_api.Services;

public class DataEmbeddingServices(IKernelMemory kernelMemory) : IDataEmbeddingServices
{
    private readonly IKernelMemory _kernelMemory = kernelMemory;

    /// <summary>
    /// Método para el embedding de PDF
    /// </summary>
    /// <param name="pdf"></param>
    /// <returns></returns>
    public async Task<bool> PdfEmbeddings(DataEmbeddingsDto.Pdf pdf)
    {
        var tags = new TagCollection
        {
            { nameof(DataEmbeddingsDto.Metadata.UseCaseId), pdf.Metadata.UseCaseId },
            { nameof(DataEmbeddingsDto.Metadata.Departament), pdf.Metadata.Departament },
            { nameof(DataEmbeddingsDto.Metadata.Owner), pdf.Metadata.Owner },
            { nameof(DataEmbeddingsDto.Metadata.Classification), pdf.Metadata.Classification }
        };

        // ✅ Abrir el stream del archivo subido
        await using Stream stream = pdf.File.OpenReadStream();

        // ✅ ¡IMPORTANTE! Usar la sobrecarga que acepta STREAM
        var embeddingPdf = await _kernelMemory.ImportDocumentAsync(
                content: stream,                   // Stream del PDF subido
                fileName: pdf.File.FileName,      // Nombre original del archivo
                documentId: pdf.DocumentId,       // Ej: "fb-02"
                tags: tags                       // Metadatos para el payload

            );

        return embeddingPdf is not null;
    }
}
