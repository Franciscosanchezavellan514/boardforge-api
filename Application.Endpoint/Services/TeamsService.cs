using System.ComponentModel.DataAnnotations;
using DevStack.Application.BoardForge.DTOs.Request;
using DevStack.Application.BoardForge.DTOs.Response;
using DevStack.Application.BoardForge.Interfaces;
using DevStack.Domain.BoardForge.Entities;
using DevStack.Domain.BoardForge.Exceptions;
using DevStack.Domain.BoardForge.Interfaces.Repositories;
using DevStack.Domain.BoardForge.Specifications;

namespace DevStack.Application.BoardForge.Services;

public class TeamsService(IUnitOfWork unitOfWork) : ITeamsService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<TeamMembershipResponse> AddMemberAsync(BaseRequest<AddTeamMemberRequest> request)
    {
        if (request.ObjectId == null) throw new ArgumentException("ObjectId must be provided");
        if (!TeamMembershipRole.AllRoles.Contains(request.Data.Role)) throw new ArgumentException("Invalid role");

        TeamMembership membership = new()
        {
            TeamId = request.ObjectId.Value,
            UserId = request.Data.UserId,
            Role = request.Data.Role,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.UserId,
            IsActive = true,
        };

        await _unitOfWork.TeamMemberships.AddAsync(membership);
        await _unitOfWork.SaveChangesAsync();

        return new TeamMembershipResponse(
            membership.UserId,
            membership.TeamId,
            membership.Role,
            membership.CreatedAt,
            membership.CreatedBy
        );
    }

    public async Task<TeamResponse> CreateAsync(BaseRequest<CreateTeamRequest> request)
    {
        Team newTeam = new()
        {
            Name = request.Data.Name,
            Description = request.Data.Description,
            CreatedBy = request.UserId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        await _unitOfWork.Teams.AddAsync(newTeam);

        TeamMembership membership = new()
        {
            Team = newTeam,
            UserId = request.UserId,
            Role = TeamMembershipRole.Owner,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.UserId,
            IsActive = true
        };
        await _unitOfWork.TeamMemberships.AddAsync(membership);
        await _unitOfWork.SaveChangesAsync();

        return new TeamResponse(newTeam.Id, newTeam.Name, newTeam.Description, newTeam.TeamMemberships.Count);
    }

    public async Task<TeamResponse> GetByIdAsync(int id)
    {
        Team? team = await _unitOfWork.Teams.GetByIdAsync(id);
        if (team is null) throw new EntityNotFoundException($"Team with ID {id} not found");

        TeamResponse response = new(
            team.Id,
            team.Name,
            team.Description,
            team.TeamMemberships.Count
        );
        return response;
    }

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

    public async Task<TeamResponse> UpdateAsync(BaseRequest<UpdateTeamRequest> request)
    {
        if (request.ObjectId == null) throw new ArgumentException("ObjectId must be provided");

        Team? dbTeam = await _unitOfWork.Teams.GetByIdAsync(request.ObjectId.Value);
        if (dbTeam == null) throw new EntityNotFoundException($"Team with ID {request.ObjectId} not found");

        dbTeam.Name = request.Data.Name;
        dbTeam.Description = request.Data.Description;
        dbTeam.UpdatedAt = DateTime.UtcNow;
        dbTeam.UpdatedBy = request.UserId;

        await _unitOfWork.SaveChangesAsync();

        return new TeamResponse(dbTeam.Id, dbTeam.Name, dbTeam.Description, dbTeam.TeamMemberships.Count);
    }

    public async Task<TeamResponse> SoftDeleteAsync(BaseRequest request)
    {
        var team = await _unitOfWork.Teams.GetByIdAsync(request.ObjectId);
        if (team is null) throw new EntityNotFoundException($"Team with ID {request.ObjectId} not found");

        team.IsActive = false;
        team.DeletedAt = DateTime.UtcNow;
        team.DeletedBy = request.UserId;

        await _unitOfWork.SaveChangesAsync();

        return new TeamResponse(team.Id, team.Name, team.Description, team.TeamMemberships.Count);
    }
}