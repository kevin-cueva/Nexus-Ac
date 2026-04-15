using Microsoft.SemanticKernel;
using Nexus_api.Dtos;
using Nexus_api.Services.Interface;

namespace Nexus_api.Plugins;
public class SectorsPlugin(IServiceProvider serviceProvider)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    [KernelFunction("get_sectors")]
    public async Task<List<SizeCasesSectorDto>> GetNamesSectorsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IClienServices>();

        return await service.SizeCasesBySector();
    }

    [KernelFunction("get_cases_by_sector")]
    public async Task<List<CasesBySectorDto>> GetCasesBySectorAsync(string sectorId)
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IClienServices>();

        return await service.CasesBySector(sectorId);
    }
}