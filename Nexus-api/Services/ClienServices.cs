using System;
using System.Drawing;
using Microsoft.EntityFrameworkCore;
using Nexus_api.Domain.Entities;
using Nexus_api.Dtos;
using Nexus_api.Infrastructure.Repository;
using Nexus_api.Services.Interface;

namespace Nexus_api.Services;

public class ClienServices(
    IGenericRepository<Case> caseRepository,
    IGenericRepository<Client> clientRepository,
    IGenericRepository<Sector> sectorRepository,
    IGenericRepository<TeamMember> teamMemberRepository
) : IClienServices
{
    private readonly IGenericRepository<Case> _caseRepository = caseRepository;
    private readonly IGenericRepository<Client> _clientRepository = clientRepository;
    private readonly IGenericRepository<Sector> _sectorRepository = sectorRepository;
    private readonly IGenericRepository<TeamMember> _teamMemberRepository = teamMemberRepository;

    /// <summary>
    /// Obtiene una lista de casos con detalles como 
    /// descripción, fecha de creación, nombre del cliente y sector asociado.
    /// </summary>
    /// <returns></returns>
    public async Task<List<CasesDto>> AllCases()
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

    public async Task<List<SizeCasesSectorDto>> SizeCasesBySector()
    {
        var casesBySector = await _caseRepository.Consultar()
            .Include(c => c.Sector)
            .GroupBy(c => new { c.Sector.Id, c.Sector.Name })
            .Select(g => new SizeCasesSectorDto
            {
                IdSector = g.Key.Id.ToString(),
                Sector = g.Key.Name,
                CasesCount = g.Count()
            })
            .ToListAsync();
        return casesBySector;
    }

    public async Task<List<CasesBySectorDto>> CasesBySector(string sectorId)
    {
        var cases = await _caseRepository.Consultar()
            .Include(c => c.Sector)
            .Where(c => c.Sector.Id.ToString() == sectorId)
            .Select(c => new CasesBySectorDto
            {
                IdCase = c.Id,
                NameCase = c.Name,
                Description = c.Description
            })
            .ToListAsync();
        return cases;
    }

    public async Task<List<CaseTeamMemberDto>> GetTeamMembersByCaseIdAsync(int caseId)
    {
        var teamMembers = await _teamMemberRepository.Consultar()
            .Include(tm => tm.User)
            .Include(tm => tm.Role)
            .Where(tm => tm.IdCase == caseId)
            .Select(tm => new CaseTeamMemberDto
            {
                UserId = tm.User.Id,
                UserName = tm.User.Username,
                RolTeamId = tm.Role.Id,
                RolTeamName = tm.Role.Name
            })
            .ToListAsync();
        return teamMembers;
    }
}
