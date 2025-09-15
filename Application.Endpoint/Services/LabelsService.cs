using DevStack.Application.BoardForge.DTOs.Request;
using DevStack.Application.BoardForge.DTOs.Response;
using DevStack.Application.BoardForge.Interfaces;
using DevStack.Domain.BoardForge.Entities;
using DevStack.Domain.BoardForge.Interfaces.Repositories;
using DevStack.Domain.BoardForge.Interfaces.Services;
using DevStack.Domain.BoardForge.Specifications;

namespace DevStack.Application.BoardForge.Services;

public class LabelsService(IUnitOfWork unitOfWork, TimeProvider timeProvider, IStringUtilsService stringUtils) : ILabelsService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly IStringUtilsService _stringUtils = stringUtils;

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

    public async Task<IEnumerable<TeamLabelResponse>> GetLabelsAsync(int teamId)
    {
        if (teamId <= 0) throw new ArgumentException("Invalid team ID");
        if (!await _unitOfWork.Teams.ExistsAsync(teamId))
            throw new KeyNotFoundException($"Team with ID {teamId} not found");

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

    public async Task DeleteLabelAsync(BaseRequest<RemoveTeamLabelRequest> request)
    {
        if (request.ObjectId <= 0) throw new ArgumentException("Invalid ObjectId");
        if (!await _unitOfWork.Teams.ExistsAsync(request.ObjectId))
            throw new KeyNotFoundException($"Team with ID {request.ObjectId} not found");

        var labelByIdAndTeamSpec = new GetLabelByIdAndTeamSpecification(request.Data.LabelId, request.ObjectId);
        Label? existingLabel = await _unitOfWork.Labels.GetFirstAsync(labelByIdAndTeamSpec);
        if (existingLabel is null) throw new KeyNotFoundException($"Label with ID: {request.Data.LabelId} not found");

        // Soft delete if label is associated with any cards
        if (existingLabel.CardLabels.Any())
        {
            existingLabel.IsActive = false;
            existingLabel.DeletedAt = _timeProvider.GetUtcNow().UtcDateTime;
            existingLabel.DeletedBy = request.UserId;

            await _unitOfWork.Labels.UpdateAsync(existingLabel);
            await _unitOfWork.SaveChangesAsync();
            return;
        }

        await _unitOfWork.Labels.DeleteAsync(existingLabel);
        await _unitOfWork.SaveChangesAsync();
    }

}