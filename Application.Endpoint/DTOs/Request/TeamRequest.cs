using System.ComponentModel.DataAnnotations;

namespace DevStack.Application.BoardForge.DTOs.Request;

public record CreateTeamRequest([Required] string Name, string Description);
public record UpdateTeamRequest([Required] string Name, string Description);
public record AddTeamMemberRequest([Required] int UserId, [Required] string Role);
public record UpdateTeamMembershipRequest([Required] string Role);
public record RemoveTeamMemberRequest([Required] int UserId);
public record AddTeamLabelRequest([Required] string Name, string? Color);
public record UpdateTeamLabelRequest([Required] string Name, string? Color);
public record UpdateTeamResourceRequest<TData>(int TeamId, int ResourceId, int UserId, TData Data);
public record DeleteTeamResourceRequest(int ResourceId, int TeamId, int UserId);