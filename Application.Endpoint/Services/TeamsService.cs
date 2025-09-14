using System.Diagnostics.SymbolStore;
using DevStack.Application.BoardForge.DTOs.Request;
using DevStack.Application.BoardForge.DTOs.Response;
using DevStack.Application.BoardForge.Interfaces;
using DevStack.Domain.BoardForge.Entities;
using DevStack.Domain.BoardForge.Exceptions;
using DevStack.Domain.BoardForge.Interfaces.Repositories;
using DevStack.Domain.BoardForge.Interfaces.Services;
using DevStack.Domain.BoardForge.Specifications;

namespace DevStack.Application.BoardForge.Services;

public class TeamsService(IUnitOfWork unitOfWork, TimeProvider timeProvider, IStringUtilsService stringUtils) : ITeamsService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly IStringUtilsService _stringUtils = stringUtils;

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

    public async Task<IEnumerable<TeamLabelResponse>> AddTeamLabels(BaseRequest<IEnumerable<AddTeamLabelRequest>> request)
    {
        if (request.ObjectId is null) throw new ArgumentNullException("ObjectId is required");
        if (!request.Data.Any()) throw new ArgumentException("At least one value must be provided");

        IEnumerable<Label> labels = request.Data.Select(l => MapRequestToLabel(l, request));
        List<Label> storedLabels = await _unitOfWork.Labels.AddAsync(labels);

        return storedLabels.Select(MapEntityToLabelResponse);
    }

    private Label MapRequestToLabel<T>(T labelRequest, BaseRequest<IEnumerable<T>> request) where T : AddTeamLabelRequest
    {
        string normalizedName = _stringUtils.NormalizeAndReplaceWhitespaces(labelRequest.Name, '_');
        string color = string.IsNullOrWhiteSpace(labelRequest.Color)
            ? _stringUtils.GetColorFromChar(labelRequest.Name[0]) 
            : labelRequest.Color;
        return new Label
        {
            Name = labelRequest.Name,
            ColorHex = color,
            NormalizedName = normalizedName,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
            IsActive = true,
            CreatedBy = request.UserId,
            TeamId = request.ObjectId!.Value
        };
    }

    private TeamLabelResponse MapEntityToLabelResponse(Label label)
    {
        return new TeamLabelResponse(
            label.Name,
            label.NormalizedName,
            label.ColorHex,
            label.CreatedAt,
            label.UpdatedAt
            );
    }

    public async Task<TeamMembershipResponse> RemoveMemberAsync(BaseRequest<RemoveTeamMemberRequest> request)
    {
        if (request.ObjectId == null) throw new ArgumentException("ObjectId must be provided");

        TeamMembership membership = _unitOfWork.TeamMemberships.ApplySpecification(
            new GetTeamMembershipByUserAndTeamSpecification(request.Data.UserId, request.ObjectId.Value, false)
        ).FirstOrDefault() ?? throw new KeyNotFoundException("Team membership not found");

        await _unitOfWork.TeamMemberships.DeleteAsync(membership);
        await _unitOfWork.SaveChangesAsync();
        return new TeamMembershipResponse(
            membership.UserId,
            membership.TeamId,
            membership.Role,
            membership.CreatedAt,
            membership.CreatedBy
        );
    }

    public async Task<IEnumerable<TeamMembersResponse>> ListMembersAsync(int teamId)
    {
        if (teamId <= 0) throw new ArgumentException("Invalid team ID");

        if (!await _unitOfWork.Teams.ExistsAsync(teamId))
            throw new EntityNotFoundException($"Team with ID {teamId} not found");

        var members = await _unitOfWork.TeamMemberships.ListAsync(
            new GetTeamMembershipsByTeamSpecification(teamId)
        );

        return members.Select(m => new TeamMembersResponse(
            m.TeamId,
            m.UserId,
            m.User!.DisplayName,
            m.User!.Email,
            m.Role,
            m.CreatedAt
        ));
    }
}