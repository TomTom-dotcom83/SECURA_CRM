using MediatR;
using Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;
using Secura.DistributionCrm.BuildingBlocks.Domain.Exceptions;
using Secura.DistributionCrm.Submissions.Application.Abstractions;
using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Submissions.Application.Commands;

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
