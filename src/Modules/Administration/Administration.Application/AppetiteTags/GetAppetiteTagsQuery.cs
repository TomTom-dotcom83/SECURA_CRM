using MediatR;

namespace Secura.DistributionCrm.Administration.Application.AppetiteTags;

public sealed record GetAppetiteTagsQuery : IRequest<IReadOnlyList<AppetiteTagDto>>;

public sealed class GetAppetiteTagsQueryHandler
    : IRequestHandler<GetAppetiteTagsQuery, IReadOnlyList<AppetiteTagDto>>
{
    private readonly IAppetiteTagRepository _repository;

    public GetAppetiteTagsQueryHandler(IAppetiteTagRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<AppetiteTagDto>> Handle(
        GetAppetiteTagsQuery request, CancellationToken cancellationToken)
    {
        var tags = await _repository.GetAllAsync(cancellationToken);
        return tags
            .Select(t => new AppetiteTagDto(t.Id, t.Label, t.Lob, t.State, t.Color, t.IsActive))
            .ToList();
    }
}
