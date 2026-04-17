namespace Secura.DistributionCrm.BuildingBlocks.Domain.Primitives;

public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    string? DeletedBy { get; set; }
}
