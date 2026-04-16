using FluentValidation;
using MediatR;
using SECURA.Application.Common.Interfaces;
using SECURA.Domain.Common;
using SECURA.Domain.Enums;

namespace SECURA.Application.Agencies.Commands;

public sealed record TransitionAgencyStatusCommand(
    Guid AgencyId,
    AgencyStatus TargetStatus,
    string? Reason) : IRequest;

public sealed class TransitionAgencyStatusCommandValidator : AbstractValidator<TransitionAgencyStatusCommand>
{
    public TransitionAgencyStatusCommandValidator()
    {
        RuleFor(x => x.AgencyId).NotEmpty();
    }
}

public sealed class TransitionAgencyStatusCommandHandler : IRequestHandler<TransitionAgencyStatusCommand>
{
    private readonly IAgencyRepository _agencies;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public TransitionAgencyStatusCommandHandler(
        IAgencyRepository agencies,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _agencies = agencies;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(TransitionAgencyStatusCommand request, CancellationToken cancellationToken)
    {
        var agency = await _agencies.GetByIdAsync(request.AgencyId, cancellationToken)
            ?? throw new DomainException($"Agency {request.AgencyId} not found.");

        agency.Transition(request.TargetStatus, _currentUser.UserId);

        _agencies.Update(agency);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
