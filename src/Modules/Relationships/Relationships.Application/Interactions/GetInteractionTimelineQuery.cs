using MediatR;
using Secura.DistributionCrm.Relationships.Application.Abstractions;

namespace Secura.DistributionCrm.Relationships.Application.Interactions;

public sealed record GetInteractionTimelineQuery(
    string RelatedEntityType,
    Guid RelatedEntityId,
    int MaxResults = 50) : IRequest<IReadOnlyList<InteractionDto>>;

public sealed class GetInteractionTimelineQueryHandler
    : IRequestHandler<GetInteractionTimelineQuery, IReadOnlyList<InteractionDto>>
{
    private readonly IInteractionRepository _interactions;

    public GetInteractionTimelineQueryHandler(IInteractionRepository interactions)
    {
        _interactions = interactions;
    }

    public async Task<IReadOnlyList<InteractionDto>> Handle(
        GetInteractionTimelineQuery request, CancellationToken cancellationToken)
    {
        var items = await _interactions.GetTimelineAsync(
            request.RelatedEntityType,
            request.RelatedEntityId,
            request.MaxResults,
            cancellationToken);

        return items.Select(i => new InteractionDto
        {
            Id = i.Id,
            RelatedEntityType = i.RelatedEntityType,
            RelatedEntityId = i.RelatedEntityId,
            InteractionType = i.InteractionType,
            Timestamp = i.Timestamp,
            Summary = i.Summary,
            DetailNotes = i.DetailNotes,
            CreatedByUserId = i.CreatedByUserId,
            CreatedByDisplayName = i.CreatedByDisplayName,
            DueDate = i.DueDate,
            IsCompleted = i.IsCompleted
        }).ToList();
    }
}
