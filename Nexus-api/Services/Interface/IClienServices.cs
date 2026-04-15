using System;
using Nexus_api.Dtos;

namespace Nexus_api.Services.Interface;

public interface IClienServices
{
    public Task<List<CaseTeamMemberDto>> GetTeamMembersByCaseIdAsync(int caseId);
   /// <summary>
   /// Obtiene una lista de casos con detalles como 
   /// descripción, fecha de creación, nombre del cliente y sector asociado. 
   /// Utiliza el repositorio genérico para consultar la base de datos y proyectar los resultados en una lista de CasesDto. 
   /// La consulta incluye las relaciones necesarias para obtener la información del cliente y el sector. 
   /// El resultado se devuelve como una tarea asincrónica que contiene la lista de casos.
   /// </summary>
   /// <returns></returns>
   public Task<List<CasesDto>> AllCases();
   public Task<List<SizeCasesSectorDto>> SizeCasesBySector();
   /// <summary>
   /// Obtiene una lista de casos filtrados por sector, proporcionando detalles como descripción, 
   /// y nombre del cliente.
   /// </summary>
   /// <param name="sectorId"></param>
   /// <returns></returns>
   public Task<List<CasesBySectorDto>> CasesBySector(string sectorId);

}
