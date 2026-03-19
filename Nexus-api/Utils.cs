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

namespace Nexus_api.Utils;

public static class Utils
{
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
}