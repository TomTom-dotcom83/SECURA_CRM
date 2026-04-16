using FluentValidation;
using MediatR;
using SECURA.Application.Common.Interfaces;
using SECURA.Domain.Common;
using SECURA.Domain.Enums;

namespace SECURA.Application.Compliance.Commands;

public sealed record TriggerAgencyActivationCommand(Guid AgencyId) : IRequest<Unit>;

public sealed class TriggerAgencyActivationCommandValidator
    : AbstractValidator<TriggerAgencyActivationCommand>
{
    public TriggerAgencyActivationCommandValidator()
    {
        RuleFor(x => x.AgencyId).NotEmpty();
    }
}

public sealed class TriggerAgencyActivationCommandHandler
    : IRequestHandler<TriggerAgencyActivationCommand, Unit>
{
    private readonly IAgencyRepository _agencies;
    private readonly IChecklistRepository _checklists;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public TriggerAgencyActivationCommandHandler(
        IAgencyRepository agencies,
        IChecklistRepository checklists,
        IUnitOfWork uow,
        ICurrentUser currentUser)
    {
        _agencies = agencies;
        _checklists = checklists;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(TriggerAgencyActivationCommand request,
        CancellationToken cancellationToken)
    {
        var agency = await _agencies.GetByIdAsync(request.AgencyId, cancellationToken)
            ?? throw new KeyNotFoundException($"Agency {request.AgencyId} not found.");

        var checklist = await _checklists.GetByAgencyIdAsync(request.AgencyId, cancellationToken);

        if (checklist is not null && !checklist.IsComplete)
            throw new DomainException(
                "Cannot activate agency — onboarding checklist is not yet complete.");

        agency.Transition(AgencyStatus.Active, _currentUser.UserId);
        _agencies.Update(agency);
        await _uow.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
