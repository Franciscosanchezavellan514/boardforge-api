using DevStack.Application.BoardForge.DTOs.Request;
using DevStack.Application.BoardForge.DTOs.Response;
using DevStack.Application.BoardForge.Interfaces;
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
    ICardsService cardsService) : BaseApiController
{
    private readonly ITeamsService _teamsService = teamsService;
    private readonly ILabelsService _labelsService = labelsService;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ICardsService _cardsService = cardsService;

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

    [HttpGet("{id:int}")]
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
        return CreatedAtAction(nameof(GetAsync), new { id = team.Id }, team);
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

        var baseRequest = new BaseRequest<RemoveTeamMemberRequest>(teamId, CurrentUserId, new RemoveTeamMemberRequest(userId));
        TeamMembershipResponse response = await _teamsService.RemoveMemberAsync(baseRequest);
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

        var baseRequest = new BaseRequest<RemoveTeamLabelRequest>(teamId, CurrentUserId, new RemoveTeamLabelRequest(id));
        await _labelsService.DeleteLabelAsync(baseRequest);
        return NoContent();
    }

    [HttpGet("{teamId:int}/cards")]
    [ProducesResponseType<IEnumerable<CardResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<HttpErrorResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<HttpErrorResponse>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCards(int teamId)
    {
        var readOnlyRequirement = new TeamRoleRequirement(TeamMembershipRole.Role.Viewer);
        var auth = await _authorizationService.AuthorizeAsync(User, new TeamResource(teamId), readOnlyRequirement);
        if (!auth.Succeeded) throw new ForbiddenException();

        IEnumerable<CardResponse> cards = await _cardsService.ListAsync(teamId);
        return Ok(cards);
    }
}
