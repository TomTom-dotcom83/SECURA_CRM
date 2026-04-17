using MediatR;
using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Administration.Application.AppetiteTags;

public sealed record CreateAppetiteTagCommand(
    string Label,
    LobType? Lob,
    string? State,
    string Color = "#808080") : IRequest<Guid>;
