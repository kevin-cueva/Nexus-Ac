using System;
using System.Collections.Generic;
using Pinecone;
using Nexus_api.Configuration;
using Microsoft.Extensions.Options;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Nexus_api.Infrastructure.Pinecone;

public class PineconeVectorDbRepository(
    IOptions<PineconeSettings> config,
    PineconeClient client)
{
    private readonly PineconeClient _client = client;
    private readonly PineconeSettings _config = config.Value;

    /// <summary>
    /// Upsert de un vector en el índice configurado en PineconeSettings.Environment.
    /// </summary>
    public async Task UpsertAsync(string id, float[] values, Metadata metadata)
    {
        var indexName = _config.Environment;

        if (string.IsNullOrWhiteSpace(indexName))
            throw new InvalidOperationException("Pinecone index name must be configured in PineconeSettings.Environment.");

        var index = _client.Index(indexName);
        Vector vector = new()
        {
            Id = id,
            Values = values,
            Metadata = metadata
        };
        UpsertRequest upsertRequest = new()
        {
            Namespace = "default",
            Vectors = [vector]
        };
        await index.UpsertAsync(
            upsertRequest
        );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="vector"></param>
    /// <param name="topK"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<QueryResponse> QueryAsync(float[] vector, int topK)
    {

        var indexName = _config.Environment;
        if (string.IsNullOrWhiteSpace(indexName))
            throw new InvalidOperationException("Pinecone index name must be configured in PineconeSettings.Environment.");

        var index = _client.Index(indexName);
        QueryResponse queryResponse = await index.QueryAsync(new QueryRequest
        {
            Vector = vector,
            TopK = 2,
            IncludeMetadata = true,
            Namespace = "default"
        });
        if (queryResponse == null || queryResponse.Matches == null)
            return new QueryResponse();

        return queryResponse;
    }
}
