namespace DevStack.Application.BoardForge.DTOs.Response;

public record TeamMembershipResponse(int UserId, int TeamId, string Role, DateTime CreatedAt, int CreatedBy);
