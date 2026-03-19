namespace Nexus_api.Dtos;

/// <summary>
/// DTO para encapsular la respuesta del chat, indicando si se encontró una respuesta en la memoria semántica
///  y el contenido de la respuesta.
/// </summary>
public record ChatResponseDto(bool FoundInSemanticMemory, string ResponseContent);