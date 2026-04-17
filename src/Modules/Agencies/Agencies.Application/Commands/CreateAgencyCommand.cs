using FluentValidation;
using MediatR;
using Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;
using Secura.DistributionCrm.Agencies.Application.Abstractions;
using Secura.DistributionCrm.Agencies.Domain.Agencies;
using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Agencies.Application.Commands;

public sealed record CreateAgencyCommand(
    string Name,
    AgencyTier Tier,
    string PrimaryState,
    string? Phone,
    string? Email,
    string? Website) : IRequest<Guid>;

public sealed class CreateAgencyCommandValidator : AbstractValidator<CreateAgencyCommand>
{
    public CreateAgencyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PrimaryState).NotEmpty().Length(2).Matches("^[A-Za-z]{2}$");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
    }
}

public sealed class CreateAgencyCommandHandler : IRequestHandler<CreateAgencyCommand, Guid>
{
    private readonly IAgencyRepository _agencies;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public CreateAgencyCommandHandler(
        IAgencyRepository agencies,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _agencies = agencies;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateAgencyCommand request, CancellationToken cancellationToken)
    {
        var agency = Agency.Create(request.Name, request.Tier, request.PrimaryState);
        agency.CreatedBy = _currentUser.UserId;
        agency.Update(request.Name, request.Tier, request.PrimaryState,
            request.Phone, request.Email, request.Website, null, _currentUser.UserId);

        await _agencies.AddAsync(agency, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return agency.Id;
    }
}
