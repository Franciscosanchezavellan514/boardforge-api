namespace DevStack.Application.BoardForge.DTOs.Response;

public record TeamResponse(int Id, string Name, string Description, int MemberCount);
public record TeamLabelResponse(string Name, string NormalizedName, string Color, DateTime CreatedAt, DateTime? UpdatedAt);