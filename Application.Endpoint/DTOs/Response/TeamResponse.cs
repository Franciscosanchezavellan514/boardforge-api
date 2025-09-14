namespace DevStack.Application.BoardForge.DTOs.Response;

public record TeamResponse(int Id, string Name, string Description, int MemberCount);
public record TeamLabelResponse(int Id, string Name, string NormalizedName, string Color, DateTime CreatedAt, DateTime? UpdatedAt);
public record TeamLabelOperationResponse(IEnumerable<TeamLabelResponse> Created, IEnumerable<TeamLabelResponse> Skipped);