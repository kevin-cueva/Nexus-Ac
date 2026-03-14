using System;
using System.Collections.Generic;
using Pinecone;
using Nexus_api.Configuration;
using Microsoft.Extensions.Options;

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
    public async Task UpsertAsync(string id, float[] values, Dictionary<string, object> metadata)
    {
        var indexName = _config.Environment;
        if (string.IsNullOrWhiteSpace(indexName))
            throw new InvalidOperationException("Pinecone index name must be configured in PineconeSettings.Environment.");

        var index = await _client.GetIndex(indexName);
        var map = new MetadataMap();
        foreach (var (key, value) in metadata)
        {
            if (MetadataValue.TryCreate(value, out var metadataValue))
            {
                map[key] = metadataValue;
            }
        }

        var vector = new Vector
        {
            Id = id,
            Values = values,
            Metadata = map
        };

        await index.Upsert([vector], indexName);
    }

    public async Task<ScoredVector[]> QueryAsync(float[] vector, int topK)
    {
        var indexName = _config.Environment;
        if (string.IsNullOrWhiteSpace(indexName))
            throw new InvalidOperationException("Pinecone index name must be configured in PineconeSettings.Environment.");

        var index = await _client.GetIndex(indexName);
        return await index.Query(vector, (uint)topK);
    }
}
