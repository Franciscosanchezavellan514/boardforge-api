namespace DevStack.Application.BoardForge.DTOs.Response;

public record TeamMembershipResponse(int UserId, int TeamId, string Role, DateTime CreatedAt, int CreatedBy);
public record TeamMembersResponse(int UserId, string DisplayName, string Email, string Role, DateTime JoinedAt);