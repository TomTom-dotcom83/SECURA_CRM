using FluentValidation;
using MediatR;
using SECURA.Application.Common.Interfaces;

namespace SECURA.Application.Compliance.Commands;

public sealed record CompleteChecklistItemCommand(
    Guid ChecklistId,
    Guid ItemId) : IRequest<Unit>;

public sealed class CompleteChecklistItemCommandValidator
    : AbstractValidator<CompleteChecklistItemCommand>
{
    public CompleteChecklistItemCommandValidator()
    {
        RuleFor(x => x.ChecklistId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
    }
}

public sealed class CompleteChecklistItemCommandHandler
    : IRequestHandler<CompleteChecklistItemCommand, Unit>
{
    private readonly IChecklistRepository _checklists;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public CompleteChecklistItemCommandHandler(
        IChecklistRepository checklists,
        IUnitOfWork uow,
        ICurrentUser currentUser)
    {
        _checklists = checklists;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(CompleteChecklistItemCommand request,
        CancellationToken cancellationToken)
    {
        var checklist = await _checklists.GetWithItemsAsync(request.ChecklistId, cancellationToken)
            ?? throw new KeyNotFoundException($"Checklist {request.ChecklistId} not found.");

        checklist.CompleteItem(request.ItemId, _currentUser.UserId);
        _checklists.Update(checklist);
        await _uow.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
