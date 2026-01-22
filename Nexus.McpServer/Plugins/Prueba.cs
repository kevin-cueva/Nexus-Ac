using System;
using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Nexus.McpServer.Plugins;

[McpServerToolType]
public class Prueba
{
    [McpServerTool]
    [Description("""Para ver el saludo del agente""")]
    public static string Saludar()
    {
        return """
        Hola, soy un agente que te ayudarte a
        encontrar los casos de uso que se hayan 
        realizado en Song, con el fin de encontrar procesos similares,
        particioantes, tecnologias uy detalles de los anteriores proyectos
        realizados
        """;
    }
}
