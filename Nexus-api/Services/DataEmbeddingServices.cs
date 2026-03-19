using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Nexus_api.Dtos;
using Nexus_api.Services.Interface;
using Qdrant.Client;
using UglyToad.PdfPig;
using PointStruct = Qdrant.Client.Grpc.PointStruct;
using PointId = Qdrant.Client.Grpc.PointId;
using Microsoft.Extensions.AI;
using static Nexus_api.Dtos.DataEmbeddingsDto;

namespace Nexus_api.Services;

public class DataEmbeddingServices(QdrantClient qdrantClient, ITextEmbeddingGenerationService embeddingService) : IDataEmbeddingServices
{
    private readonly QdrantClient _qdrantClient = qdrantClient;
    private readonly ITextEmbeddingGenerationService _embeddingService = embeddingService;

    public async Task<bool> PdfEmbeddings(DataEmbeddingsDto.Pdf pdf)
    {
        string pdfText = "";
        await using var stream = pdf.File.OpenReadStream();
        var text = Utils.Utils.ExtractTextFromPdf(stream);
        IEnumerable<string> chunksText = Utils.Utils.SplitIntoChunks(text, 4);

        //var apiKey = Environment.GetEnvironmentVariable("Nexus:ApiKey") ?? throw new InvalidOperationException("API key not found in environment variables.");
        var model = "text-embedding-3-small";


        IList<ReadOnlyMemory<float>>? embeddings = await _embeddingService.GenerateEmbeddingsAsync([.. chunksText], null, CancellationToken.None);
        Metadata metadata = new(pdf.Metadata.UseCaseId, pdf.Metadata.Departament, pdf.Metadata.Owner, pdf.Metadata.Classification);

        await UpsertListChunksWithEmbeddings(embeddings, chunksText, metadata, "pdfs");
        return true;
    }

    private async Task UpsertListChunksWithEmbeddings(
        IList<ReadOnlyMemory<float>> vectores,
        IEnumerable<string> chunksText,
        Metadata metadata,
        string collectionName)
    {
        int index = 0;
        foreach (var vector in vectores)
        {
            await _qdrantClient.UpsertAsync(
               collectionName: collectionName,
               points:
               [
                   new PointStruct
                {
                    Id = new PointId { Uuid = Guid.NewGuid().ToString() },
                    Vectors = vector.ToArray(),
                    Payload =
                    {
                        ["text"] = chunksText.ElementAt(index),
                        ["useCaseId"] = metadata.UseCaseId,
                        ["departament"] = metadata.Departament,
                        ["owner"] = metadata.Owner,
                        ["classification"] = metadata.Classification
                    }
                }
               ]
           );
            index++;
        }
    }
}
