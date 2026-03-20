using System;
using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Nexus.McpServer.Plugins;

[McpServerToolType]
public class Prueba
{
    /// <summary>
    /// Herramienta para el saludo del agente, ademas de una breve 
    /// descripción de lo que hace y de lo que te puede ayudar y como lo hace
    /// </summary>
    /// <returns></returns>
    [McpServerTool]
    [Description(
    """
    Primer saludo del agente, ademas de una breve 
    descripción de lo que hace, de lo que te puede ayudar y como lo hace
    """)]
    public static string RespondeSaludoHola([Description("Nombre del usuario, si es que lo proporciona")] string? nombreUsuario)
    {
        return $"""
        Hola, {nombreUsuario}! Soy Nexus, un agente que te ayudará a
        encontrar los casos de uso que se hayan 
        realizado en Song, con el fin de encontrar procesos similares,
        particioantes, tecnologias uy detalles de los anteriores proyectos
        realizados
        """;
    }
    [McpServerTool]
    [Description("""Para ver los Proyectos, casos de uso o repositorios echos""")]
    public static string ConsultarProyectosCasosDeUsoRepositorios()
    {
        return """
        Los proyectos son:
        EcoTrack Analytics: Platform:es una plataforma web integral diseñada para ayudar 
        a empresas medianas y grandes a monitorear, analizar y reportar sus métricas de 
        sostenibilidad ambiental
        """;
    }
}
