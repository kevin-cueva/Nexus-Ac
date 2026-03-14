using System;
using Pinecone;

namespace Nexus_api.Infrastructure.Pinecone;

// Infrastructure/Pinecone/PineconeClientFactory.cs
using Nexus_api.Configuration;

public static class PineconeClientFactory
{
    /// <summary>
    /// Crea una instancia de PineconeClient con la configuración
    /// provista por <paramref name="config"/>.
    /// </summary>
    /// <param name="config">Configuración de Pinecone</param>
    /// <returns>PineconeClient</returns>
    /// <exception cref="InvalidOperationException">Si la configuración de Pinecone es nula</exception>
    public static PineconeClient Create(PineconeSettings config)
    {
        if (string.IsNullOrEmpty(config.ApiKey))
            throw new InvalidOperationException("Pinecone API key is missing.");

        return new PineconeClient(config.ApiKey);
    }
}
