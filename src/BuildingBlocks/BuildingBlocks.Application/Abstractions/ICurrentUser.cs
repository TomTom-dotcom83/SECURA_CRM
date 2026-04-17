namespace Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;

public interface ICurrentUser
{
    string UserId { get; }
    string Email { get; }
    string DisplayName { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsInRole(string role);
    string? CorrelationId { get; }
}
