using DevStack.Application.BoardForge.DTOs.Response;

namespace DevStack.Application.BoardForge.Interfaces;

public interface ICardsService
{
    Task<IEnumerable<CardResponse>> ListAsync(int teamId);
}