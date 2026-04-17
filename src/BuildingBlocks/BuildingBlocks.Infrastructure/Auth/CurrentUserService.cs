using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;

namespace Secura.DistributionCrm.BuildingBlocks.Infrastructure.Auth;

public sealed class CurrentUserService : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    private string? Find(string type) =>
        User?.FindFirst(type)?.Value;

    public string UserId =>
        Find(ClaimTypes.NameIdentifier)
        ?? Find("oid")
        ?? "anonymous";

    public string Email =>
        Find(ClaimTypes.Email)
        ?? Find("preferred_username")
        ?? string.Empty;

    public string DisplayName =>
        Find("name")
        ?? Find(ClaimTypes.Name)
        ?? Email;

    public IReadOnlyList<string> Roles =>
        User?.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "roles")
            .Select(c => c.Value)
            .ToList()
        ?? (IReadOnlyList<string>)Array.Empty<string>();

    public bool IsInRole(string role) =>
        User?.IsInRole(role) ?? false;

    public string? CorrelationId =>
        _httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString();
}
