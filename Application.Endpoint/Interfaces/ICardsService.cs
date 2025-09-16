using DevStack.Application.BoardForge.DTOs.Request;
using DevStack.Application.BoardForge.DTOs.Response;

namespace DevStack.Application.BoardForge.Interfaces;

public interface ICardsService
{
    Task<IEnumerable<CardResponse>> ListAsync(int teamId);
    Task<CardResponse> CreateAsync(CreateCardRequest request);
}