using Secura.DistributionCrm.BuildingBlocks.Domain.Exceptions;
using Secura.DistributionCrm.BuildingBlocks.Domain.Primitives;
using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Submissions.Domain;

public sealed class AppetiteTag : Entity<Guid>
{
    private AppetiteTag() { }

    private AppetiteTag(Guid id, string label, LobType? lob, string? state, string color) : base(id)
    {
        Label = label;
        Lob = lob;
        State = state;
        Color = color;
    }

    public string Label { get; private set; } = string.Empty;
    public LobType? Lob { get; private set; }
    public string? State { get; private set; }
    public string Color { get; private set; } = "#808080";
    public bool IsActive { get; private set; } = true;

    public static AppetiteTag Create(string label, LobType? lob = null,
        string? state = null, string color = "#808080")
    {
        if (string.IsNullOrWhiteSpace(label))
            throw new DomainException("Appetite tag label is required.");

        return new AppetiteTag(Guid.NewGuid(), label.Trim(), lob, state?.ToUpperInvariant(), color);
    }
}
