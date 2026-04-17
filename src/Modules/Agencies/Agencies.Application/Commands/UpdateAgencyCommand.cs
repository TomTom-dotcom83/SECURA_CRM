using FluentValidation;
using MediatR;
using Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;
using Secura.DistributionCrm.BuildingBlocks.Domain.Exceptions;
using Secura.DistributionCrm.Agencies.Application.Abstractions;
using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Agencies.Application.Commands;

public sealed record UpdateAgencyCommand(
    Guid Id,
    string Name,
    AgencyTier Tier,
    string PrimaryState,
    string? Phone,
    string? Email,
    string? Website,
    string? Notes) : IRequest;

public sealed class UpdateAgencyCommandValidator : AbstractValidator<UpdateAgencyCommand>
{
    public UpdateAgencyCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PrimaryState).NotEmpty().Length(2);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
    }
}

public sealed class UpdateAgencyCommandHandler : IRequestHandler<UpdateAgencyCommand>
{
    private readonly IAgencyRepository _agencies;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public UpdateAgencyCommandHandler(
        IAgencyRepository agencies,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _agencies = agencies;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateAgencyCommand request, CancellationToken cancellationToken)
    {
        var agency = await _agencies.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainException($"Agency {request.Id} not found.");

        agency.Update(request.Name, request.Tier, request.PrimaryState,
            request.Phone, request.Email, request.Website, request.Notes, _currentUser.UserId);

        _agencies.Update(agency);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
