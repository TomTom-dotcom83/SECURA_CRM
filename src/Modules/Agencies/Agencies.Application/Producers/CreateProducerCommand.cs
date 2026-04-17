using FluentValidation;
using MediatR;
using Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;
using Secura.DistributionCrm.BuildingBlocks.Domain.Exceptions;
using Secura.DistributionCrm.Agencies.Application.Abstractions;
using Secura.DistributionCrm.Agencies.Domain.Producers;

namespace Secura.DistributionCrm.Agencies.Application.Producers;

public sealed record CreateProducerCommand(
    Guid BranchId,
    string Npn,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone) : IRequest<Guid>;

public sealed class CreateProducerCommandValidator : AbstractValidator<CreateProducerCommand>
{
    public CreateProducerCommandValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.Npn).NotEmpty().MaximumLength(20).Matches("^[0-9]+$")
            .WithMessage("NPN must contain only digits.");
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
    }
}

public sealed class CreateProducerCommandHandler : IRequestHandler<CreateProducerCommand, Guid>
{
    private readonly IProducerRepository _producers;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public CreateProducerCommandHandler(
        IProducerRepository producers,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _producers = producers;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateProducerCommand request, CancellationToken cancellationToken)
    {
        if (await _producers.NpnExistsAsync(request.Npn, cancellationToken))
            throw new DomainException($"A producer with NPN {request.Npn} already exists.");

        var producer = Producer.Create(request.BranchId, request.Npn,
            request.FirstName, request.LastName);
        producer.Update(request.FirstName, request.LastName,
            request.Email, request.Phone, _currentUser.UserId);
        producer.CreatedBy = _currentUser.UserId;

        await _producers.AddAsync(producer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return producer.Id;
    }
}
