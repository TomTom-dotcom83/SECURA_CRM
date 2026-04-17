using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Administration.Application.AppetiteTags;

public sealed record AppetiteTagDto(
    Guid Id,
    string Label,
    LobType? Lob,
    string? State,
    string Color,
    bool IsActive);
