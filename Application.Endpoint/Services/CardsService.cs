using DevStack.Application.BoardForge.DTOs.Response;
using DevStack.Application.BoardForge.Interfaces;

namespace DevStack.Application.BoardForge.Services;

public class CardsService : ICardsService
{
    public Task<IEnumerable<CardResponse>> ListAsync(int teamId)
    {
        throw new NotImplementedException();
    }
}