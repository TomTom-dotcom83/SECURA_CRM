using FluentValidation;
using MediatR;
using Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;
using Secura.DistributionCrm.BuildingBlocks.Domain.Exceptions;
using Secura.DistributionCrm.Submissions.Application.Abstractions;

namespace Secura.DistributionCrm.Submissions.Application.Commands;

public sealed record AddUWNoteCommand(
    Guid SubmissionId,
    string NoteText) : IRequest<Guid>;

public sealed class AddUWNoteCommandValidator : AbstractValidator<AddUWNoteCommand>
{
    public AddUWNoteCommandValidator()
    {
        RuleFor(x => x.SubmissionId).NotEmpty();
        RuleFor(x => x.NoteText).NotEmpty().MaximumLength(4000);
    }
}

public sealed class AddUWNoteCommandHandler : IRequestHandler<AddUWNoteCommand, Guid>
{
    private readonly ISubmissionRepository _submissions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public AddUWNoteCommandHandler(
        ISubmissionRepository submissions,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _submissions = submissions;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(AddUWNoteCommand request, CancellationToken cancellationToken)
    {
        var submission = await _submissions.GetByIdAsync(request.SubmissionId, cancellationToken)
            ?? throw new DomainException($"Submission {request.SubmissionId} not found.");

        var note = submission.AddNote(request.NoteText, _currentUser.UserId);

        _submissions.Update(submission);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return note.Id;
    }
}
