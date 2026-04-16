using FluentValidation;
using MediatR;
using SECURA.Application.Common.Interfaces;
using SECURA.Domain.Common;
using SECURA.Domain.Entities;
using SECURA.Domain.Enums;

namespace SECURA.Application.Submissions.Commands;

public sealed record RequestDocumentCommand(
    Guid SubmissionId,
    string DocumentType,
    string Notes) : IRequest;

public sealed class RequestDocumentCommandValidator : AbstractValidator<RequestDocumentCommand>
{
    public RequestDocumentCommandValidator()
    {
        RuleFor(x => x.SubmissionId).NotEmpty();
        RuleFor(x => x.DocumentType).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

public sealed class RequestDocumentCommandHandler : IRequestHandler<RequestDocumentCommand>
{
    private readonly ISubmissionRepository _submissions;
    private readonly IRepository<Interaction, Guid> _interactions;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public RequestDocumentCommandHandler(
        ISubmissionRepository submissions,
        IRepository<Interaction, Guid> interactions,
        IUnitOfWork uow,
        ICurrentUser currentUser)
    {
        _submissions = submissions;
        _interactions = interactions;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(RequestDocumentCommand request, CancellationToken cancellationToken)
    {
        var submission = await _submissions.GetByIdAsync(request.SubmissionId, cancellationToken)
            ?? throw new DomainException($"Submission {request.SubmissionId} not found.");

        var summary = string.IsNullOrWhiteSpace(request.Notes)
            ? $"Document requested: {request.DocumentType}"
            : $"Document requested: {request.DocumentType}. {request.Notes}";

        var interaction = Interaction.Create(
            "Submission",
            submission.Id,
            InteractionType.Note,
            summary,
            _currentUser.UserId);

        await _interactions.AddAsync(interaction, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
