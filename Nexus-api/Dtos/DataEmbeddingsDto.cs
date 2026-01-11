using System;
using System.ComponentModel.DataAnnotations;

namespace Nexus_api.Dtos;

/// <summary>
/// DTO para las fuentes de conocimiento
/// Pdf, excel, word y paginas web
/// </summary>
public class DataEmbeddingsDto
{
    public record Pdf(
        /// <summary>
        /// El archivo PDF que se va a procesar.
        /// </summary>
        [Required] IFormFile File,
        Metadata Metadata
    );
    /// <summary>
    /// Metadatos usados para la fuente del conocimiento 
    /// para entrenar el modelo
    /// </summary>
    /// <param name="UseCaseId"></param>
    /// <param name="Departament"></param>
    /// <param name="Owner"></param>
    /// <param name="Classification"></param>
    public record Metadata(
        string UseCaseId,
        string Departament,
        string Owner,
        string Classification
    );
}
