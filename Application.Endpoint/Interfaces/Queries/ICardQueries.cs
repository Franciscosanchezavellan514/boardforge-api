using DevStack.Application.BoardForge.DTOs.Response;

namespace DevStack.Application.BoardForge.Interfaces.Queries;

public interface ICardQueries
{
    Task<IEnumerable<CardResponse>> GetByTeamWithLabelsAsync(int teamId);
}