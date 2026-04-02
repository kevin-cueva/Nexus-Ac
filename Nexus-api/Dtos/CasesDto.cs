using System;

namespace Nexus_api.Dtos;

public class CasesDto
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public DateTime Year { get; set; }
    public string? ClientId { get; set; }
    public string? Sector { get; set; }

}
