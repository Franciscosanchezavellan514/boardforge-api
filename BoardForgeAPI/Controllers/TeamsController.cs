using System.Security.Claims;
using DevStack.Application.BoardForge.DTOs.Request;
using DevStack.Application.BoardForge.DTOs.Response;
using DevStack.Application.BoardForge.Interfaces;
using DevStack.BoardForgeAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DevStack.BoardForgeAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TeamsController(ITeamsService teamsService) : BaseApiController
{
    private readonly ITeamsService _teamsService = teamsService;

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
    public async Task<IActionResult> Put(int id, [FromBody] UpdateTeamRequest request)
    {
        var baseRequest = new BaseRequest<UpdateTeamRequest>(id, CurrentUserId, request);
        var result = await _teamsService.UpdateAsync(baseRequest);
        return Ok(result);
    }

    // // DELETE api/<TeamsController>/5
    // [HttpDelete("{id}")]
    // public void Delete(int id)
    // {
    // }
}
