using System;
using Nexus_api.Dtos;

namespace Nexus_api.Services.Interface;

public interface IClienServices
{
   /// <summary>
   /// Obtiene una lista de casos con detalles como 
   /// descripción, fecha de creación, nombre del cliente y sector asociado. 
   /// Utiliza el repositorio genérico para consultar la base de datos y proyectar los resultados en una lista de CasesDto. 
   /// La consulta incluye las relaciones necesarias para obtener la información del cliente y el sector. 
   /// El resultado se devuelve como una tarea asincrónica que contiene la lista de casos.
   /// </summary>
   /// <returns></returns>
   public Task<List<CasesDto>> AllCase();

}
