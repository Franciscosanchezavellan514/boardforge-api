using DevStack.Application.BoardForge.DTOs.Request;
using DevStack.Application.BoardForge.DTOs.Response;

namespace DevStack.Application.BoardForge.Interfaces;

public interface ILabelsService
{
    Task<TeamLabelOperationResponse> AddLabels(BaseRequest<IEnumerable<AddTeamLabelRequest>> request);
    Task<IEnumerable<TeamLabelResponse>> GetLabelsAsync(int teamId);
    Task<TeamLabelResponse> UpdateLabelAsync(BaseRequest<KeyValuePair<int, UpdateTeamLabelRequest>> request);
    Task DeleteLabelAsync(DeleteTeamResourceRequest request);
}