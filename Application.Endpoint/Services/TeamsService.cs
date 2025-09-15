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
        if (request.ObjectId <= 0) throw new ArgumentException("Invalid object Id");
        if (!TeamMembershipRole.AllRoles.Contains(request.Data.Role)) throw new ArgumentException("Invalid role");

        TeamMembership membership = new()
        {
            TeamId = request.ObjectId,
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

    public async Task<TeamResponse> CreateAsync(CreateRequest<CreateTeamRequest> request)
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
        if (request.ObjectId <= 0) throw new ArgumentException("Invalid ObjectId");

        Team? dbTeam = await _unitOfWork.Teams.GetByIdAsync(request.ObjectId);
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

    public async Task<TeamLabelOperationResponse> AddLabels(BaseRequest<IEnumerable<AddTeamLabelRequest>> request)
    {
        if (request.ObjectId <= 0) throw new ArgumentNullException("Invalid ObjectId");
        if (!request.Data.Any()) throw new ArgumentException("At least one value must be provided");

        List<Label> labels = request.Data.Select(l => MapRequestToLabel(l, request)).ToList();
        bool containsDuplicates = labels.GroupBy(l => l.NormalizedName).Any(g => g.Count() > 1);
        if (containsDuplicates) throw new ArgumentException("Duplicate label names found in request");

        var query = new GetLabelsByTeamAndNormalizedNameSpecification(request.ObjectId, labels.Select(l => l.NormalizedName));
        IReadOnlyList<Label> existingLabels = await _unitOfWork.Labels.ListAsync(query);
        if (existingLabels.Any())
        {
            List<string> normalizedNames = existingLabels.Select(el => el.NormalizedName).ToList();
            IEnumerable<Label> filteredLabels = labels.Where(l => !normalizedNames.Contains(l.NormalizedName)).ToList();
            if (filteredLabels.Any())
            {
                await _unitOfWork.Labels.AddAsync(filteredLabels);
                await _unitOfWork.Labels.SaveChangesAsync();
            }

            return new TeamLabelOperationResponse(
                filteredLabels.Select(MapEntityToLabelResponse),
                existingLabels.Select(MapEntityToLabelResponse)
                );
        }

        List<Label> storedLabels = await _unitOfWork.Labels.AddAsync(labels);
        await _unitOfWork.Labels.SaveChangesAsync();

        return new TeamLabelOperationResponse(storedLabels.Select(MapEntityToLabelResponse), []);
    }

    public async Task<IEnumerable<TeamLabelResponse>> GetLabelsAsync(int teamId)
    {
        if (teamId <= 0) throw new ArgumentException("Invalid team ID");
        if (!await _unitOfWork.Teams.ExistsAsync(teamId))
            throw new EntityNotFoundException($"Team with ID {teamId} not found");

        IReadOnlyList<Label> labels = await _unitOfWork.Labels.ListAsync(new GetLabelsByTeamSpecification(teamId));

        return labels.Select(MapEntityToLabelResponse);
    }

    public async Task<TeamLabelResponse> UpdateLabelAsync(BaseRequest<KeyValuePair<int, UpdateTeamLabelRequest>> request)
    {
        if (request.ObjectId <= 0) throw new ArgumentException("Invalid ObjectId");
        if (!await _unitOfWork.Teams.ExistsAsync(request.ObjectId))
            throw new KeyNotFoundException($"Team with ID {request.ObjectId} not found");

        var labelByIdAndTeamSpec = new GetLabelByIdAndTeamSpecification(request.Data.Key, request.ObjectId);
        var existingLabel = await _unitOfWork.Labels.GetFirstAsync(labelByIdAndTeamSpec);
        if (existingLabel is null) throw new KeyNotFoundException($"Label with ID: {request.Data.Key} not found");

        existingLabel.Name = request.Data.Value.Name;
        existingLabel.NormalizedName = _stringUtils.NormalizeAndReplaceWhitespaces(existingLabel.Name, '_');
        existingLabel.ColorHex = string.IsNullOrWhiteSpace(request.Data.Value.Color)
            ? existingLabel.ColorHex
            : request.Data.Value.Color;
        existingLabel.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;
        existingLabel.UpdatedBy = request.UserId;

        var duplicateCheckSpec = new GetLabelsByTeamAndNormalizedNameSpecification(
            existingLabel.TeamId,
            existingLabel.NormalizedName, 
            existingLabel.Id
            );
        var isDuplicated = await _unitOfWork.Labels.ExistsAsync(duplicateCheckSpec);
        if (isDuplicated) throw new ArgumentException($"Label with name {existingLabel.Name} already exists");

        await _unitOfWork.Labels.UpdateAsync(existingLabel);
        await _unitOfWork.SaveChangesAsync();

        return MapEntityToLabelResponse(existingLabel);
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
            TeamId = request.ObjectId
        };
    }

    private TeamLabelResponse MapEntityToLabelResponse(Label label)
    {
        return new TeamLabelResponse(
            label.Id,
            label.Name,
            label.NormalizedName,
            label.ColorHex,
            label.CreatedAt,
            label.UpdatedAt
            );
    }

    public async Task<TeamMembershipResponse> UpdateMembershipAsync(BaseRequest<KeyValuePair<int, UpdateTeamMembershipRequest>> request)
    {
        if (request.ObjectId <= 0) throw new ArgumentException("Invalid ObjectId");
        if (!TeamMembershipRole.AllRoles.Contains(request.Data.Value.Role)) throw new ArgumentException("Invalid role");

        if (!await _unitOfWork.Teams.ExistsAsync(request.ObjectId))
            throw new KeyNotFoundException($"Team with ID {request.ObjectId} not found");

        var querySpec = new GetTeamMembershipByUserAndTeamSpecification(request.Data.Key, request.ObjectId, false);
        TeamMembership? membership = await _unitOfWork.TeamMemberships.GetFirstAsync(querySpec);
        if (membership is null) throw new KeyNotFoundException($"Member with Id: {request.Data.Key} not found");

        membership.Role = request.Data.Value.Role;
        membership.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;
        membership.UpdatedBy = request.UserId;
        await _unitOfWork.TeamMemberships.UpdateAsync(membership);
        await _unitOfWork.SaveChangesAsync();
        
        return new TeamMembershipResponse(
            membership.UserId,
            membership.TeamId,
            membership.Role,
            membership.CreatedAt,
            membership.CreatedBy
        );
    }

    public async Task<TeamMembershipResponse> RemoveMemberAsync(BaseRequest<RemoveTeamMemberRequest> request)
    {
        if (request.ObjectId <= 0) throw new ArgumentException("Invalid ObjectId");

        TeamMembership membership = _unitOfWork.TeamMemberships.ApplySpecification(
            new GetTeamMembershipByUserAndTeamSpecification(request.Data.UserId, request.ObjectId, false)
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