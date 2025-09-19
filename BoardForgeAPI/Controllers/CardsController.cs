using DevStack.Application.BoardForge.DTOs;
using DevStack.Application.BoardForge.DTOs.Request;
using DevStack.Application.BoardForge.DTOs.Response;
using DevStack.Application.BoardForge.Interfaces;
using DevStack.BoardForgeAPI.Authorization;
using DevStack.BoardForgeAPI.Exceptions;
using DevStack.BoardForgeAPI.Models;
using DevStack.Domain.BoardForge.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevStack.BoardForgeAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CardsController(
    IAuthorizationService authorizationService,
    ICardsService cardsService,
    ITeamAuthorizationService teamAuthorizationService) : BaseApiController
{
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ICardsService _cardsService = cardsService;
    private readonly ITeamAuthorizationService _teamAuthorizationService = teamAuthorizationService;

    [HttpGet("{id:int}")]
    [ProducesResponseType<CardResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<HttpErrorResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<HttpErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCardByIdAsync(int id)
    {
        // Retrieve partial card information for authorization avoid loading unnecessary data
        TeamResource? resource = await _teamAuthorizationService.GetTeamResourceAsync<Card>(id);
        if (resource == null) throw new KeyNotFoundException("Card not found.");

        var viewerRequirement = new TeamRoleRequirement(TeamMembershipRole.Role.Viewer);
        var authorizationResult = await _authorizationService.AuthorizeAsync(User, resource, viewerRequirement);
        if (!authorizationResult.Succeeded) throw new ForbiddenException();

        CardResponse card = await _cardsService.GetAsync(id);
        return Ok(card);
    }

    [HttpGet("{id:int}/labels")]
    [ProducesResponseType<IEnumerable<CardLabelResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<HttpErrorResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<HttpErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCardLabelsAsync(int id)
    {
        // Retrieve partial card information for authorization avoid loading unnecessary data
        TeamResource? resource = await _teamAuthorizationService.GetTeamResourceAsync<Card>(id);
        if (resource == null) throw new KeyNotFoundException("Card not found.");

        var viewerRequirement = new TeamRoleRequirement(TeamMembershipRole.Role.Viewer);
        var authorizationResult = await _authorizationService.AuthorizeAsync(User, resource, viewerRequirement);
        if (!authorizationResult.Succeeded) throw new ForbiddenException();

        IEnumerable<CardLabelResponse> labels = await _cardsService.GetLabelsAsync(id);
        return Ok(labels);
    }

    [HttpPost("{id:int}/labels")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<HttpErrorResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<HttpErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddCardLabelAsync(int id, [FromBody] AddCardLabelsRequest request)
    {
        // Retrieve partial card information for authorization avoid loading unnecessary data
        TeamResource? resource = await _teamAuthorizationService.GetTeamResourceAsync<Card>(id);
        if (resource == null) throw new KeyNotFoundException("Card not found.");

        var memberRequirement = new TeamRoleRequirement(TeamMembershipRole.Role.Member);
        var authorizationResult = await _authorizationService.AuthorizeAsync(User, resource, memberRequirement);
        if (!authorizationResult.Succeeded) throw new ForbiddenException();

        await _cardsService.AddLabelsAsync(id, resource.TeamId, request);
        return NoContent();
    }

    [HttpDelete("{id:int}/labels/{labelId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<HttpErrorResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<HttpErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveCardLabelAsync(int id, int labelId)
    {
        // Retrieve partial card information for authorization avoid loading unnecessary data
        TeamResource? resource = await _teamAuthorizationService.GetTeamResourceAsync<Card>(id);
        if (resource == null) throw new KeyNotFoundException("Card not found.");

        var memberRequirement = new TeamRoleRequirement(TeamMembershipRole.Role.Member);
        var authorizationResult = await _authorizationService.AuthorizeAsync(User, resource, memberRequirement);
        if (!authorizationResult.Succeeded) throw new ForbiddenException();

        // Assuming a method exists in ICardsService to remove a label
        await _cardsService.RemoveLabelAsync(id, labelId);
        return NoContent();
    }
}