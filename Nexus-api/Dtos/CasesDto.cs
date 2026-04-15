using System;
using System.Data.Common;

namespace Nexus_api.Dtos;

public class CasesDto
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public DateTime Year { get; set; }
    public string? ClientId { get; set; }
    public string? Sector { get; set; }

}

public class SizeCasesSectorDto
{
    public string? IdSector { get; set; }
    public string? Sector { get; set; }
    public int CasesCount { get; set; }
}
public class CasesBySectorDto
{
    public int IdCase { get; set; }
    public string? NameCase { get; set; }
    public string? Description { get; set; }
}

public class CaseTeamMemberDto
{
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public int RolTeamId { get; set; }
    public string? RolTeamName { get; set; }
}
