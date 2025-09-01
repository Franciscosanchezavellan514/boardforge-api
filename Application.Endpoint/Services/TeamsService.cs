using DevStack.Application.Endpoint.DTOs.Response;
using DevStack.Application.Endpoint.Interfaces;
using DevStack.Domain.BoardForge.Interfaces.Repositories;
using DevStack.Domain.BoardForge.Specifications;

namespace DevStack.Application.BoardForge.Services;

public class TeamsService(IUnitOfWork unitOfWork) : ITeamsService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<IEnumerable<TeamResponse>> ListMyTeamsAsync(int userId)
    {
        var teams = await _unitOfWork.Teams.ListAsync(new TeamsByUserIdSpecification(userId));
        IEnumerable<TeamResponse> teamResponses = teams.Select(t =>
            new TeamResponse(
                t.Id,
                t.Name,
                t.Description,
                t.TeamMemberships.Count
            )
        );
        return teamResponses;
    }
}