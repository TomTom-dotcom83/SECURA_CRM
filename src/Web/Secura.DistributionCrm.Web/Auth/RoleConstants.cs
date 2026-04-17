namespace Secura.DistributionCrm.Web.Auth;

public static class RoleConstants
{
    public const string CarrierAdmin = "CarrierAdmin";
    public const string SalesManager = "SalesManager";
    public const string MarketingRep = "MarketingRep";
    public const string Underwriter = "Underwriter";
    public const string UnderwritingManager = "UnderwritingManager";
    public const string ClaimsAdjuster = "ClaimsAdjuster";
    public const string ClaimsManager = "ClaimsManager";
    public const string AgencyCoordinator = "AgencyCoordinator";
    public const string ComplianceAnalyst = "ComplianceAnalyst";
    public const string ExecutiveReadOnly = "ExecutiveReadOnly";

    public static readonly string[] AllRoles =
    [
        CarrierAdmin, SalesManager, MarketingRep,
        Underwriter, UnderwritingManager,
        ClaimsAdjuster, ClaimsManager,
        AgencyCoordinator, ComplianceAnalyst, ExecutiveReadOnly
    ];
}
