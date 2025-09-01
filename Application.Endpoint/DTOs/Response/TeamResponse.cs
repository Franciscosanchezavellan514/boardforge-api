namespace DevStack.Application.Endpoint.DTOs.Response;

public record TeamResponse(int Id, string Name, string Description, int MemberCount);