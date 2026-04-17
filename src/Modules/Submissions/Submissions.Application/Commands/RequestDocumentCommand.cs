using FluentValidation;
using MediatR;
using Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;
using Secura.DistributionCrm.BuildingBlocks.Domain.Exceptions;
using Secura.DistributionCrm.Submissions.Application.Abstractions;

namespace Secura.DistributionCrm.Submissions.Application.Commands;

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
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public RequestDocumentCommandHandler(
        ISubmissionRepository submissions,
        IUnitOfWork uow,
        ICurrentUser currentUser)
    {
        _submissions = submissions;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(RequestDocumentCommand request, CancellationToken cancellationToken)
    {
        var submission = await _submissions.GetByIdAsync(request.SubmissionId, cancellationToken)
            ?? throw new DomainException($"Submission {request.SubmissionId} not found.");

        // Log the document request as a note on the submission
        var summary = string.IsNullOrWhiteSpace(request.Notes)
            ? $"Document requested: {request.DocumentType}"
            : $"Document requested: {request.DocumentType}. {request.Notes}";
        submission.AddNote(summary, _currentUser.UserId);

        _submissions.Update(submission);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
