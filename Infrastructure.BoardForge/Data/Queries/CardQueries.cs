using DevStack.Application.BoardForge.DTOs.Response;
using DevStack.Domain.BoardForge.Interfaces.Services;
using DevStack.Infrastructure.BoardForge.Data;
using Microsoft.EntityFrameworkCore;

namespace DevStack.Application.BoardForge.Interfaces.Queries;

public class CardQueries(BoardForgeDbContext dbContext, IEtagService etagService) : ICardQueries
{

    public async Task<IEnumerable<CardResponse>> GetByTeamWithLabelsAsync(int teamId)
    {
        var cards = await dbContext.Cards
            .AsNoTracking()
            .Where(c => c.TeamId == teamId && c.IsActive)
            .Select(c => new
            {
                c.Id,
                c.Title,
                c.Description,
                c.Order,
                c.BoardColumnId,
                c.TeamId,
                c.BoardId,
                c.OwnerId,
                OwnerName = c.Owner!.DisplayName,
                c.RowVersion,
                c.CreatedAt,
                c.UpdatedAt,
                c.CreatedBy,
                c.UpdatedBy,
                Labels = c.Labels.Where(cl => cl.Label!.IsActive).Select(cl => new { cl.Label!.Id, cl.Label.Name, cl.Label.ColorHex }).ToList()
            })
            .AsSplitQuery()
            .ToListAsync();

        return cards.Select(c =>
        {
            var etag = etagService.FromRowVersion(c.RowVersion);
            var labels = c.Labels.Select(l => new CardLabelResponse(l.Id, l.Name, l.ColorHex));
            return new CardResponse(
                c.Id,
                c.Title,
                c.Description,
                c.Order,
                c.BoardColumnId,
                c.TeamId,
                c.BoardId,
                c.OwnerId,
                c.OwnerName,
                etag,
                c.CreatedAt,
                c.UpdatedAt,
                c.CreatedBy,
                c.UpdatedBy,
                labels
            );
        });
    }
}
