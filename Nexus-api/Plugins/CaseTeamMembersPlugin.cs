using Microsoft.SemanticKernel;
using Nexus_api.Dtos;
using Nexus_api.Services.Interface;

namespace Nexus_api.Plugins;
public class CaseTeamMembersPlugin(IServiceProvider serviceProvider)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    [KernelFunction("get_case_team_members")]
    public async Task<List<CaseTeamMemberDto>> GetCaseTeamMembersAsync(int caseId)
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IClienServices>();

        return await service.GetTeamMembersByCaseIdAsync(caseId);
    }
}
