using DevStack.Application.BoardForge.DTOs.Request;
using DevStack.Application.BoardForge.DTOs.Response;
using DevStack.Application.BoardForge.Interfaces;
using DevStack.Application.BoardForge.Interfaces.Queries;
using DevStack.BoardForgeAPI.Authorization;
using DevStack.BoardForgeAPI.Exceptions;
using DevStack.BoardForgeAPI.Models;
using DevStack.Domain.BoardForge.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DevStack.BoardForgeAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TeamsController(
    ITeamsService teamsService,
    ILabelsService labelsService,
    IAuthorizationService authorizationService,
    ICardsService cardsService,
    ICardQueries cardQueries) : BaseApiController
{
    private readonly ITeamsService _teamsService = teamsService;
    private readonly ILabelsService _labelsService = labelsService;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ICardsService _cardsService = cardsService;
    private readonly ICardQueries _cardQueries = cardQueries;

    /// <summary>
    /// Get all teams the current user is a member of
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TeamResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(HttpErrorResponse))]
    public async Task<IActionResult> GetMyTeamsAsync()
    {
        var teams = await _teamsService.ListMyTeamsAsync(CurrentUserId);
        return Ok(teams);
    }

    [HttpGet("{id:int}", Name = "GetTeamById")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TeamResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(HttpErrorResponse))]
    public async Task<IActionResult> GetAsync(int id)
    {
        var team = await _teamsService.GetByIdAsync(id);
        return Ok(team);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TeamResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(HttpErrorResponse))]
    public async Task<IActionResult> Post([FromBody] CreateTeamRequest request)
    {
        var baseRequest = new CreateRequest<CreateTeamRequest>(CurrentUserId, request);
        var team = await _teamsService.CreateAsync(baseRequest);
        return CreatedAtRoute("GetTeamById", new { id = team.Id }, team);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TeamResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(HttpErrorResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(HttpErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(HttpErrorResponse))]
    public async Task<IActionResult> Put(int id, [FromBody] UpdateTeamRequest request)
    {
        var ownerRequirement = new TeamRoleRequirement(TeamMembershipRole.Role.Owner);
        var auth = await _authorizationService.AuthorizeAsync(User, new TeamResource(id), ownerRequirement);
        if (!auth.Succeeded) throw new ForbiddenException();

        var baseRequest = new BaseRequest<UpdateTeamRequest>(id, CurrentUserId, request);
        var result = await _teamsService.UpdateAsync(baseRequest);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TeamResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(HttpErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(HttpErrorResponse))]
    public async Task<IActionResult> Delete(int id)
    {
        var ownerRequirement = new TeamRoleRequirement(TeamMembershipRole.Role.Owner);
        var auth = await _authorizationService.AuthorizeAsync(User, new TeamResource(id), ownerRequirement);
        if (!auth.Succeeded) throw new ForbiddenException();

        var baseRequest = new BaseRequest(id, CurrentUserId);
        var result = await _teamsService.SoftDeleteAsync(baseRequest);

        return Ok(result);
    }

    [HttpGet("{teamId:int}/members")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TeamMembersResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(HttpErrorResponse))]
    public async Task<IActionResult> GetMembersAsync(int teamId)
    {
        var readOnlyRequirement = new TeamRoleRequirement(TeamMembershipRole.Role.Viewer);
        var auth = await _authorizationService.AuthorizeAsync(User, new TeamResource(teamId), readOnlyRequirement);
        if (!auth.Succeeded) throw new ForbiddenException();

        var members = await _teamsService.ListMembersAsync(teamId);
        return Ok(members);
    }

    // Add members
    [HttpPost]
    [Route("{teamId:int}/members")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TeamMembershipResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(HttpErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(HttpErrorResponse))]
    public async Task<IActionResult> AddMembersAsync(int teamId, [FromBody] AddTeamMemberRequest request)
    {
        var ownerRequirement = new TeamRoleRequirement(TeamMembershipRole.Role.Owner);
        var auth = await _authorizationService.AuthorizeAsync(User, new TeamResource(teamId), ownerRequirement);
        if (!auth.Succeeded) throw new ForbiddenException();

        var baseRequest = new BaseRequest<AddTeamMemberRequest>(teamId, CurrentUserId, request);
        TeamMembershipResponse response = await _teamsService.AddMemberAsync(baseRequest);
        return Ok(response);
    }

    // Remove members
    [HttpDelete("{teamId:int}/members/{userId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TeamMembershipResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(HttpErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(HttpErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(HttpErrorResponse))]
    public async Task<IActionResult> RemoveMembersAsync(int teamId, int userId)
    {
        var ownerRequirement = new TeamRoleRequirement(TeamMembershipRole.Role.Owner);
        var auth = await _authorizationService.AuthorizeAsync(User, new TeamResource(teamId), ownerRequirement);
        if (!auth.Succeeded) throw new ForbiddenException();

        var deleteRequest = new DeleteTeamResourceRequest(userId, teamId, CurrentUserId);
        TeamMembershipResponse response = await _teamsService.RemoveMemberAsync(deleteRequest);
        return Ok(response);
    }

    [HttpPut("{teamId:int}/members/{userId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TeamMembershipResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(HttpErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(HttpErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(HttpErrorResponse))]
    public async Task<IActionResult> UpdateMembership(int teamId, int userId, [FromBody] UpdateTeamMembershipRequest request)
    {
        if (userId == CurrentUserId) throw new BadHttpRequestException("You cannot change your own role");

        var ownerRequirement = new TeamRoleRequirement(TeamMembershipRole.Role.Owner);
        var auth = await _authorizationService.AuthorizeAsync(User, new TeamResource(teamId), ownerRequirement);
        if (!auth.Succeeded) throw new ForbiddenException();

        var baseRequest = new BaseRequest<KeyValuePair<int, UpdateTeamMembershipRequest>>(teamId, CurrentUserId, KeyValuePair.Create(userId, request));
        TeamMembershipResponse response = await _teamsService.UpdateMembershipAsync(baseRequest);
        return Ok(response);
    }

    [HttpGet("{teamId:int}/labels")]
    [ProducesResponseType<IEnumerable<TeamLabelResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(HttpErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(HttpErrorResponse))]
    public async Task<IActionResult> GetTeamLabels(int teamId)
    {
        var readOnlyRequirement = new TeamRoleRequirement(TeamMembershipRole.Role.Viewer);
        var auth = await _authorizationService.AuthorizeAsync(User, new TeamResource(teamId), readOnlyRequirement);
        if (!auth.Succeeded) throw new ForbiddenException();

        IEnumerable<TeamLabelResponse> labels = await _labelsService.GetLabelsAsync(teamId);
        return Ok(labels);
    }

    [HttpPost("{teamId:int}/labels")]
    [ProducesResponseType<TeamLabelOperationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(HttpErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(HttpErrorResponse))]
    public async Task<IActionResult> AddTeamLabels(int teamId, [FromBody] IEnumerable<AddTeamLabelRequest> requests)
    {
        var ownerRequirement = new TeamRoleRequirement(TeamMembershipRole.Role.Owner);
        var auth = await _authorizationService.AuthorizeAsync(User, new TeamResource(teamId), ownerRequirement);
        if (!auth.Succeeded) throw new ForbiddenException();

        var baseRequest = new BaseRequest<IEnumerable<AddTeamLabelRequest>>(teamId, CurrentUserId, requests);
        TeamLabelOperationResponse labels = await _labelsService.AddLabels(baseRequest);
        return Ok(labels);
    }

    [HttpPut("{teamId:int}/labels/{id:int}")]
    [ProducesResponseType<TeamLabelResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(HttpErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(HttpErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(HttpErrorResponse))]
    public async Task<IActionResult> UpdateLabel(int teamId, int id, [FromBody] UpdateTeamLabelRequest request)
    {
        var ownerRequirement = new TeamRoleRequirement(TeamMembershipRole.Role.Owner);
        var auth = await _authorizationService.AuthorizeAsync(User, new TeamResource(teamId), ownerRequirement);
        if (!auth.Succeeded) throw new ForbiddenException();

        var baseRequest = new BaseRequest<KeyValuePair<int, UpdateTeamLabelRequest>>(
            teamId,
            CurrentUserId,
            KeyValuePair.Create(id, request)
            );

        TeamLabelResponse response = await _labelsService.UpdateLabelAsync(baseRequest);
        return Ok(response);
    }

    [HttpDelete("{teamId:int}/labels/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(HttpErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(HttpErrorResponse))]
    public async Task<IActionResult> DeleteLabel(int teamId, int id)
    {
        var ownerRequirement = new TeamRoleRequirement(TeamMembershipRole.Role.Owner);
        var auth = await _authorizationService.AuthorizeAsync(User, new TeamResource(teamId), ownerRequirement);
        if (!auth.Succeeded) throw new ForbiddenException();

        var deleteRequest = new DeleteTeamResourceRequest(id, teamId, CurrentUserId);
        await _labelsService.DeleteLabelAsync(deleteRequest);
        return NoContent();
    }

    [HttpGet("{teamId:int}/cards")]
    [ProducesResponseType<IEnumerable<CardResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<HttpErrorResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<HttpErrorResponse>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCardsAsync(int teamId)
    {
        var readOnlyRequirement = new TeamRoleRequirement(TeamMembershipRole.Role.Viewer);
        var auth = await _authorizationService.AuthorizeAsync(User, new TeamResource(teamId), readOnlyRequirement);
        if (!auth.Succeeded) throw new ForbiddenException();

        IEnumerable<CardResponse> cards = await _cardQueries.GetByTeamWithLabelsAsync(teamId);
        return Ok(cards);
    }

    [HttpGet("{teamId:int}/cards/{id:int}", Name = "GetTeamCardById")]
    [ProducesResponseType<CardResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<HttpErrorResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<HttpErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCardAsync(int teamId, int id)
    {
        var readOnlyRequirement = new TeamRoleRequirement(TeamMembershipRole.Role.Viewer);
        var auth = await _authorizationService.AuthorizeAsync(User, new TeamResource(teamId), readOnlyRequirement);
        if (!auth.Succeeded) throw new ForbiddenException();

        CardResponse card = await _cardsService.GetAsync(teamId, id);
        return Ok(card);
    }

    [HttpPost("{teamId:int}/cards")]
    [ProducesResponseType<CardResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<HttpErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<HttpErrorResponse>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateCardAsync(int teamId, [FromBody] CreateCardRequest request)
    {
        var writeRequirement = new TeamRoleRequirement(TeamMembershipRole.Role.Member);
        var auth = await _authorizationService.AuthorizeAsync(User, new TeamResource(teamId), writeRequirement);
        if (!auth.Succeeded) throw new ForbiddenException();

        var baseRequest = new BaseRequest<CreateCardRequest>(teamId, CurrentUserId, request);
        CardResponse card = await _cardsService.CreateAsync(baseRequest);
        return CreatedAtRoute("GetTeamCardById", new { teamId, id = card.Id }, card);
    }

    [HttpPatch("{teamId:int}/cards/{id:int}")]
    [ProducesResponseType<CardResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<HttpErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<HttpErrorResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<HttpErrorResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<HttpErrorResponse>(StatusCodes.Status412PreconditionFailed)]
    public async Task<IActionResult> UpdateCardAsync(int teamId, int id, [FromBody] UpdateCardRequest request, [FromHeader(Name = "If-Match")] string ifMatch)
    {
        var writeRequirement = new TeamRoleRequirement(TeamMembershipRole.Role.Member);
        var auth = await _authorizationService.AuthorizeAsync(User, new TeamResource(teamId), writeRequirement);
        if (!auth.Succeeded) throw new ForbiddenException();

        if (string.IsNullOrWhiteSpace(ifMatch))
        {
            throw new BadHttpRequestException("Missing If-Match header");
        }

        var updateRequest = new UpdateTeamResourceRequest<UpdateCardRequest>(teamId, id, CurrentUserId, request);
        var updatedCard = await _cardsService.UpdateAsync(updateRequest, ifMatch);
        return Ok(updatedCard);
    }

    [HttpDelete("{teamId:int}/cards/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<HttpErrorResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<HttpErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCardAsync(int teamId, int id)
    {
        var writeRequirement = new TeamRoleRequirement(TeamMembershipRole.Role.Member);
        var auth = await _authorizationService.AuthorizeAsync(User, new TeamResource(teamId), writeRequirement);
        if (!auth.Succeeded) throw new ForbiddenException();

        var deleteRequest = new DeleteTeamResourceRequest(id, teamId, CurrentUserId);
        await _cardsService.SoftDeleteAsync(deleteRequest);
        return NoContent();
    }
}
