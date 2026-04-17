using FluentValidation;
using MediatR;
using Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;
using Secura.DistributionCrm.BuildingBlocks.Domain.Exceptions;
using Secura.DistributionCrm.Submissions.Application.Abstractions;
using Secura.DistributionCrm.Submissions.Domain;
using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Submissions.Application.Commands;

public sealed record CreateSubmissionCommand(
    Guid AgencyId,
    LobType Lob,
    string State,
    DateTime ReceivedDate,
    string? InsuredName,
    string? Description) : IRequest<Guid>;

public sealed class CreateSubmissionCommandValidator : AbstractValidator<CreateSubmissionCommand>
{
    public CreateSubmissionCommandValidator()
    {
        RuleFor(x => x.AgencyId).NotEmpty();
        RuleFor(x => x.State).NotEmpty().Length(2);
        RuleFor(x => x.ReceivedDate).LessThanOrEqualTo(DateTime.UtcNow.AddDays(1));
    }
}

public sealed class CreateSubmissionCommandHandler : IRequestHandler<CreateSubmissionCommand, Guid>
{
    private readonly ISubmissionRepository _submissions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public CreateSubmissionCommandHandler(
        ISubmissionRepository submissions,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _submissions = submissions;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateSubmissionCommand request, CancellationToken cancellationToken)
    {
        var submission = Submission.Create(
            request.AgencyId, request.Lob, request.State,
            request.ReceivedDate, request.InsuredName);
        submission.CreatedBy = _currentUser.UserId;

        await _submissions.AddAsync(submission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return submission.Id;
    }
}
