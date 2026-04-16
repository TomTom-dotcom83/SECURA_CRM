using FluentValidation;
using MediatR;
using SECURA.Application.Common.Interfaces;
using SECURA.Domain.Entities;
using SECURA.Domain.Enums;

namespace SECURA.Application.Interactions.Commands;

public sealed record LogInteractionCommand(
    string RelatedEntityType,
    Guid RelatedEntityId,
    InteractionType InteractionType,
    string Summary,
    string? DetailNotes = null,
    DateTime? DueDate = null) : IRequest<Guid>;

public sealed class LogInteractionCommandValidator
    : AbstractValidator<LogInteractionCommand>
{
    public LogInteractionCommandValidator()
    {
        RuleFor(x => x.RelatedEntityType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RelatedEntityId).NotEmpty();
        RuleFor(x => x.Summary).NotEmpty().MaximumLength(500);
        RuleFor(x => x.DetailNotes).MaximumLength(4000);
    }
}

public sealed class LogInteractionCommandHandler
    : IRequestHandler<LogInteractionCommand, Guid>
{
    private readonly IInteractionRepository _interactions;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public LogInteractionCommandHandler(
        IInteractionRepository interactions,
        IUnitOfWork uow,
        ICurrentUser currentUser)
    {
        _interactions = interactions;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(LogInteractionCommand request,
        CancellationToken cancellationToken)
    {
        var interaction = Interaction.Create(
            request.RelatedEntityType,
            request.RelatedEntityId,
            request.InteractionType,
            request.Summary,
            _currentUser.UserId);

        await _interactions.AddAsync(interaction, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return interaction.Id;
    }
}
