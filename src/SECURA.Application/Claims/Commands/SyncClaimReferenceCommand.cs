using FluentValidation;
using MediatR;
using SECURA.Application.Common.Interfaces;
using SECURA.Domain.Entities;
using SECURA.Domain.Enums;

namespace SECURA.Application.Claims.Commands;

public sealed record SyncClaimReferenceCommand(
    Guid AgencyId,
    string ExternalClaimNumber,
    LobType Lob,
    DateTime LossDate,
    string? InsuredName,
    string? Description) : IRequest<Guid>;

public sealed class SyncClaimReferenceCommandValidator
    : AbstractValidator<SyncClaimReferenceCommand>
{
    public SyncClaimReferenceCommandValidator()
    {
        RuleFor(x => x.AgencyId).NotEmpty();
        RuleFor(x => x.ExternalClaimNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LossDate).NotEmpty();
    }
}

public sealed class SyncClaimReferenceCommandHandler
    : IRequestHandler<SyncClaimReferenceCommand, Guid>
{
    private readonly IClaimRepository _claims;
    private readonly IUnitOfWork _uow;

    public SyncClaimReferenceCommandHandler(IClaimRepository claims, IUnitOfWork uow)
    {
        _claims = claims;
        _uow = uow;
    }

    public async Task<Guid> Handle(SyncClaimReferenceCommand request,
        CancellationToken cancellationToken)
    {
        var claim = ClaimReference.Create(
            request.AgencyId,
            request.ExternalClaimNumber,
            request.Lob,
            request.LossDate);

        await _claims.AddAsync(claim, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return claim.Id;
    }
}
