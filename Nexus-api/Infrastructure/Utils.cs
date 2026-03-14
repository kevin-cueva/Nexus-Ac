using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UglyToad.PdfPig;

namespace Nexus_api.Infrastructure;

public static class Utils
{
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(30) };

    /// <summary>
    /// Extrae texto de un PDF usando PdfPig.
    /// </summary>
    public static string ExtractTextFromPdf(Stream pdfStream)
    {
        if (pdfStream == null) throw new ArgumentNullException(nameof(pdfStream));

        using var document = PdfDocument.Open(pdfStream);
        var sb = new StringBuilder();
        foreach (var page in document.GetPages())
        {
            sb.AppendLine(page.Text);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Divide un texto en hasta <paramref name="chunkCount"/> partes lo más balanceadas posible.
    /// </summary>
    public static IEnumerable<string> SplitIntoChunks(string text, int chunkCount = 4)
    {
        if (string.IsNullOrWhiteSpace(text))
            yield break;

        var tokens = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0)
            yield break;

        var targetSize = (int)Math.Ceiling(tokens.Length / (double)chunkCount);
        for (var i = 0; i < chunkCount; i++)
        {
            var chunk = tokens.Skip(i * targetSize).Take(targetSize);
            if (!chunk.Any())
                yield break;

            yield return string.Join(' ', chunk);
        }
    }

    /// <summary>
    /// Llama a la API de OpenAI embeddings y devuelve el vector.
    /// </summary>
    public static async Task<float[]> CreateOpenAiEmbeddingAsync(string apiKey, string model, string input)
    {
        if (string.IsNullOrWhiteSpace(apiKey)) throw new ArgumentNullException(nameof(apiKey));
        if (string.IsNullOrWhiteSpace(model)) throw new ArgumentNullException(nameof(model));
        if (input == null) throw new ArgumentNullException(nameof(input));

        var request = new
        {
            model,
            input
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/embeddings")
        {
            Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        using var response = await HttpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        using Stream stream = await response.Content.ReadAsStreamAsync() ?? throw new InvalidOperationException("OpenAI response did not contain a response stream.");

        using var doc = await JsonDocument.ParseAsync(stream) ?? throw new InvalidOperationException("OpenAI response parsing failed.");
        var embeddingJson = doc.RootElement
            .GetProperty("data")[0]
            .GetProperty("embedding");

        var values = new float[embeddingJson.GetArrayLength()];
        for (var i = 0; i < values.Length; i++)
        {
            values[i] = embeddingJson[i].GetSingle();
        }

        return values;
    }
}
