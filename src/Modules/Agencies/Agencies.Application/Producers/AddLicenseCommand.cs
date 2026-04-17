using FluentValidation;
using MediatR;
using Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;
using Secura.DistributionCrm.BuildingBlocks.Domain.Exceptions;
using Secura.DistributionCrm.Agencies.Application.Abstractions;
using Secura.DistributionCrm.Agencies.Domain.Producers;
using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Agencies.Application.Producers;

public sealed record AddLicenseCommand(
    Guid ProducerId,
    string State,
    LobType Lob,
    LicenseStatus Status,
    DateTime ExpirationDate,
    string? LicenseNumber) : IRequest<Guid>;

public sealed class AddLicenseCommandValidator : AbstractValidator<AddLicenseCommand>
{
    public AddLicenseCommandValidator()
    {
        RuleFor(x => x.ProducerId).NotEmpty();
        RuleFor(x => x.State).NotEmpty().Length(2);
        RuleFor(x => x.ExpirationDate).GreaterThan(DateTime.UtcNow.AddDays(-1))
            .WithMessage("Expiration date must be in the future.");
    }
}

public sealed class AddLicenseCommandHandler : IRequestHandler<AddLicenseCommand, Guid>
{
    private readonly IProducerRepository _producers;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public AddLicenseCommandHandler(
        IProducerRepository producers,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _producers = producers;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(AddLicenseCommand request, CancellationToken cancellationToken)
    {
        var producer = await _producers.GetByIdAsync(request.ProducerId, cancellationToken)
            ?? throw new DomainException($"Producer {request.ProducerId} not found.");

        var license = License.Create(
            request.ProducerId, request.State, request.Lob,
            request.Status, request.ExpirationDate, request.LicenseNumber);
        license.CreatedBy = _currentUser.UserId;

        producer.AddLicense(license);
        _producers.Update(producer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return license.Id;
    }
}
