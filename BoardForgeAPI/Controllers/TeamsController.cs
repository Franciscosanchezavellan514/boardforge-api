using System.Security.Claims;
using DevStack.Application.BoardForge.DTOs.Response;
using DevStack.Application.BoardForge.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DevStack.BoardForgeAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TeamsController(ITeamsService teamsService) : ControllerBase
{
    private readonly ITeamsService _teamsService = teamsService;

    // GET: api/<TeamsController>
    [HttpGet]
    [Route("my-teams")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TeamResponse>))]
    public async Task<IEnumerable<TeamResponse>> GetMyTeamsAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null || !int.TryParse(userId, out int userIdInt))
        {
            return Array.Empty<TeamResponse>();
        }

        return await _teamsService.ListMyTeamsAsync(userIdInt);
    }

    // // GET api/<TeamsController>/5
    // [HttpGet("{id}")]
    // public string Get(int id)
    // {
    //     return "value";
    // }

    // POST api/<TeamsController>
    //[HttpPost]
    //public void Post([FromBody] string value)
    //{
    //}

    // // PUT api/<TeamsController>/5
    // [HttpPut("{id}")]
    // public void Put(int id, [FromBody] string value)
    // {
    // }

    // // DELETE api/<TeamsController>/5
    // [HttpDelete("{id}")]
    // public void Delete(int id)
    // {
    // }
}
