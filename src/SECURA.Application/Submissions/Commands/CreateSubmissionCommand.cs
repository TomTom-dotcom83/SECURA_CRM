using FluentValidation;
using MediatR;
using SECURA.Application.Common.Interfaces;
using SECURA.Domain.Entities;
using SECURA.Domain.Enums;

namespace SECURA.Application.Submissions.Commands;

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
    private readonly IAgencyRepository _agencies;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public CreateSubmissionCommandHandler(
        ISubmissionRepository submissions,
        IAgencyRepository agencies,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _submissions = submissions;
        _agencies = agencies;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateSubmissionCommand request, CancellationToken cancellationToken)
    {
        var agencyExists = await _agencies.ExistsAsync(request.AgencyId, cancellationToken);
        if (!agencyExists)
            throw new Domain.Common.DomainException($"Agency {request.AgencyId} not found.");

        var submission = Submission.Create(
            request.AgencyId, request.Lob, request.State,
            request.ReceivedDate, request.InsuredName);
        submission.CreatedBy = _currentUser.UserId;

        await _submissions.AddAsync(submission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return submission.Id;
    }
}
