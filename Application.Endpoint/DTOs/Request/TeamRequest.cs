using System.ComponentModel.DataAnnotations;

namespace DevStack.Application.BoardForge.DTOs.Request;

public record CreateTeamRequest([Required] string Name, string Description);
public record UpdateTeamRequest([Required] string Name, string Description);
public record AddTeamMemberRequest([Required] int UserId, [Required] string Role);
public record RemoveTeamMemberRequest([Required] int UserId);
public record AddTeamLabelRequest([Required] string Name, string? Color);