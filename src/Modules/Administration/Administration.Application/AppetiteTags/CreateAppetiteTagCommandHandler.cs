using MediatR;
using Secura.DistributionCrm.Submissions.Domain;

namespace Secura.DistributionCrm.Administration.Application.AppetiteTags;

public sealed class CreateAppetiteTagCommandHandler
    : IRequestHandler<CreateAppetiteTagCommand, Guid>
{
    private readonly IAppetiteTagRepository _repository;

    public CreateAppetiteTagCommandHandler(IAppetiteTagRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(
        CreateAppetiteTagCommand request, CancellationToken cancellationToken)
    {
        var tag = AppetiteTag.Create(request.Label, request.Lob, request.State, request.Color);
        await _repository.AddAsync(tag, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return tag.Id;
    }
}
