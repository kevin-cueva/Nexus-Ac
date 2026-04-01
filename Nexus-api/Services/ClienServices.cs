using System;
using Microsoft.EntityFrameworkCore;
using Nexus_api.Domain.Entities;
using Nexus_api.Dtos;
using Nexus_api.Infrastructure.Repository;
using Nexus_api.Services.Interface;

namespace Nexus_api.Services;

public class ClienServices(
    IGenericRepository<Case> caseRepository) : IClienServices
{
    private readonly IGenericRepository<Case> _caseRepository = caseRepository;

    /// <summary>
    /// Obtiene una lista de casos con detalles como 
    /// descripción, fecha de creación, nombre del cliente y sector asociado.
    /// </summary>
    /// <returns></returns>
    public async Task<List<CasesDto>> AllCase()
    {
        var cases = await _caseRepository.Consultar()
            .Include(c => c.Sector)
            .Include(c => c.Client)
            .Select(c => new CasesDto
            {
                Id = c.Id,
                Description = c.Description,
                Year = c.Year,
                ClientId = c.Client.Name,
                Sector = c.Sector.Name
            })
            .ToListAsync();
        return cases;
        
    }
}
