using MediatR;
using SECURA.Application.Common.Interfaces;
using SECURA.Domain.Common;
using SECURA.Domain.Enums;

namespace SECURA.Application.Submissions.Commands;

public sealed record TransitionSubmissionStatusCommand(
    Guid SubmissionId,
    SubmissionStatus TargetStatus) : IRequest;

public sealed class TransitionSubmissionStatusCommandHandler
    : IRequestHandler<TransitionSubmissionStatusCommand>
{
    private readonly ISubmissionRepository _submissions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public TransitionSubmissionStatusCommandHandler(
        ISubmissionRepository submissions,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _submissions = submissions;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(TransitionSubmissionStatusCommand request, CancellationToken cancellationToken)
    {
        var submission = await _submissions.GetByIdAsync(request.SubmissionId, cancellationToken)
            ?? throw new DomainException($"Submission {request.SubmissionId} not found.");

        submission.Transition(request.TargetStatus, _currentUser.UserId);

        _submissions.Update(submission);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
