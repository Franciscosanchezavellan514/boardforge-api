namespace DevStack.Application.BoardForge.DTOs.Request;

public record CreateTeamRequest(string Name, string Description);
public record UpdateTeamRequest(string Name, string Description);
public record AddTeamMemberRequest(int UserId, string Role);