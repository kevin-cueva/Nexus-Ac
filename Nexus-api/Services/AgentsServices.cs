using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Nexus_api.Services;

public class AgentsServices(Kernel kernel)
{

   /// <summary>
        /// En Semantic Kernel (SK), ese código configura cómo el kernel debe decidir 
        /// automáticamente si ejecuta o no una Function (Skill) de tu aplicación cuando 
        /// el modelo de IA lo considere necesario.
        /// </summary>
        /// <returns></returns>
        private static readonly OpenAIPromptExecutionSettings settings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            ChatSystemPrompt = """
             Instrucciones obligatorias: responde siempre en español; 
             si el usuario saluda, comienza con un saludo cordial; 
             busca primero en tu memoria o base de conocimiento y, 
             si hay una respuesta completa y verificada, 
             úsala tal cual; si no existe, usa los plugins adecuados 
             y si estos devuelven una respuesta completa, 
             preséntala sin cambios; 
             si ni memoria ni plugins contienen la respuesta, 
             no generes una por tu cuenta; 
             sigue siempre el orden Memoria->Plugins y nunca combines fuentes.
             Instrucciones obligatorias: responde siempre en español; 
             si el usuario saluda, comienza con un saludo cordial; 
             busca primero en tu memoria o base de conocimiento y, 
             si hay una respuesta completa y verificada, 
             úsala tal cual; si no existe, usa los plugins adecuados 
             y si estos devuelven una respuesta completa, 
             preséntala sin cambios; 
             si ni memoria ni plugins contienen la respuesta, 
             no generes una por tu cuenta; 
             sigue siempre el orden Memoria->Plugins y nunca combines fuentes.
            """
        };
        /// <summary>
        /// Cada vez que ejecutes una función o prompt en el Kernel y uses arguments, Semantic Kernel 
        /// aplicará automáticamente la configuración OpenAIPromptExecutionSettings que definiste.
        /// </summary>
        private static readonly KernelArguments arguments = new (settings);
        public async Task<string> Chat(string prompt)
        {
            var result = await kernel.InvokePromptAsync(prompt, arguments);
            return result.GetValue<string>() ?? string.Empty;
        }
}

