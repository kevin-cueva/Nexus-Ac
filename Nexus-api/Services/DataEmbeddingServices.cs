using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.KernelMemory;
using Microsoft.Extensions.Options;
using Nexus_api.Configuration;
using Nexus_api.Dtos;
using Nexus_api.Infrastructure;
using Nexus_api.Infrastructure.Pinecone;
using Nexus_api.Services.Interface;
using Pinecone;

namespace Nexus_api.Services;

public class DataEmbeddingServices(
    IKernelMemory kernelMemory,
    PineconeVectorDbRepository pineconeRepository,
    IOptions<AppSettings> options)
    : IDataEmbeddingServices
{
    private readonly IKernelMemory _kernelMemory = kernelMemory;
    private readonly PineconeVectorDbRepository _pineconeRepository = pineconeRepository;
    private readonly AppSettings _appSettings = options.Value;

    private const string EmbeddingModel = "text-embedding-3-small";

    /// <summary>
    /// Método para el embedding de PDF en Qdrant (ya existente)
    /// </summary>
    public async Task<bool> PdfEmbeddingsQdrant(DataEmbeddingsDto.Pdf pdf)
    {
        var tags = new TagCollection
        {
            { nameof(DataEmbeddingsDto.Metadata.UseCaseId), pdf.Metadata.UseCaseId },
            { nameof(DataEmbeddingsDto.Metadata.Departament), pdf.Metadata.Departament },
            { nameof(DataEmbeddingsDto.Metadata.Owner), pdf.Metadata.Owner },
            { nameof(DataEmbeddingsDto.Metadata.Classification), pdf.Metadata.Classification }
        };

        await using Stream stream = pdf.File.OpenReadStream();

        var embeddingPdf = await _kernelMemory.ImportDocumentAsync(
            content: stream,
            fileName: pdf.File.FileName,
            documentId: pdf.DocumentId,
            tags: tags);

        return embeddingPdf is not null;
    }

    /// <summary>
    /// Genera embeddings usando OpenAI y los guarda en Pinecone.
    /// </summary>
    public async Task<bool> TextEmbeddingsPinecone(DataEmbeddingsDto.Pdf pdf)
    {
        await using var stream = pdf.File.OpenReadStream();
        var text = Nexus_api.Infrastructure.Utils.ExtractTextFromPdf(stream);
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var chunks = Nexus_api.Infrastructure.Utils.SplitIntoChunks(text, 4);
        var apiKey = _appSettings.Nexus.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Nexus API key is required to generate embeddings.");

        Metadata metadata = new()
        {
            ["DocumentId"] = pdf.DocumentId,
            ["FileName"] = pdf.File.FileName,
            ["UseCaseId"] = pdf.Metadata.UseCaseId,
            ["Departament"] = pdf.Metadata.Departament,
            ["Owner"] = pdf.Metadata.Owner,
            ["Classification"] = pdf.Metadata.Classification
        };

        var index = 0;
        foreach (var chunk in chunks)
        {
            var embedding = await Utils.CreateOpenAiEmbeddingAsync(apiKey, EmbeddingModel, chunk);
            var vectorId = $"{pdf.DocumentId}_{index++}";
            metadata["ChunkText"] = chunk;
            await _pineconeRepository.UpsertAsync(vectorId, embedding, metadata);
        }

        return true;
    }
}
