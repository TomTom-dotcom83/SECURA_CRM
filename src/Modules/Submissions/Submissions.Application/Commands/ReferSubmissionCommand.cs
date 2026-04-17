using FluentValidation;
using MediatR;
using Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;
using Secura.DistributionCrm.BuildingBlocks.Domain.Exceptions;
using Secura.DistributionCrm.Submissions.Application.Abstractions;
using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Submissions.Application.Commands;

public sealed record ReferSubmissionCommand(
    Guid SubmissionId,
    string ReferralReason) : IRequest;

public sealed class ReferSubmissionCommandValidator : AbstractValidator<ReferSubmissionCommand>
{
    public ReferSubmissionCommandValidator()
    {
        RuleFor(x => x.SubmissionId).NotEmpty();
        RuleFor(x => x.ReferralReason).NotEmpty().MaximumLength(1000);
    }
}

public sealed class ReferSubmissionCommandHandler : IRequestHandler<ReferSubmissionCommand>
{
    private readonly ISubmissionRepository _submissions;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public ReferSubmissionCommandHandler(
        ISubmissionRepository submissions,
        IUnitOfWork uow,
        ICurrentUser currentUser)
    {
        _submissions = submissions;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(ReferSubmissionCommand request, CancellationToken cancellationToken)
    {
        var submission = await _submissions.GetByIdAsync(request.SubmissionId, cancellationToken)
            ?? throw new DomainException($"Submission {request.SubmissionId} not found.");

        submission.Transition(SubmissionStatus.Referred, _currentUser.UserId);
        submission.AddNote($"Referred: {request.ReferralReason}", _currentUser.UserId);

        _submissions.Update(submission);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
