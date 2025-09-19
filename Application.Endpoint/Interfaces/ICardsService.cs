using DevStack.Application.BoardForge.DTOs.Request;
using DevStack.Application.BoardForge.DTOs.Response;

namespace DevStack.Application.BoardForge.Interfaces;

public interface ICardsService
{
    Task<CardResponse> CreateAsync(BaseRequest<CreateCardRequest> request);
    Task<CardResponse> GetAsync(int teamId, int cardId);
    Task<CardResponse> GetAsync(int id);
    Task<CardResponse> UpdateAsync(UpdateTeamResourceRequest<UpdateCardRequest> request, string etag);
    Task SoftDeleteAsync(DeleteTeamResourceRequest request);
    Task<IEnumerable<CardLabelResponse>> GetLabelsAsync(int id);
    Task AddLabelsAsync(int id, AddCardLabelsRequest request);
    Task RemoveLabelAsync(int id, int labelId);
}