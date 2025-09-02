using DevStack.Application.BoardForge.DTOs.Request;
using DevStack.Application.BoardForge.DTOs.Response;
using DevStack.Application.BoardForge.Interfaces;
using DevStack.BoardForgeAPI.Authorization;
using DevStack.BoardForgeAPI.Models;
using DevStack.Domain.BoardForge.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DevStack.BoardForgeAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TeamsController(ITeamsService teamsService, IAuthorizationService authorizationService) : BaseApiController
{
    private readonly ITeamsService _teamsService = teamsService;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    [HttpGet]
    [Route("my-teams")]
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
        var baseRequest = new BaseRequest<CreateTeamRequest>(null, CurrentUserId, request);
        var team = await _teamsService.CreateAsync(baseRequest);
        return CreatedAtAction(nameof(GetAsync), new { id = team.Id }, team);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TeamResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(HttpErrorResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(HttpErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ForbidResult))]
    public async Task<IActionResult> Put(int id, [FromBody] UpdateTeamRequest request)
    {
        var ownerRequirement = new TeamRoleRequirement(TeamMembershipRole.Role.Owner);
        var auth = await _authorizationService.AuthorizeAsync(User, new TeamResource(id), ownerRequirement);
        if (!auth.Succeeded) return Forbid();

        var baseRequest = new BaseRequest<UpdateTeamRequest>(id, CurrentUserId, request);
        var result = await _teamsService.UpdateAsync(baseRequest);
        return Ok(result);
    }

    // Add members
    [HttpPost]
    [Route("{teamId:int}/members")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TeamMembershipResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(HttpErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ForbidResult))]
    public async Task<IActionResult> AddMembersAsync(int teamId, [FromBody] AddTeamMemberRequest request)
    {
        var ownerRequirement = new TeamRoleRequirement(TeamMembershipRole.Role.Owner);
        var auth = await _authorizationService.AuthorizeAsync(User, new TeamResource(teamId), ownerRequirement);
        if (!auth.Succeeded) return Forbid();

        var baseRequest = new BaseRequest<AddTeamMemberRequest>(teamId, CurrentUserId, request);
        TeamMembershipResponse response = await _teamsService.AddMemberAsync(baseRequest);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TeamResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ForbidResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(HttpErrorResponse))]
    public async Task<IActionResult> Delete(int id)
    {
        var ownerRequirement = new TeamRoleRequirement(TeamMembershipRole.Role.Owner);
        var auth = await _authorizationService.AuthorizeAsync(User, new TeamResource(id), ownerRequirement);
        if (!auth.Succeeded) return Forbid();

        var baseRequest = new BaseRequest(id, CurrentUserId);
        var result = await _teamsService.SoftDeleteAsync(baseRequest);

        return Ok(result);
    }
}
